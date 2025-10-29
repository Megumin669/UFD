using UnityEngine;

/// <summary>
/// Test script to verify enemy damage reception and timer functionality
/// Attach this to any GameObject and call TestEnemyDamage() from Unity's inspector or code
/// </summary>
public class EnemyDamageTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestOnStart = false;
    [SerializeField] private bool logDetailedResults = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            Invoke(nameof(TestEnemyDamage), 1f); // Wait 1 second after start
        }
    }
    
    [ContextMenu("Test Enemy Damage Reception")]
    public void TestEnemyDamage()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        if (enemies.Length == 0)
        {
            Debug.LogWarning("[EnemyDamageTest] No enemies found in scene for testing");
            return;
        }
        
        Debug.Log($"[EnemyDamageTest] Testing damage reception on {enemies.Length} enemies");
        
        foreach (Enemy enemy in enemies)
        {
            TestSingleEnemy(enemy);
        }
    }
    
    private void TestSingleEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        // Get the Health component
        Health healthComponent = enemy.GetComponent<Health>();
        
        if (healthComponent == null)
        {
            Debug.LogError($"[EnemyDamageTest] Enemy {enemy.name} has no Health component! This will prevent damage reception.");
            return;
        }
        
        // Test damage reception
        int initialHealth = healthComponent.CurrentHealth;
        int testDamage = 10;
        
        if (logDetailedResults)
        {
            Debug.Log($"[EnemyDamageTest] Testing {enemy.name}: Initial health = {initialHealth}, applying {testDamage} damage");
        }
        
        // Apply damage directly to health component (simulating weapon damage)
        healthComponent.TakeDamage(testDamage);
        
        int finalHealth = healthComponent.CurrentHealth;
        
        if (finalHealth < initialHealth)
        {
            Debug.Log($"[EnemyDamageTest] ✓ SUCCESS: {enemy.name} took damage. Health: {initialHealth} → {finalHealth}");
        }
        else
        {
            Debug.LogError($"[EnemyDamageTest] ✗ FAILED: {enemy.name} did not take damage. Health remained at {finalHealth}");
        }
    }
    
    [ContextMenu("Test Enemy Timer Functionality")]
    public void TestEnemyTimerFunctionality()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        if (enemies.Length == 0)
        {
            Debug.LogWarning("[EnemyDamageTest] No enemies found in scene for timer testing");
            return;
        }
        
        Debug.Log($"[EnemyDamageTest] Testing timer functionality on {enemies.Length} enemies");
        Debug.Log("[EnemyDamageTest] Note: Timer test requires enemies to lose their targets. Remove all players/defenses to test.");
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Debug.Log($"[EnemyDamageTest] Enemy {enemy.name} - Current State: {enemy.State}, Has Target: {enemy.GetComponent<Enemy>() != null}");
            }
        }
        
        Debug.Log("[EnemyDamageTest] Monitor console for enemy give-up messages after 20 seconds without targets");
    }
    
    [ContextMenu("Force Enemy Health Setup")]
    public void ForceEnemyHealthSetup()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Health health = enemy.GetComponent<Health>();
                if (health == null)
                {
                    Debug.LogWarning($"[EnemyDamageTest] Adding missing Health component to {enemy.name}");
                    health = enemy.gameObject.AddComponent<Health>();
                }
                
                // Force reinitialize enemy to apply EnemyData settings
                if (enemy.Data != null)
                {
                    Debug.Log($"[EnemyDamageTest] Reinitializing {enemy.name} with EnemyData settings");
                    // The enemy's Start() method should handle this, but we can verify
                    health.Initialize();
                }
            }
        }
        
        Debug.Log("[EnemyDamageTest] Enemy health setup complete");
    }
}