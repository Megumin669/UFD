using UnityEngine;

/// <summary>
/// Example script showing how to use the Stamina system
/// This can be attached to any GameObject with a Stamina component for testing
/// </summary>
public class StaminaTesterExample : MonoBehaviour
{
    [Header("Stamina Testing")]
    [SerializeField] private bool enableKeyboardTesting = true;
    
    [Header("Test Values")]
    [Range(1, 50)] public int staminaConsumeAmount = 10;
    [Range(1, 50)] public int staminaRestoreAmount = 15;
    [Range(1, 20)] public float continuousConsumeRate = 5f;
    
    [Header("Continuous Testing")]
    [SerializeField] private bool isContinuousConsume = false;
    
    private Stamina staminaComponent;
    
    void Start()
    {
        staminaComponent = GetComponent<Stamina>();
        
        if (staminaComponent == null)
        {
            Debug.LogError($"[{gameObject.name}] StaminaTesterExample: No Stamina component found!");
            enabled = false;
            return;
        }
        
        // Subscribe to stamina events for testing
        staminaComponent.OnStaminaChanged.AddListener(OnStaminaChanged);
        staminaComponent.OnStaminaConsumed.AddListener(OnStaminaConsumed);
        staminaComponent.OnStaminaRestored.AddListener(OnStaminaRestored);
        staminaComponent.OnStaminaExhausted.AddListener(OnStaminaExhausted);
        staminaComponent.OnStaminaRecovered.AddListener(OnStaminaRecovered);
        staminaComponent.OnRegenerationStarted.AddListener(OnRegenerationStarted);
        staminaComponent.OnRegenerationStopped.AddListener(OnRegenerationStopped);
        
        Debug.Log($"[{gameObject.name}] Stamina Tester initialized - Use keys:\n" +
                 $"Q (consume), E (restore), X (exhaust), C (full restore), V (toggle continuous)\n" +
                 $"B (toggle exhaustion), I (status), U (toggle console debug), Y (force log)\n" +
                 $"J (jump test), M (magic attack test)");
        LogCurrentStatus();
    }
    
    void Update()
    {
        if (!enableKeyboardTesting || staminaComponent == null) return;
        
        // Test keys (only in editor/development)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"[TEST] Consuming {staminaConsumeAmount} stamina");
            staminaComponent.ConsumeStamina(staminaConsumeAmount);
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[TEST] Restoring {staminaRestoreAmount} stamina");
            staminaComponent.RestoreStamina(staminaRestoreAmount);
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log($"[TEST] Exhausting character (set stamina to 0)");
            staminaComponent.DEBUG_SetStamina(0);
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"[TEST] Full stamina restore");
            staminaComponent.FullRestore();
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            isContinuousConsume = !isContinuousConsume;
            Debug.Log($"[TEST] Continuous stamina consume: {(isContinuousConsume ? "ON" : "OFF")}");
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"[TEST] Toggle exhaustion state");
            staminaComponent.DEBUG_ToggleExhaustion();
        }
        
        // Status display key
        if (Input.GetKeyDown(KeyCode.I))
        {
            LogCurrentStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log($"[TEST] Toggling console debug");
            staminaComponent.ToggleConsoleDebug();
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log($"[TEST] Force logging current stamina status");
            staminaComponent.LogCurrentStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            var staminaStats = staminaComponent.GetStaminaStats();
            Debug.Log($"[TEST] Testing jump stamina consumption ({staminaStats.jumpCost} units)");
            if (!staminaComponent.ConsumeStamina(staminaStats.jumpCost))
            {
                Debug.Log($"[TEST] Jump failed - insufficient stamina!");
            }
            else
            {
                Debug.Log($"[TEST] Jump successful - stamina consumed");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            var staminaStats = staminaComponent.GetStaminaStats();
            Debug.Log($"[TEST] Testing magic attack stamina consumption ({staminaStats.magicAttackCost} units)");
            if (!staminaComponent.ConsumeStamina(staminaStats.magicAttackCost))
            {
                Debug.Log($"[TEST] Magic attack failed - insufficient stamina!");
            }
            else
            {
                Debug.Log($"[TEST] Magic attack successful - stamina consumed");
            }
        }
        
        // Continuous stamina consumption test
        if (isContinuousConsume)
        {
            staminaComponent.ConsumeStaminaOverTime(continuousConsumeRate);
        }
    }
    
    // Event handlers for demonstration
    private void OnStaminaChanged(int currentStamina, int maxStamina)
    {
        Debug.Log($"[{gameObject.name}] Stamina Changed: {currentStamina}/{maxStamina} ({staminaComponent.StaminaPercentage:P0})");
    }
    
    private void OnStaminaConsumed(int consumed, int remaining)
    {
        Debug.Log($"[{gameObject.name}] Stamina Consumed: -{consumed} (Remaining: {remaining})");
    }
    
    private void OnStaminaRestored(int restored)
    {
        Debug.Log($"[{gameObject.name}] Stamina Restored: +{restored}");
    }
    
    private void OnStaminaExhausted()
    {
        Debug.Log($"[{gameObject.name}] CHARACTER EXHAUSTED!");
    }
    
    private void OnStaminaRecovered()
    {
        Debug.Log($"[{gameObject.name}] CHARACTER RECOVERED FROM EXHAUSTION!");
    }
    
    private void OnRegenerationStarted()
    {
        Debug.Log($"[{gameObject.name}] Stamina regeneration started");
    }
    
    private void OnRegenerationStopped()
    {
        Debug.Log($"[{gameObject.name}] Stamina regeneration stopped");
    }
    
    private void LogCurrentStatus()
    {
        if (staminaComponent == null) return;
        
        var stats = staminaComponent.GetStaminaStats();
        Debug.Log($"[{gameObject.name}] STAMINA STATUS:\n" +
                 $"Current: {staminaComponent.CurrentStamina}/{staminaComponent.MaxStamina} ({staminaComponent.StaminaPercentage:P0})\n" +
                 $"Exhausted: {staminaComponent.IsExhausted}\n" +
                 $"Regenerating: {staminaComponent.IsRegenerating}\n" +
                 $"Can Perform Action: {staminaComponent.CanPerformAction}\n" +
                 $"Regeneration Enabled: {stats.regenerationEnabled}\n" +
                 $"Regen Rate: {stats.regenerationRate}/sec\n" +
                 $"Melee Attack Cost: {stats.meleeAttackCost}\n" +
                 $"Sprint Cost: {stats.sprintCostPerSecond}/sec");
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (staminaComponent != null)
        {
            staminaComponent.OnStaminaChanged.RemoveListener(OnStaminaChanged);
            staminaComponent.OnStaminaConsumed.RemoveListener(OnStaminaConsumed);
            staminaComponent.OnStaminaRestored.RemoveListener(OnStaminaRestored);
            staminaComponent.OnStaminaExhausted.RemoveListener(OnStaminaExhausted);
            staminaComponent.OnStaminaRecovered.RemoveListener(OnStaminaRecovered);
            staminaComponent.OnRegenerationStarted.RemoveListener(OnRegenerationStarted);
            staminaComponent.OnRegenerationStopped.RemoveListener(OnRegenerationStopped);
        }
    }
    
    // Public methods for external testing (UI buttons, etc.)
    public void TestConsume() => staminaComponent?.ConsumeStamina(staminaConsumeAmount);
    public void TestRestore() => staminaComponent?.RestoreStamina(staminaRestoreAmount);
    public void TestExhaust() => staminaComponent?.DEBUG_SetStamina(0);
    public void TestFullRestore() => staminaComponent?.FullRestore();
    public void ToggleContinuousConsume() 
    {
        isContinuousConsume = !isContinuousConsume;
        Debug.Log($"Continuous consume: {(isContinuousConsume ? "ON" : "OFF")}");
    }
}