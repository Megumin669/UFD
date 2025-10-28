using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Modular stamina system for managing character energy/endurance
/// Can be used for any character type (player, enemies, NPCs)
/// Handles stamina consumption, regeneration, and exhaustion states
/// </summary>
public class Stamina : MonoBehaviour
{
    [Header("Stamina Configuration")]
    [SerializeField] private StaminaStats staminaStats = new StaminaStats();
    
    [Header("Debug Information")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool enableConsoleDebug = false;
    [SerializeField] private float debugUpdateInterval = 1f;
    [SerializeField] private int currentStamina;
    [SerializeField] private bool isRegenerating;
    [SerializeField] private bool isExhausted;
    
    // Debug variables
    private float debugTimer;
    
    // Private variables
    private float regenerationTimer;
    private bool canRegenerate = true;
    private float accumulatedStaminaDelta = 0f; // For precise over-time calculations
    
    // Events for external systems to subscribe to
    [Header("Stamina Events")]
    public UnityEvent<int, int> OnStaminaChanged;        // (current, max)
    public UnityEvent<int, int> OnStaminaConsumed;       // (consumed, remaining)
    public UnityEvent<int> OnStaminaRestored;            // (restored amount)
    public UnityEvent OnStaminaExhausted;                // When stamina reaches 0
    public UnityEvent OnStaminaRecovered;                // When stamina recovers from exhaustion
    public UnityEvent OnRegenerationStarted;             // When stamina regeneration begins
    public UnityEvent OnRegenerationStopped;             // When stamina regeneration stops
    
    // Public properties for external access
    public int CurrentStamina => currentStamina;
    public int MaxStamina => staminaStats.maxStamina;
    public bool IsExhausted => isExhausted;
    public bool IsRegenerating => isRegenerating;
    public float StaminaPercentage => MaxStamina > 0 ? (float)currentStamina / MaxStamina : 0f;
    public bool CanPerformAction => currentStamina > 0 && !isExhausted;
    
    void Awake()
    {
        InitializeStamina();
    }
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Stamina System initialized: {currentStamina}/{MaxStamina}");
        }
    }
    
    void Update()
    {
        HandleStaminaRegeneration();
        HandleDebugConsole();
    }
    
    #region Initialization
    
    /// <summary>
    /// Initialize stamina system with configured values
    /// </summary>
    void InitializeStamina()
    {
        currentStamina = staminaStats.startingStamina;
        isExhausted = false;
        isRegenerating = false;
        regenerationTimer = 0f;
        canRegenerate = true;
        
        // Ensure starting stamina doesn't exceed max
        if (currentStamina > MaxStamina)
        {
            currentStamina = MaxStamina;
        }
        
        // Trigger initial event
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
    }
    
    #endregion
    
    #region Stamina Consumption
    
    /// <summary>
    /// Consume stamina for actions like attacks or sprinting
    /// </summary>
    /// <param name="amount">Amount of stamina to consume</param>
    /// <returns>True if stamina was consumed, false if insufficient stamina</returns>
    public bool ConsumeStamina(int amount)
    {
        if (amount <= 0) return true;
        
        // Check if we have enough stamina
        if (currentStamina < amount && staminaStats.preventActionOnLowStamina)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] Insufficient stamina: {currentStamina}/{amount} required");
            }
            return false;
        }
        
        // Consume stamina
        int previousStamina = currentStamina;
        currentStamina = Mathf.Max(0, currentStamina - amount);
        
        // Stop regeneration temporarily
        StopRegeneration();
        
        // Check for exhaustion
        if (currentStamina == 0 && !isExhausted)
        {
            SetExhausted(true);
        }
        
        // Trigger events
        OnStaminaConsumed?.Invoke(amount, currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        
        // Debug logging
        LogStaminaChange("CONSUMED", -amount, previousStamina, currentStamina);
        
        return true;
    }
    
    /// <summary>
    /// Consume stamina over time (for sprinting, channeling, etc.)
    /// </summary>
    /// <param name="amountPerSecond">Stamina consumption rate per second</param>
    /// <returns>True if stamina was consumed, false if exhausted</returns>
    public bool ConsumeStaminaOverTime(float amountPerSecond)
    {
        if (amountPerSecond <= 0) return true;
        
        // Accumulate fractional stamina consumption for precision
        accumulatedStaminaDelta -= amountPerSecond * Time.deltaTime;
        
        // Only consume whole stamina points when accumulated
        if (accumulatedStaminaDelta <= -1f)
        {
            int staminaToConsume = Mathf.FloorToInt(-accumulatedStaminaDelta);
            accumulatedStaminaDelta += staminaToConsume; // Keep the remainder
            
            return ConsumeStamina(staminaToConsume);
        }
        
        return true; // No stamina consumed yet, but action can continue
    }
    
    /// <summary>
    /// Check if character has enough stamina for an action
    /// </summary>
    /// <param name="requiredAmount">Required stamina amount</param>
    /// <returns>True if sufficient stamina available</returns>
    public bool HasSufficientStamina(int requiredAmount)
    {
        return currentStamina >= requiredAmount;
    }
    
    #endregion
    
    #region Stamina Restoration
    
    /// <summary>
    /// Restore stamina (for potions, rest, etc.)
    /// </summary>
    /// <param name="amount">Amount of stamina to restore</param>
    public void RestoreStamina(int amount)
    {
        if (amount <= 0) return;
        
        int previousStamina = currentStamina;
        currentStamina = Mathf.Min(MaxStamina, currentStamina + amount);
        
        // Check if recovered from exhaustion
        if (isExhausted && currentStamina > 0)
        {
            SetExhausted(false);
        }
        
        // Trigger events
        OnStaminaRestored?.Invoke(amount);
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        
        // Debug logging
        LogStaminaChange("RESTORED", amount, previousStamina, currentStamina);
    }
    
    /// <summary>
    /// Fully restore stamina to maximum
    /// </summary>
    public void FullRestore()
    {
        int restoredAmount = MaxStamina - currentStamina;
        if (restoredAmount > 0)
        {
            RestoreStamina(restoredAmount);
        }
    }
    
    #endregion
    
    #region Stamina Regeneration
    
    /// <summary>
    /// Handle automatic stamina regeneration
    /// </summary>
    void HandleStaminaRegeneration()
    {
        if (!staminaStats.regenerationEnabled || currentStamina >= MaxStamina)
        {
            if (isRegenerating)
            {
                SetRegenerating(false);
            }
            return;
        }
        
        // Handle regeneration delay
        if (!canRegenerate)
        {
            regenerationTimer += Time.deltaTime;
            if (regenerationTimer >= staminaStats.regenerationDelay)
            {
                canRegenerate = true;
                regenerationTimer = 0f;
            }
            return;
        }
        
        // Start regeneration if not already regenerating
        if (!isRegenerating)
        {
            SetRegenerating(true);
        }
        
        // Regenerate stamina using accumulation for precision
        accumulatedStaminaDelta += staminaStats.regenerationRate * Time.deltaTime;
        
        // Only regenerate whole stamina points when accumulated
        if (accumulatedStaminaDelta >= 1f)
        {
            int staminaToAdd = Mathf.FloorToInt(accumulatedStaminaDelta);
            accumulatedStaminaDelta -= staminaToAdd; // Keep the remainder
            
            RestoreStamina(staminaToAdd);
        }
    }
    
    /// <summary>
    /// Stop stamina regeneration (called when stamina is consumed)
    /// </summary>
    void StopRegeneration()
    {
        canRegenerate = false;
        regenerationTimer = 0f;
        accumulatedStaminaDelta = 0f; // Reset accumulator when regeneration stops
        
        if (isRegenerating)
        {
            SetRegenerating(false);
        }
    }
    
    /// <summary>
    /// Set regeneration state and trigger events
    /// </summary>
    /// <param name="regenerating">New regeneration state</param>
    void SetRegenerating(bool regenerating)
    {
        if (isRegenerating != regenerating)
        {
            isRegenerating = regenerating;
            
            if (regenerating)
            {
                OnRegenerationStarted?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] Stamina regeneration started");
                }
            }
            else
            {
                OnRegenerationStopped?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] Stamina regeneration stopped");
                }
            }
        }
    }
    
    #endregion
    
    #region Exhaustion System
    
    /// <summary>
    /// Set exhaustion state and trigger events
    /// </summary>
    /// <param name="exhausted">New exhaustion state</param>
    void SetExhausted(bool exhausted)
    {
        if (isExhausted != exhausted)
        {
            isExhausted = exhausted;
            
            if (exhausted)
            {
                OnStaminaExhausted?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] CHARACTER EXHAUSTED!");
                }
            }
            else
            {
                OnStaminaRecovered?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] CHARACTER RECOVERED FROM EXHAUSTION!");
                }
            }
        }
    }
    
    #endregion
    
    #region Configuration
    
    /// <summary>
    /// Update stamina configuration at runtime
    /// </summary>
    /// <param name="newStats">New stamina configuration</param>
    public void UpdateStaminaStats(StaminaStats newStats)
    {
        staminaStats = newStats;
        
        // Ensure current stamina doesn't exceed new max
        if (currentStamina > MaxStamina)
        {
            currentStamina = MaxStamina;
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        }
    }
    
    /// <summary>
    /// Get current stamina configuration
    /// </summary>
    /// <returns>Current stamina stats</returns>
    public StaminaStats GetStaminaStats()
    {
        return staminaStats;
    }
    
    #endregion
    
    #region Debug Console System
    
    /// <summary>
    /// Handle console debug output
    /// </summary>
    void HandleDebugConsole()
    {
        if (!enableConsoleDebug) return;
        
        debugTimer += Time.deltaTime;
        if (debugTimer >= debugUpdateInterval)
        {
            debugTimer = 0f;
            LogStaminaStatus();
        }
    }
    
    /// <summary>
    /// Log current stamina status to console
    /// </summary>
    void LogStaminaStatus()
    {
        string status = $"[STAMINA DEBUG] {gameObject.name}\n" +
                       $"Current: {currentStamina}/{MaxStamina} ({StaminaPercentage:P1})\n" +
                       $"Exhausted: {isExhausted} | Regenerating: {isRegenerating}\n" +
                       $"Can Act: {CanPerformAction} | Regen Rate: {staminaStats.regenerationRate}/sec";
        
        // Color coding based on stamina level
        if (isExhausted)
        {
            Debug.LogWarning(status);
        }
        else if (StaminaPercentage < 0.3f)
        {
            Debug.LogWarning(status);
        }
        else
        {
            Debug.Log(status);
        }
    }
    
    /// <summary>
    /// Log stamina change with details
    /// </summary>
    void LogStaminaChange(string action, int amount, int before, int after)
    {
        if (showDebugInfo || enableConsoleDebug)
        {
            string change = amount > 0 ? $"+{amount}" : $"{amount}";
            Debug.Log($"[STAMINA] {gameObject.name} {action}: {change} ({before} → {after})");
        }
    }
    
    /// <summary>
    /// Toggle console debug at runtime
    /// </summary>
    public void ToggleConsoleDebug()
    {
        enableConsoleDebug = !enableConsoleDebug;
        Debug.Log($"[STAMINA DEBUG] Console debug {(enableConsoleDebug ? "ENABLED" : "DISABLED")} for {gameObject.name}");
        
        if (enableConsoleDebug)
        {
            LogStaminaStatus();
        }
    }
    
    /// <summary>
    /// Force log current stamina status
    /// </summary>
    public void LogCurrentStatus()
    {
        LogStaminaStatus();
    }
    
    #endregion
    
    #region Debug Methods
    
    /// <summary>
    /// Debug method to set stamina directly
    /// </summary>
    /// <param name="amount">Stamina amount to set</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DEBUG_SetStamina(int amount)
    {
        currentStamina = Mathf.Clamp(amount, 0, MaxStamina);
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        
        if (showDebugInfo)
        {
            Debug.Log($"[DEBUG] Stamina set to: {currentStamina}/{MaxStamina}");
        }
    }
    
    /// <summary>
    /// Debug method to toggle exhaustion state
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DEBUG_ToggleExhaustion()
    {
        if (isExhausted)
        {
            currentStamina = MaxStamina;
            SetExhausted(false);
        }
        else
        {
            currentStamina = 0;
            SetExhausted(true);
        }
        
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
    }
    
    #endregion
}

/// <summary>
/// Configurable stamina statistics and settings
/// </summary>
[System.Serializable]
public class StaminaStats
{
    [Header("Basic Stamina Settings")]
    [Tooltip("Maximum stamina points")]
    [Range(10, 500)] public int maxStamina = 100;
    
    [Tooltip("Starting stamina at initialization (defaults to max if 0)")]
    [Range(0, 500)] public int startingStamina = 100;
    
    [Header("Regeneration Settings")]
    [Tooltip("Enable automatic stamina regeneration")]
    public bool regenerationEnabled = true;
    
    [Tooltip("Stamina points restored per second")]
    [Range(0.1f, 50f)] public float regenerationRate = 10f;
    
    [Tooltip("Delay after consumption before regeneration begins")]
    [Range(0f, 10f)] public float regenerationDelay = 2f;
    
    [Header("Action Settings")]
    [Tooltip("Prevent actions when stamina is insufficient")]
    public bool preventActionOnLowStamina = true;
    
    [Header("Combat Stamina Costs")]
    [Tooltip("Stamina cost for melee attacks")]
    [Range(0, 50)] public int meleeAttackCost = 10;
    
    [Tooltip("Stamina cost for ranged attacks")]
    [Range(0, 30)] public int rangedAttackCost = 5;
    
    [Tooltip("Stamina cost for magic attacks")]
    [Range(0, 40)] public int magicAttackCost = 15;
    
    [Header("Movement Stamina Costs")]
    [Tooltip("Stamina consumption per second while sprinting")]
    [Range(0f, 20f)] public float sprintCostPerSecond = 5f;
    
    [Tooltip("Stamina cost for jumping")]
    [Range(0, 50)] public int jumpCost = 15;
    
    [Tooltip("Stamina cost for dodging/rolling")]
    [Range(0, 30)] public int dodgeCost = 15;
    
    /// <summary>
    /// Constructor with default values
    /// </summary>
    public StaminaStats()
    {
        // Set default starting stamina to max if not specified
        if (startingStamina <= 0)
        {
            startingStamina = maxStamina;
        }
    }
}