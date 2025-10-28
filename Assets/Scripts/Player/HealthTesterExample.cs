using UnityEngine;

/// <summary>
/// Example script showing how to use the Health system
/// This can be attached to any GameObject with a Health component for testing
/// </summary>
public class HealthTesterExample : MonoBehaviour
{
    [Header("Health Testing")]
    [SerializeField] private bool enableKeyboardTesting = true;
    
    [Header("Test Values")]
    [Range(1, 100)] public int damageAmount = 10;
    [Range(1, 100)] public int healAmount = 15;
    
    private Health healthComponent;
    
    void Start()
    {
        healthComponent = GetComponent<Health>();
        
        if (healthComponent == null)
        {
            Debug.LogError($"[{gameObject.name}] HealthTesterExample: No Health component found!");
            enabled = false;
            return;
        }
        
        // Subscribe to health events for testing
        healthComponent.OnHealthChanged += OnHealthChanged;
        healthComponent.OnDamageTaken += OnDamageTaken;
        healthComponent.OnHealthHealed += OnHealthHealed;
        healthComponent.OnDeath += OnDeath;
        healthComponent.OnRevived += OnRevived;
        
        Debug.Log($"[{gameObject.name}] Health Tester initialized - Use keys: T (damage), H (heal), K (kill), R (revive), F (full heal)");
    }
    
    void Update()
    {
        if (!enableKeyboardTesting || healthComponent == null) return;
        
        // Test keys (only in editor/development)
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"[TEST] Taking {damageAmount} damage");
            healthComponent.TakeDamage(damageAmount);
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"[TEST] Healing {healAmount} health");
            healthComponent.Heal(healAmount);
        }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log($"[TEST] Killing character");
            healthComponent.Kill();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"[TEST] Reviving character");
            healthComponent.Revive();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"[TEST] Full heal");
            healthComponent.FullHeal();
        }
    }
    
    // Event handlers for demonstration
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"[{gameObject.name}] Health Changed: {currentHealth}/{maxHealth} ({healthComponent.HealthPercentage:P0})");
    }
    
    private void OnDamageTaken(int damageAmount, int remainingHealth)
    {
        Debug.Log($"[{gameObject.name}] Damage Taken: -{damageAmount} (Remaining: {remainingHealth})");
    }
    
    private void OnHealthHealed(int healAmount)
    {
        Debug.Log($"[{gameObject.name}] Health Healed: +{healAmount}");
    }
    
    private void OnDeath()
    {
        Debug.Log($"[{gameObject.name}] CHARACTER DIED!");
    }
    
    private void OnRevived()
    {
        Debug.Log($"[{gameObject.name}] CHARACTER REVIVED!");
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= OnHealthChanged;
            healthComponent.OnDamageTaken -= OnDamageTaken;
            healthComponent.OnHealthHealed -= OnHealthHealed;
            healthComponent.OnDeath -= OnDeath;
            healthComponent.OnRevived -= OnRevived;
        }
    }
    
    // Public methods for external testing
    public void TestDamage() => healthComponent?.TakeDamage(damageAmount);
    public void TestHeal() => healthComponent?.Heal(healAmount);
    public void TestKill() => healthComponent?.Kill();
    public void TestRevive() => healthComponent?.Revive();
    public void TestFullHeal() => healthComponent?.FullHeal();
}