using UnityEngine;

[System.Serializable]
public class HealthStats
{
    [Header("Health Configuration")]
    [Tooltip("Maximum health points")]
    [Range(1, 1000)] public int maxHealth = 100;
    
    [Tooltip("Starting health (if different from max)")]
    [Range(1, 1000)] public int startingHealth = 100;
    
    [Header("Health Regeneration")]
    [Tooltip("Enable automatic health regeneration")]
    public bool enableRegeneration = false;
    
    [Tooltip("Health points regenerated per second")]
    [Range(0.1f, 50f)] public float regenerationRate = 1f;
    
    [Tooltip("Delay before regeneration starts after taking damage")]
    [Range(0f, 10f)] public float regenerationDelay = 3f;
    
    [Header("Damage Resistance")]
    [Tooltip("Damage reduction percentage (0 = no reduction, 0.5 = 50% reduction)")]
    [Range(0f, 0.9f)] public float damageResistance = 0f;
    
    [Tooltip("Minimum damage that can be dealt (ignores resistance)")]
    [Range(1, 10)] public int minimumDamage = 1;
}

public class Health : MonoBehaviour
{
    [Header("Health System")]
    public HealthStats healthStats = new HealthStats();
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private bool showHealthInName = false;
    
    // Current health state
    private int currentHealth;
    private float lastDamageTime;
    private bool isDead = false;
    
    // Events
    public System.Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public System.Action<int, int> OnDamageTaken; // (damageAmount, remainingHealth)
    public System.Action<int> OnHealthHealed; // (healAmount)
    public System.Action OnDeath;
    public System.Action OnRevived;
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        if (healthStats.enableRegeneration && !isDead)
        {
            HandleRegeneration();
        }
        
        if (showHealthInName)
        {
            UpdateNameDisplay();
        }
    }
    
    /// <summary>
    /// Initialize the health system with starting values
    /// </summary>
    public void Initialize()
    {
        currentHealth = healthStats.startingHealth;
        isDead = false;
        lastDamageTime = -healthStats.regenerationDelay;
        
        // Clamp starting health to max health
        currentHealth = Mathf.Clamp(currentHealth, 1, healthStats.maxHealth);
        
        if (showDebugMessages)
        {
            Debug.Log($"[{gameObject.name}] Health System Initialized - Health: {currentHealth}/{healthStats.maxHealth}");
        }
        
        OnHealthChanged?.Invoke(currentHealth, healthStats.maxHealth);
    }
    
    /// <summary>
    /// Take damage and reduce health
    /// </summary>
    /// <param name="damageAmount">Raw damage amount before resistance</param>
    /// <returns>Actual damage dealt after resistance</returns>
    public int TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0) return 0;
        
        // Calculate damage after resistance
        float resistanceMultiplier = 1f - healthStats.damageResistance;
        int finalDamage = Mathf.RoundToInt(damageAmount * resistanceMultiplier);
        
        // Ensure minimum damage
        finalDamage = Mathf.Max(finalDamage, healthStats.minimumDamage);
        
        // Apply damage
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        lastDamageTime = Time.time;
        
        // Debug output
        if (showDebugMessages)
        {
            string resistanceInfo = healthStats.damageResistance > 0 ? 
                $" (Resisted: {damageAmount - finalDamage}, Final: {finalDamage})" : "";
            Debug.Log($"[{gameObject.name}] Took {finalDamage} damage{resistanceInfo} - Health: {currentHealth}/{healthStats.maxHealth}");
        }
        
        // Trigger events
        OnDamageTaken?.Invoke(finalDamage, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, healthStats.maxHealth);
        
        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            HandleDeath();
        }
        
        return finalDamage;
    }
    
    /// <summary>
    /// Heal health points
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    /// <returns>Actual amount healed</returns>
    public int Heal(int healAmount)
    {
        if (isDead || healAmount <= 0) return 0;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(healthStats.maxHealth, currentHealth + healAmount);
        int actualHealAmount = currentHealth - previousHealth;
        
        if (actualHealAmount > 0)
        {
            if (showDebugMessages)
            {
                Debug.Log($"[{gameObject.name}] Healed {actualHealAmount} health - Health: {currentHealth}/{healthStats.maxHealth}");
            }
            
            OnHealthHealed?.Invoke(actualHealAmount);
            OnHealthChanged?.Invoke(currentHealth, healthStats.maxHealth);
        }
        
        return actualHealAmount;
    }
    
    /// <summary>
    /// Set health to a specific value
    /// </summary>
    /// <param name="newHealth">New health value</param>
    public void SetHealth(int newHealth)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0, healthStats.maxHealth);
        
        if (showDebugMessages && currentHealth != previousHealth)
        {
            Debug.Log($"[{gameObject.name}] Health set to {currentHealth}/{healthStats.maxHealth}");
        }
        
        OnHealthChanged?.Invoke(currentHealth, healthStats.maxHealth);
        
        // Handle death/revival
        if (currentHealth <= 0 && !isDead)
        {
            HandleDeath();
        }
        else if (currentHealth > 0 && isDead)
        {
            HandleRevival();
        }
    }
    
    /// <summary>
    /// Restore health to maximum
    /// </summary>
    public void FullHeal()
    {
        SetHealth(healthStats.maxHealth);
        
        if (showDebugMessages)
        {
            Debug.Log($"[{gameObject.name}] Fully healed - Health: {currentHealth}/{healthStats.maxHealth}");
        }
    }
    
    /// <summary>
    /// Kill the character instantly
    /// </summary>
    public void Kill()
    {
        SetHealth(0);
    }
    
    /// <summary>
    /// Revive the character with specified health
    /// </summary>
    /// <param name="reviveHealth">Health to revive with (default: max health)</param>
    public void Revive(int reviveHealth = -1)
    {
        if (reviveHealth < 0) reviveHealth = healthStats.maxHealth;
        
        isDead = false;
        SetHealth(reviveHealth);
        
        if (showDebugMessages)
        {
            Debug.Log($"[{gameObject.name}] Revived with {currentHealth} health");
        }
        
        OnRevived?.Invoke();
    }
    
    private void HandleRegeneration()
    {
        if (currentHealth >= healthStats.maxHealth) return;
        if (Time.time - lastDamageTime < healthStats.regenerationDelay) return;
        
        float regenAmount = healthStats.regenerationRate * Time.deltaTime;
        int healAmount = Mathf.RoundToInt(regenAmount);
        
        if (healAmount > 0)
        {
            Heal(healAmount);
        }
    }
    
    private void HandleDeath()
    {
        isDead = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"[{gameObject.name}] DIED - Health reached 0");
        }
        
        OnDeath?.Invoke();
    }
    
    private void HandleRevival()
    {
        isDead = false;
        
        if (showDebugMessages)
        {
            Debug.Log($"[{gameObject.name}] REVIVED - Health restored");
        }
        
        OnRevived?.Invoke();
    }
    
    private void UpdateNameDisplay()
    {
        if (!gameObject.name.Contains($"({currentHealth}"))
        {
            string baseName = gameObject.name.Split('(')[0].Trim();
            gameObject.name = $"{baseName} ({currentHealth}/{healthStats.maxHealth})";
        }
    }
    
    // Public getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => healthStats.maxHealth;
    public float HealthPercentage => (float)currentHealth / healthStats.maxHealth;
    public bool IsDead => isDead;
    public bool IsFullHealth => currentHealth >= healthStats.maxHealth;
    public bool IsLowHealth => HealthPercentage <= 0.25f; // 25% or less
    
    // Debug methods
    [ContextMenu("Take 10 Damage")]
    public void DebugTakeDamage() => TakeDamage(10);
    
    [ContextMenu("Heal 25 Health")]
    public void DebugHeal() => Heal(25);
    
    [ContextMenu("Full Heal")]
    public void DebugFullHeal() => FullHeal();
    
    [ContextMenu("Kill")]
    public void DebugKill() => Kill();
    
    [ContextMenu("Revive")]
    public void DebugRevive() => Revive();
    
    // Validation
    void OnValidate()
    {
        if (healthStats.startingHealth > healthStats.maxHealth)
        {
            healthStats.startingHealth = healthStats.maxHealth;
        }
        
        if (healthStats.startingHealth < 1)
        {
            healthStats.startingHealth = 1;
        }
    }
}