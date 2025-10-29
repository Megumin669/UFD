using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Configuration for a single enemy type within a wave
/// </summary>
[System.Serializable]
public class WaveEnemyConfig
{
    [Tooltip("Enemy type to spawn")]
    public EnemyData enemyData;
    
    [Tooltip("Number of this enemy type to spawn in the wave")]
    [Range(1, 20)] public int count = 1;
    
    [Tooltip("Weight for random selection (higher = more likely)")]
    [Range(0.1f, 5f)] public float spawnWeight = 1f;
    
    [Tooltip("Minimum wave number for this enemy to appear")]
    [Range(1, 100)] public int minWaveNumber = 1;
    
    [Tooltip("Maximum wave number for this enemy to appear (0 = no limit)")]
    [Range(0, 100)] public int maxWaveNumber = 0;
}

/// <summary>
/// Configuration for a complete wave of enemies
/// </summary>
[System.Serializable]
public class WaveConfiguration
{
    [Tooltip("Wave number (for identification)")]
    public int waveNumber = 1;
    
    [Tooltip("Custom name for this wave (optional)")]
    public string waveName = "";
    
    [Tooltip("Enemy types and counts for this wave")]
    public WaveEnemyConfig[] enemies;
    
    [Tooltip("Time between enemy spawns in this wave (seconds)")]
    [Range(0.1f, 10f)] public float spawnInterval = 2f;
    
    [Tooltip("Is this a boss wave?")]
    public bool isBossWave = false;
    
    [Tooltip("Special effects or modifiers for this wave")]
    public string description = "";
    
    /// <summary>
    /// Get total enemy count for this wave
    /// </summary>
    public int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var enemy in enemies)
        {
            total += enemy.count;
        }
        return total;
    }
    
    /// <summary>
    /// Get valid enemies for a specific wave number
    /// </summary>
    public List<WaveEnemyConfig> GetValidEnemiesForWave(int currentWaveNumber)
    {
        List<WaveEnemyConfig> validEnemies = new List<WaveEnemyConfig>();
        
        foreach (var enemy in enemies)
        {
            if (enemy.enemyData == null) continue;
            
            // Check wave number constraints
            if (currentWaveNumber < enemy.minWaveNumber) continue;
            if (enemy.maxWaveNumber > 0 && currentWaveNumber > enemy.maxWaveNumber) continue;
            
            validEnemies.Add(enemy);
        }
        
        return validEnemies;
    }
}

/// <summary>
/// Manages enemy spawning using EnemyData configurations.
/// Supports wave-based spawning, difficulty scaling, and special events.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [Tooltip("Spawn points where enemies can appear")]
    public Transform[] spawnPoints;
    
    [Tooltip("Maximum number of enemies alive at once")]
    [Range(1, 100)] public int maxConcurrentEnemies = 20;
    
    [Header("Wave Configuration")]
    [Tooltip("Enable wave-based spawning")]
    public bool useWaveSystem = true;
    
    [Tooltip("Time between waves (seconds)")]
    [Range(5f, 300f)] public float timeBetweenWaves = 60f;
    
    [Tooltip("Wave configurations - each wave can have different enemy types")]
    public WaveConfiguration[] waveConfigurations;
    
    [Tooltip("Fallback enemy types for procedural waves beyond configured ones")]
    public EnemyData[] fallbackEnemies;
    
    [Header("Procedural Wave Settings")]
    [Tooltip("Base enemies per procedural wave (used when no wave config exists)")]
    [Range(1, 50)] public int baseEnemiesPerWave = 5;
    
    [Tooltip("Enemy count increase per wave for procedural waves")]
    [Range(0f, 5f)] public float enemyScalingPerWave = 1.2f;
    
    [Header("Difficulty Scaling")]
    [Tooltip("Enable dynamic difficulty scaling")]
    public bool useDifficultyScaling = true;
    
    [Tooltip("Health multiplier per wave")]
    [Range(1f, 3f)] public float healthScalingPerWave = 1.1f;
    
    [Tooltip("Damage multiplier per wave")]
    [Range(1f, 3f)] public float damageScalingPerWave = 1.05f;
    
    [Header("Special Events")]
    [Tooltip("Boss enemy data for special waves")]
    public EnemyData[] bossEnemies;
    
    [Tooltip("Wave interval for boss spawns (every X waves)")]
    [Range(3, 20)] public int bossWaveInterval = 5;
    
    [Header("Runtime Info")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private int enemiesSpawnedThisWave = 0;
    [SerializeField] private bool waveInProgress = false;
    
    // Private state
    private List<Enemy> activeEnemies = new List<Enemy>();
    private Coroutine currentWaveCoroutine;
    private float lastSpawnTime = 0f;
    
    // Events
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action<Enemy> OnEnemySpawned;
    public System.Action<Enemy> OnEnemyKilled;
    public System.Action OnAllWavesCompleted;
    
    // Properties
    public int CurrentWave => currentWave;
    public int EnemiesAlive => enemiesAlive;
    public bool WaveInProgress => waveInProgress;
    public float WaveProgress => enemiesSpawnedThisWave / (float)GetEnemiesForWave(currentWave);
    
    #region Unity Lifecycle
    
    void Start()
    {
        ValidateConfiguration();
        
        if (useWaveSystem)
        {
            StartCoroutine(WaveManagerCoroutine());
        }
    }
    
    void Update()
    {
        CleanupDeadEnemies();
        UpdateEnemyCount();
    }
    
    #endregion
    
    #region Wave Management
    
    IEnumerator WaveManagerCoroutine()
    {
        yield return new WaitForSeconds(2f); // Initial delay
        
        while (true)
        {
            currentWave++;
            yield return StartCoroutine(SpawnWave(currentWave));
            
            // Wait for all enemies to be defeated
            yield return new WaitUntil(() => enemiesAlive == 0);
            
            OnWaveCompleted?.Invoke(currentWave);
            Debug.Log($"[EnemySpawner] Wave {currentWave} completed!");
            
            // Break time between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
    
    IEnumerator SpawnWave(int wave)
    {
        waveInProgress = true;
        enemiesSpawnedThisWave = 0;
        
        OnWaveStarted?.Invoke(wave);
        Debug.Log($"[EnemySpawner] Starting Wave {wave}");
        
        // Get wave configuration
        WaveConfiguration waveConfig = GetWaveConfiguration(wave);
        
        if (waveConfig != null)
        {
            // Use specific wave configuration
            Debug.Log($"[EnemySpawner] Using configured wave: {waveConfig.waveName}");
            yield return StartCoroutine(SpawnConfiguredWave(waveConfig, wave));
        }
        else
        {
            // Use procedural wave generation
            Debug.Log($"[EnemySpawner] Using procedural wave generation for wave {wave}");
            yield return StartCoroutine(SpawnProceduralWave(wave));
        }
        
        waveInProgress = false;
    }
    
    int GetEnemiesForWave(int wave)
    {
        return Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyScalingPerWave, wave - 1));
    }
    
    IEnumerator SpawnConfiguredWave(WaveConfiguration waveConfig, int wave)
    {
        // Get valid enemies for this wave
        var validEnemies = waveConfig.GetValidEnemiesForWave(wave);
        
        if (validEnemies.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] No valid enemies found for wave {wave}, falling back to procedural");
            yield return StartCoroutine(SpawnProceduralWave(wave));
            yield break;
        }
        
        // Spawn each enemy type according to its count
        foreach (var enemyConfig in validEnemies)
        {
            for (int i = 0; i < enemyConfig.count; i++)
            {
                // Wait if we've hit the concurrent limit
                yield return new WaitUntil(() => enemiesAlive < maxConcurrentEnemies);
                
                SpawnEnemy(enemyConfig.enemyData, wave);
                enemiesSpawnedThisWave++;
                
                // Use wave-specific spawn interval
                yield return new WaitForSeconds(waveConfig.spawnInterval);
            }
        }
    }
    
    IEnumerator SpawnProceduralWave(int wave)
    {
        int enemiesToSpawn = GetEnemiesForWave(wave);
        bool isBossWave = (wave % bossWaveInterval == 0) && bossEnemies.Length > 0;
        
        // Spawn regular enemies
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Wait if we've hit the concurrent limit
            yield return new WaitUntil(() => enemiesAlive < maxConcurrentEnemies);
            
            WaveEnemyConfig enemyConfig = SelectEnemyForWave(wave, isBossWave && i == enemiesToSpawn - 1);
            if (enemyConfig != null && enemyConfig.enemyData != null)
            {
                SpawnEnemy(enemyConfig.enemyData, wave);
                enemiesSpawnedThisWave++;
            }
            else
            {
                Debug.LogWarning($"[EnemySpawner] Failed to get valid enemy config for wave {wave}, enemy {i + 1}. Skipping this spawn.");
            }
            
            // Stagger spawns
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
    
    WaveConfiguration GetWaveConfiguration(int wave)
    {
        // Find specific wave configuration
        if (waveConfigurations != null)
        {
            foreach (var config in waveConfigurations)
            {
                if (config.waveNumber == wave)
                {
                    return config;
                }
            }
        }
        
        return null; // No specific configuration, use procedural generation
    }
    
    WaveEnemyConfig SelectEnemyForWave(int wave, bool forceBoss = false)
    {
        if (forceBoss && bossEnemies.Length > 0)
        {
            var bossConfig = new WaveEnemyConfig();
            bossConfig.enemyData = bossEnemies[Random.Range(0, bossEnemies.Length)];
            bossConfig.count = 1;
            bossConfig.spawnWeight = 1f;
            return bossConfig;
        }
        
        // Check if we have a specific wave configuration
        WaveConfiguration waveConfig = GetWaveConfiguration(wave);
        if (waveConfig != null)
        {
            var validEnemies = waveConfig.GetValidEnemiesForWave(wave);
            if (validEnemies.Count > 0)
            {
                return GetWeightedRandomEnemy(validEnemies);
            }
        }
        
        // Fallback to procedural generation using fallbackEnemies
        if (fallbackEnemies != null && fallbackEnemies.Length > 0)
        {
            List<EnemyData> suitableEnemies = new List<EnemyData>();
            int maxThreatLevel = Mathf.Min(10, 1 + (wave - 1) / 2); // Gradually introduce higher threat enemies
            
            foreach (var enemy in fallbackEnemies)
            {
                if (enemy != null && enemy.threatLevel <= maxThreatLevel)
                {
                    suitableEnemies.Add(enemy);
                }
            }
            
            if (suitableEnemies.Count == 0)
            {
                // Add all non-null fallback enemies as last resort
                foreach (var enemy in fallbackEnemies)
                {
                    if (enemy != null)
                    {
                        suitableEnemies.Add(enemy);
                    }
                }
            }
            
            if (suitableEnemies.Count > 0)
            {
                var fallbackConfig = new WaveEnemyConfig();
                fallbackConfig.enemyData = suitableEnemies[Random.Range(0, suitableEnemies.Count)];
                fallbackConfig.count = 1;
                fallbackConfig.spawnWeight = 1f;
                return fallbackConfig;
            }
        }
        
        // Final fallback: try boss enemies if available
        if (bossEnemies != null && bossEnemies.Length > 0)
        {
            Debug.LogWarning($"[EnemySpawner] No regular enemies configured for wave {wave}, using boss enemy as fallback!");
            var emergencyConfig = new WaveEnemyConfig();
            emergencyConfig.enemyData = bossEnemies[Random.Range(0, bossEnemies.Length)];
            emergencyConfig.count = 1;
            emergencyConfig.spawnWeight = 1f;
            return emergencyConfig;
        }
        
        Debug.LogError($"[EnemySpawner] No enemy configuration found for wave {wave}! Please configure wave configurations, fallback enemies, or boss enemies.");
        return null;
    }
    
    WaveEnemyConfig GetWeightedRandomEnemy(List<WaveEnemyConfig> enemies)
    {
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var enemy in enemies)
        {
            totalWeight += enemy.spawnWeight;
        }
        
        // Select random point in weight range
        float randomPoint = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // Find the enemy that corresponds to this weight point
        foreach (var enemy in enemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomPoint <= currentWeight)
            {
                return enemy;
            }
        }
        
        // Fallback to first enemy (shouldn't happen)
        return enemies[0];
    }
    
    #endregion
    
    #region Enemy Spawning
    
    public Enemy SpawnEnemy(EnemyData enemyData, int wave = 1)
    {
        if (enemyData == null || enemyData.enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Cannot spawn enemy - missing EnemyData or prefab!");
            return null;
        }
        
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[EnemySpawner] No valid spawn points available!");
            return null;
        }
        
        // Instantiate enemy
        GameObject enemyObject = Instantiate(enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        
        if (enemy == null)
        {
            Debug.LogError($"[EnemySpawner] Enemy prefab {enemyData.enemyPrefab.name} does not have Enemy component!");
            Destroy(enemyObject);
            return null;
        }
        
        // Apply enemy data
        enemy.ApplyEnemyData(enemyData);
        
        // Apply difficulty scaling
        if (useDifficultyScaling && wave > 1)
        {
            ApplyDifficultyScaling(enemy, wave);
        }
        
        // Subscribe to enemy events
        enemy.OnEnemyDeath += HandleEnemyDeath;
        
        // Track enemy
        activeEnemies.Add(enemy);
        enemiesAlive++;
        lastSpawnTime = Time.time;
        
        // Spawn effects
        if (enemyData.spawnEffect != null)
        {
            GameObject effect = Instantiate(enemyData.spawnEffect, spawnPoint.position, spawnPoint.rotation);
            Destroy(effect, 3f);
        }
        
        OnEnemySpawned?.Invoke(enemy);
        Debug.Log($"[EnemySpawner] Spawned {enemyData.enemyName} at {spawnPoint.name} (Wave {wave})");
        
        return enemy;
    }
    
    void ApplyDifficultyScaling(Enemy enemy, int wave)
    {
        if (enemy.Data == null) return;
        
        // Create scaled enemy data
        EnemyData scaledData = Instantiate(enemy.Data); // Create a copy
        
        // Scale health
        float healthMultiplier = Mathf.Pow(healthScalingPerWave, wave - 1);
        scaledData.maxHealth = Mathf.RoundToInt(scaledData.maxHealth * healthMultiplier);
        
        // Scale damage
        float damageMultiplier = Mathf.Pow(damageScalingPerWave, wave - 1);
        scaledData.attackDamage = Mathf.RoundToInt(scaledData.attackDamage * damageMultiplier);
        
        // Apply scaled data
        enemy.ApplyEnemyData(scaledData);
        
        Debug.Log($"[EnemySpawner] Applied scaling to {enemy.name}: Health x{healthMultiplier:F2}, Damage x{damageMultiplier:F2}");
    }
    
    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return transform; // Fallback to spawner position
        }
        
        // Filter spawn points that aren't too close to player or other enemies
        List<Transform> validSpawnPoints = new List<Transform>();
        
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null && IsSpawnPointValid(spawnPoint))
            {
                validSpawnPoints.Add(spawnPoint);
            }
        }
        
        if (validSpawnPoints.Count == 0)
        {
            validSpawnPoints.AddRange(spawnPoints); // Fallback to all spawn points
        }
        
        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }
    
    bool IsSpawnPointValid(Transform spawnPoint)
    {
        // Check if too close to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(spawnPoint.position, player.transform.position);
            if (distanceToPlayer < 5f) // Too close to player
            {
                return false;
            }
        }
        
        // Check if spawn point is clear
        Collider[] overlapping = Physics.OverlapSphere(spawnPoint.position, 1f);
        foreach (var collider in overlapping)
        {
            if (collider.GetComponent<Enemy>() != null)
            {
                return false; // Another enemy is too close
            }
        }
        
        return true;
    }
    
    #endregion
    
    #region Enemy Management
    
    void HandleEnemyDeath(Enemy enemy)
    {
        if (enemy != null)
        {
            // Award souls and resources (implement in game manager)
            AwardRewards(enemy);
            
            // Unsubscribe from events
            enemy.OnEnemyDeath -= HandleEnemyDeath;
            
            OnEnemyKilled?.Invoke(enemy);
        }
    }
    
    void AwardRewards(Enemy enemy)
    {
        if (enemy.Data == null) return;
        
        // Award souls
        int souls = enemy.Data.soulReward;
        Debug.Log($"[EnemySpawner] Player earned {souls} souls from {enemy.Data.enemyName}");
        
        // Award resources
        if (enemy.Data.resourceDrops != null)
        {
            foreach (var drop in enemy.Data.resourceDrops)
            {
                if (Random.Range(0f, 1f) <= drop.dropChance)
                {
                    int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                    Debug.Log($"[EnemySpawner] Dropped {amount} {drop.resourceType}");
                    // Implement actual resource awarding in game manager
                }
            }
        }
    }
    
    void CleanupDeadEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
    }
    
    void UpdateEnemyCount()
    {
        enemiesAlive = activeEnemies.Count;
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// Manually spawn a specific enemy type
    /// </summary>
    public Enemy SpawnSpecificEnemy(string enemyName, Vector3 position)
    {
        EnemyData enemyData = FindEnemyDataByName(enemyName);
        if (enemyData == null)
        {
            Debug.LogError($"[EnemySpawner] Enemy type '{enemyName}' not found!");
            return null;
        }
        
        GameObject enemyObject = Instantiate(enemyData.enemyPrefab, position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.ApplyEnemyData(enemyData);
            enemy.OnEnemyDeath += HandleEnemyDeath;
            activeEnemies.Add(enemy);
            enemiesAlive++;
            OnEnemySpawned?.Invoke(enemy);
        }
        
        return enemy;
    }
    
    /// <summary>
    /// Force start the next wave
    /// </summary>
    public void ForceNextWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
        }
        
        currentWave++;
        currentWaveCoroutine = StartCoroutine(SpawnWave(currentWave));
    }
    
    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDeath -= HandleEnemyDeath;
                Destroy(enemy.gameObject);
            }
        }
        
        activeEnemies.Clear();
        enemiesAlive = 0;
    }
    
    /// <summary>
    /// Get all enemies of a specific type
    /// </summary>
    public List<Enemy> GetEnemiesByType(string enemyName)
    {
        List<Enemy> result = new List<Enemy>();
        
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.Data != null && enemy.Data.enemyName == enemyName)
            {
                result.Add(enemy);
            }
        }
        
        return result;
    }
    
    #endregion
    
    #region Validation & Debug
    
    EnemyData FindEnemyDataByName(string enemyName)
    {
        // Search in wave configurations first
        if (waveConfigurations != null)
        {
            foreach (var waveConfig in waveConfigurations)
            {
                if (waveConfig.enemies != null)
                {
                    foreach (var enemyConfig in waveConfig.enemies)
                    {
                        if (enemyConfig.enemyData != null && enemyConfig.enemyData.enemyName == enemyName)
                        {
                            return enemyConfig.enemyData;
                        }
                    }
                }
            }
        }
        
        // Search in fallback enemies
        if (fallbackEnemies != null)
        {
            foreach (var enemyData in fallbackEnemies)
            {
                if (enemyData != null && enemyData.enemyName == enemyName)
                {
                    return enemyData;
                }
            }
        }
        
        // Search in boss enemies
        if (bossEnemies != null)
        {
            foreach (var enemyData in bossEnemies)
            {
                if (enemyData != null && enemyData.enemyName == enemyName)
                {
                    return enemyData;
                }
            }
        }
        
        return null;
    }
    
    void ValidateConfiguration()
    {
        bool hasAnyEnemies = false;
        bool hasWaveConfigs = false;
        bool hasFallbackEnemies = false;
        
        // Check wave configurations
        if (waveConfigurations != null && waveConfigurations.Length > 0)
        {
            foreach (var waveConfig in waveConfigurations)
            {
                if (waveConfig.enemies != null && waveConfig.enemies.Length > 0)
                {
                    hasAnyEnemies = true;
                    hasWaveConfigs = true;
                    break;
                }
            }
        }
        
        // Check fallback enemies
        if (fallbackEnemies != null && fallbackEnemies.Length > 0)
        {
            foreach (var enemy in fallbackEnemies)
            {
                if (enemy != null)
                {
                    hasAnyEnemies = true;
                    hasFallbackEnemies = true;
                    break;
                }
            }
        }
        
        // Check boss enemies as final fallback
        bool hasBossEnemies = (bossEnemies != null && bossEnemies.Length > 0);
        
        if (!hasAnyEnemies && hasBossEnemies)
        {
            Debug.LogWarning("[EnemySpawner] No regular enemies configured, but boss enemies are available. Waves will use boss enemies as fallback.");
            hasAnyEnemies = true;
        }
        
        if (!hasAnyEnemies)
        {
            Debug.LogError("[EnemySpawner] CRITICAL: No enemy types assigned! Please configure at least one of: wave configurations, fallback enemies, or boss enemies.");
        }
        else
        {
            // Give helpful configuration advice
            if (!hasWaveConfigs && !hasFallbackEnemies)
            {
                Debug.LogWarning("[EnemySpawner] Only boss enemies are configured. Consider adding fallback enemies for regular waves.");
            }
            else if (!hasFallbackEnemies)
            {
                Debug.LogWarning("[EnemySpawner] No fallback enemies configured. Waves beyond your wave configurations will fail to spawn.");
            }
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No spawn points assigned! Using spawner position.");
        }
        
        // Validate wave configurations
        if (waveConfigurations != null)
        {
            foreach (var waveConfig in waveConfigurations)
            {
                if (waveConfig.enemies != null)
                {
                    foreach (var enemyConfig in waveConfig.enemies)
                    {
                        if (enemyConfig.enemyData == null)
                        {
                            Debug.LogError($"[EnemySpawner] Null EnemyData found in wave {waveConfig.waveNumber}!");
                            continue;
                        }
                        
                        if (enemyConfig.enemyData.enemyPrefab == null)
                        {
                            Debug.LogError($"[EnemySpawner] EnemyData '{enemyConfig.enemyData.enemyName}' has no prefab assigned!");
                        }
                    }
                }
            }
        }
        
        // Validate fallback enemies
        if (fallbackEnemies != null)
        {
            foreach (var enemyData in fallbackEnemies)
            {
                if (enemyData == null)
                {
                    Debug.LogError("[EnemySpawner] Null EnemyData found in fallbackEnemies array!");
                    continue;
                }
                
                if (enemyData.enemyPrefab == null)
                {
                    Debug.LogError($"[EnemySpawner] EnemyData '{enemyData.enemyName}' has no prefab assigned!");
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn points
        if (spawnPoints != null)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);
                }
            }
        }
        
        // Draw detection ranges for active enemies
        Gizmos.color = Color.yellow;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.Data != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, enemy.Data.detectionRange);
            }
        }
    }
    
    #endregion
}