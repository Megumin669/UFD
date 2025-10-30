using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Base turret component that handles targeting, rotation, and shooting
/// Uses TurretData ScriptableObject for configuration
/// </summary>
[RequireComponent(typeof(Health))]
public class Turret : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Turret data asset containing all configuration")]
    public TurretData turretData;
    
    [Header("Components")]
    [Tooltip("Transform that rotates to aim at targets (turret head)")]
    public Transform turretHead;
    
    [Tooltip("Point where projectiles spawn")]
    public Transform projectileSpawnPoint;
    
    [Tooltip("Optional turret base (always static)")]
    public Transform turretBase;
    
    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    [Tooltip("Show targeting range in scene view")]
    public bool showRangeInScene = true;
    
    // Runtime state
    private Health healthComponent;
    private AudioSource audioSource;
    
    // Targeting system
    private Transform currentTarget;
    private List<Transform> potentialTargets = new List<Transform>();
    private float lastTargetScanTime = 0f;
    private const float TARGET_SCAN_INTERVAL = 0.2f; // Scan for targets 5 times per second
    
    // Shooting system
    private float lastFireTime = 0f;
    private bool canFire = true;
    
    // Rotation system
    private Quaternion targetRotation;
    private bool isRotating = false;
    
    // Power system
    private bool hasPower = true;
    
    // Properties
    public TurretData Data => turretData;
    public Transform CurrentTarget => currentTarget;
    public bool IsOperational => healthComponent != null && !healthComponent.IsDead && hasPower && turretData != null;
    public bool HasTarget => currentTarget != null;
    public float DistanceToTarget => currentTarget != null ? Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Get required components
        healthComponent = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        
        // Subscribe to health events
        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandleDestroy;
            healthComponent.OnHealthChanged += HandleDamaged;
        }
    }
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] === TURRET START ===");
            Debug.Log($"[{gameObject.name}] TurretData assigned: {turretData != null}");
            Debug.Log($"[{gameObject.name}] TurretHead assigned: {turretHead != null}");
            Debug.Log($"[{gameObject.name}] ProjectileSpawnPoint assigned: {projectileSpawnPoint != null}");
            Debug.Log($"[{gameObject.name}] Health component: {healthComponent != null}");
        }
        
        if (turretData != null)
        {
            InitializeFromData();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No TurretData assigned! Turret will not function.", this);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Initialization complete. IsOperational: {IsOperational}");
        }
    }
    
    void Update()
    {
        if (!IsOperational)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[{gameObject.name}] Not operational: Health={healthComponent != null}, HealthDead={healthComponent != null && healthComponent.IsDead}, Power={hasPower}, Data={turretData != null}");
            }
            return;
        }
        
        UpdateTargeting();
        UpdateRotation();
        UpdateShooting();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDestroy;
            healthComponent.OnHealthChanged -= HandleDamaged;
        }
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize turret from TurretData configuration
    /// </summary>
    void InitializeFromData()
    {
        if (turretData == null) return;
        
        // Apply health settings
        if (healthComponent != null)
        {
            healthComponent.healthStats.maxHealth = turretData.maxHealth;
            healthComponent.healthStats.startingHealth = turretData.maxHealth;
            healthComponent.healthStats.enableRegeneration = false; // Turrets don't regenerate by default
            healthComponent.Initialize();
        }
        
        // Validate components
        if (turretHead == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No turret head assigned! Rotation will not work.");
        }
        
        if (projectileSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No projectile spawn point assigned! Using turret position.");
            projectileSpawnPoint = transform;
        }
        
        // Set initial rotation target
        if (turretHead != null)
        {
            targetRotation = turretHead.rotation;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Turret initialized: {turretData.turretName}");
            Debug.Log($"[{gameObject.name}] - Range: {turretData.range}, Damage: {turretData.damage}");
            Debug.Log($"[{gameObject.name}] - Looking for tags: [{string.Join(", ", turretData.targetableTags)}]");
            Debug.Log($"[{gameObject.name}] - Can target Air: {turretData.canTargetAir}, Ground: {turretData.canTargetGround}");
        }
    }
    
    #endregion
    
    #region Targeting System
    
    void UpdateTargeting()
    {
        // Scan for targets periodically
        if (Time.time - lastTargetScanTime >= TARGET_SCAN_INTERVAL)
        {
            ScanForTargets();
            lastTargetScanTime = Time.time;
        }
        
        // Check if current target is still valid
        if (currentTarget != null)
        {
            if (!IsValidTarget(currentTarget))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] Lost target: {currentTarget.name}");
                }
                currentTarget = null;
            }
        }
        
        // Select best target if we don't have one
        if (currentTarget == null && potentialTargets.Count > 0)
        {
            SelectBestTarget();
        }
    }
    
    void ScanForTargets()
    {
        potentialTargets.Clear();
        
        // Find all potential targets in range (360-degree sphere detection)
        Collider[] colliders = Physics.OverlapSphere(transform.position, turretData.range);
        
        foreach (Collider col in colliders)
        {
            if (IsValidTarget(col.transform))
            {
                potentialTargets.Add(col.transform);
                if (showDebugInfo)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    Debug.Log($"[{gameObject.name}] ✓ Found valid target: {col.gameObject.name} at distance {dist:F1}");
                }
            }
        }
        
        if (showDebugInfo && potentialTargets.Count > 0)
        {
            Debug.Log($"[{gameObject.name}] Scan complete: {potentialTargets.Count} valid targets");
        }
    }
    
    bool IsValidTarget(Transform target)
    {
        if (target == null) return false;
        
        // Don't target self
        if (target == transform || target.IsChildOf(transform)) return false;
        
        // Check if target has valid tag
        bool hasValidTag = false;
        foreach (string tag in turretData.targetableTags)
        {
            try
            {
                if (target.CompareTag(tag))
                {
                    hasValidTag = true;
                    break;
                }
            }
            catch (UnityException)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[{gameObject.name}] Tag '{tag}' is not defined in Tag Manager!");
                }
            }
        }
        if (!hasValidTag) return false;
        
        // Check if target is in range
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > turretData.range) return false;
        
        // Check if target is dead
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead) return false;
        
        // Check air/ground targeting capability
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            bool isFlying = enemy.IsFlying;
            if (isFlying && !turretData.canTargetAir) return false;
            if (!isFlying && !turretData.canTargetGround) return false;
        }
        
        // Check line of sight (if required)
        if (turretData.requireLineOfSight && !HasLineOfSight(target)) return false;
        
        return true;
    }
    
    bool HasLineOfSight(Transform target)
    {
        // Use turret position if spawn point is not set
        Vector3 originPoint = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        
        // Aim at center of target (slightly up from position)
        Vector3 targetPoint = target.position + Vector3.up * 0.5f;
        Vector3 directionToTarget = (targetPoint - originPoint).normalized;
        float distanceToTarget = Vector3.Distance(originPoint, targetPoint);
        
        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(originPoint, directionToTarget, out hit, distanceToTarget))
        {
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] LOS raycast hit: {hit.transform.name} (root: {hit.transform.root.name}), looking for: {target.name} (root: {target.root.name})");
            }
            
            // Check if we hit the target, target's parent, target's root, or a child of target
            if (hit.transform == target || 
                hit.transform.root == target.root ||
                hit.transform.IsChildOf(target) ||
                target.IsChildOf(hit.transform))
            {
                return true;
            }
            
            // Also check if the hit object has the same tag as the target (likely another enemy in the way, which is OK)
            if (hit.transform.CompareTag(target.tag))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] Hit same-tagged object, allowing LOS");
                }
                return true;
            }
            
            // Something is blocking
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] LOS blocked by: {hit.transform.name}");
            }
            return false;
        }
        
        return true; // No obstacles found
    }
    
    void SelectBestTarget()
    {
        if (potentialTargets.Count == 0) return;
        
        Transform bestTarget = null;
        float bestScore = float.MinValue;
        
        foreach (Transform target in potentialTargets)
        {
            float score = CalculateTargetPriority(target);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }
        
        if (bestTarget != currentTarget)
        {
            currentTarget = bestTarget;
            if (showDebugInfo && currentTarget != null)
            {
                Debug.Log($"[{gameObject.name}] New target selected: {currentTarget.name}");
            }
        }
    }
    
    float CalculateTargetPriority(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        Health targetHealth = target.GetComponent<Health>();
        Enemy enemy = target.GetComponent<Enemy>();
        
        float score = 0f;
        
        switch (turretData.targetPriority)
        {
            case TurretTargetPriority.Closest:
                score = 1000f - distance; // Closer = higher score
                break;
                
            case TurretTargetPriority.Furthest:
                score = distance; // Further = higher score
                break;
                
            case TurretTargetPriority.HighestHealth:
                if (targetHealth != null)
                    score = targetHealth.CurrentHealth;
                break;
                
            case TurretTargetPriority.LowestHealth:
                if (targetHealth != null)
                    score = 1000f - targetHealth.CurrentHealth;
                break;
                
            case TurretTargetPriority.Flying:
                if (enemy != null && enemy.IsFlying)
                    score = 1000f;
                else
                    score = 100f;
                break;
                
            case TurretTargetPriority.HighestThreat:
                if (enemy != null && enemy.Data != null)
                    score = enemy.Data.threatLevel * 100f;
                break;
                
            default:
                score = 1000f - distance; // Default to closest
                break;
        }
        
        return score;
    }
    
    #endregion
    
    #region Rotation System
    
    void UpdateRotation()
    {
        if (turretHead == null) return;
        
        // Lock turret head position (should only rotate, not move)
        Vector3 lockedLocalPosition = turretHead.localPosition;
        
        if (currentTarget != null)
        {
            // Calculate target rotation
            Vector3 directionToTarget = (currentTarget.position - turretHead.position).normalized;
            Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);
            
            // Apply rotation constraints
            if (!turretData.canRotate360)
            {
                desiredRotation = ConstrainRotation(desiredRotation);
            }
            
            targetRotation = desiredRotation;
            isRotating = true;
        }
        
        // Smoothly rotate towards target
        if (isRotating)
        {
            turretHead.rotation = Quaternion.RotateTowards(
                turretHead.rotation, 
                targetRotation, 
                turretData.rotationSpeed * Time.deltaTime
            );
            
            // Check if we've reached target rotation
            if (Quaternion.Angle(turretHead.rotation, targetRotation) < 1f)
            {
                isRotating = false;
            }
        }
        
        // Ensure turret head stays in place (only rotates)
        turretHead.localPosition = lockedLocalPosition;
    }
    
    Quaternion ConstrainRotation(Quaternion desiredRotation)
    {
        // Convert to local space relative to turret base
        Transform parentTransform = turretHead.parent != null ? turretHead.parent : transform;
        Quaternion localRotation = Quaternion.Inverse(parentTransform.rotation) * desiredRotation;
        Vector3 localEuler = localRotation.eulerAngles;
        
        // Normalize angles to -180 to 180 range
        if (localEuler.y > 180f) localEuler.y -= 360f;
        
        // Apply constraints
        localEuler.y = Mathf.Clamp(localEuler.y, turretData.minRotationAngle, turretData.maxRotationAngle);
        
        // Convert back to world rotation
        return parentTransform.rotation * Quaternion.Euler(0, localEuler.y, 0);
    }
    
    #endregion
    
    #region Shooting System
    
    void UpdateShooting()
    {
        if (!canFire)
        {
            if (showDebugInfo && currentTarget != null)
            {
                Debug.Log($"[{gameObject.name}] Cannot fire - canFire is false");
            }
            return;
        }
        
        if (currentTarget == null)
        {
            return; // No target, no debug spam
        }
        
        // Check if we can fire (fire rate cooldown)
        float effectiveFireRate = turretData.GetEffectiveFireRate();
        if (Time.time - lastFireTime < effectiveFireRate)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] Cooldown: {Time.time - lastFireTime:F2}/{effectiveFireRate:F2}");
            }
            return;
        }
        
        // Check if we're aimed at target
        if (IsAimedAtTarget())
        {
            Fire();
        }
        else if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Not aimed at target yet");
        }
    }
    
    bool IsAimedAtTarget()
    {
        if (currentTarget == null || turretHead == null)
        {
            if (showDebugInfo && currentTarget != null)
            {
                Debug.LogWarning($"[{gameObject.name}] IsAimedAtTarget failed - turretHead is null!");
            }
            return false;
        }
        
        Vector3 directionToTarget = (currentTarget.position - turretHead.position).normalized;
        Vector3 turretForward = turretHead.forward;
        
        float angle = Vector3.Angle(turretForward, directionToTarget);
        
        // Allow some tolerance based on accuracy
        float aimTolerance = Mathf.Lerp(10f, 1f, turretData.accuracy);
        
        bool isAimed = angle <= aimTolerance;
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Aim check: angle={angle:F1}°, tolerance={aimTolerance:F1}°, aimed={isAimed}");
        }
        
        return isAimed;
    }
    
    void Fire()
    {
        if (turretData.projectilePrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No projectile prefab assigned!");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] FIRING at {currentTarget?.name}!");
        }
        
        lastFireTime = Time.time;
        
        // Spawn projectile
        SpawnProjectile();
        
        // Play effects
        PlayFireEffects();
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Fired at {currentTarget.name}");
        }
    }
    
    void SpawnProjectile()
    {
        Vector3 spawnPosition = projectileSpawnPoint.position;
        Quaternion spawnRotation = turretHead.rotation;
        
        // For ballistic projectiles, calculate predicted target position
        Transform targetTransform = currentTarget.transform;
        Vector3 predictedPosition = currentTarget.transform.position;
        
        if (turretData.projectileType == ProjectileBehavior.Ballistic)
        {
            predictedPosition = PredictTargetPosition();
        }
        
        // Apply accuracy spread
        if (turretData.accuracy < 1f)
        {
            float spread = (1f - turretData.accuracy) * 10f; // Max 10 degree spread
            Vector3 randomSpread = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );
            spawnRotation *= Quaternion.Euler(randomSpread);
        }
        
        GameObject projectileObject = Instantiate(turretData.projectilePrefab, spawnPosition, spawnRotation);
        
        // Configure projectile
        TurretProjectile projectile = projectileObject.GetComponent<TurretProjectile>();
        if (projectile != null)
        {
            Enemy enemy = currentTarget.GetComponent<Enemy>();
            bool isFlying = enemy != null ? enemy.IsFlying : false;
            int enemyThreat = enemy != null && enemy.Data != null ? enemy.Data.threatLevel : 1;
            
            int effectiveDamage = turretData.GetEffectiveDamage(isFlying, enemyThreat);
            
            // For ballistic projectiles, pass predicted position instead of actual target
            Transform targetToPass = turretData.projectileType == ProjectileBehavior.Ballistic ? null : currentTarget;
            
            projectile.Initialize(
                targetToPass,
                effectiveDamage,
                turretData.projectileSpeed,
                turretData.projectileLifetime,
                turretData.projectileType,
                turretData.piercing,
                turretData.maxPierceTargets,
                turretData.explosionRadius,
                turretData.explosionDamage,
                turretData.hitEffect,
                turretData.explosionEffect,
                turretData.arcHeightMultiplier,
                predictedPosition  // Pass predicted position for ballistic
            );
        }
    }
    
    /// <summary>
    /// Predict where the target will be when projectile lands (for ballistic projectiles)
    /// </summary>
    Vector3 PredictTargetPosition()
    {
        if (currentTarget == null) return Vector3.zero;
        
        Enemy enemy = currentTarget.GetComponent<Enemy>();
        if (enemy == null || enemy.Data == null)
        {
            return currentTarget.transform.position; // Can't predict, use current position
        }
        
        // Get enemy's current velocity
        Vector3 enemyVelocity = Vector3.zero;
        
        // Try to get NavMeshAgent velocity (most accurate for moving enemies)
        var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null && navAgent.hasPath && navAgent.velocity.magnitude > 0.1f)
        {
            enemyVelocity = navAgent.velocity;
        }
        else
        {
            // If not moving, no need to predict
            return currentTarget.transform.position;
        }
        
        // Iterative prediction: calculate flight time more accurately
        Vector3 predictedPosition = currentTarget.transform.position;
        float gravity = Mathf.Abs(Physics.gravity.y);
        
        // Iterate to find accurate prediction (accounts for changing distance as target moves)
        for (int i = 0; i < 3; i++) // 3 iterations is usually enough
        {
            float distance = Vector3.Distance(projectileSpawnPoint.position, predictedPosition);
            
            // Calculate flight time for ballistic arc to this position
            // Using 45-degree optimal angle: time = sqrt(2 * distance / gravity)
            float flightTime = Mathf.Sqrt(2f * distance / gravity);
            
            // Clamp to reasonable values
            flightTime = Mathf.Clamp(flightTime, 0.2f, 4f);
            
            // Update prediction with this flight time
            predictedPosition = currentTarget.transform.position + enemyVelocity * flightTime;
            predictedPosition.y = currentTarget.transform.position.y; // Keep at same height
        }
        
        return predictedPosition;
    }
    
    void PlayFireEffects()
    {
        // Spawn muzzle flash
        if (turretData.muzzleFlashEffect != null)
        {
            GameObject flash = Instantiate(turretData.muzzleFlashEffect, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            Destroy(flash, 2f);
        }
        
        // Play fire sound
        if (turretData.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(turretData.fireSound);
        }
    }
    
    #endregion
    
    #region Power System
    
    public void SetPowerState(bool powered)
    {
        hasPower = powered;
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Power state: {(powered ? "ON" : "OFF")}");
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    void HandleDestroy()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Turret destroyed");
        }
        
        // Play destruction effects
        if (turretData.destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(turretData.destroySound);
        }
        
        // Disable targeting and shooting
        canFire = false;
        currentTarget = null;
        
        // Could spawn wreckage or explosion here
        Destroy(gameObject, 2f); // Delay to allow death sound to play
    }
    
    void HandleDamaged(int currentHealth, int maxHealth)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Turret damaged: {currentHealth}/{maxHealth}");
        }
        
        // Could add damage effects, sparks, etc.
    }
    
    #endregion
    
    #region Debug & Visualization
    
    void OnDrawGizmos()
    {
        if (!showRangeInScene || turretData == null) return;
        
        // Draw range
        Gizmos.color = IsOperational ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, turretData.range);
        
        // Draw target line
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
        
        // Draw rotation constraints
        if (turretHead != null && !turretData.canRotate360)
        {
            Gizmos.color = Color.blue;
            Vector3 minDirection = Quaternion.Euler(0, turretData.minRotationAngle, 0) * turretHead.forward;
            Vector3 maxDirection = Quaternion.Euler(0, turretData.maxRotationAngle, 0) * turretHead.forward;
            
            Gizmos.DrawRay(turretHead.position, minDirection * turretData.range);
            Gizmos.DrawRay(turretHead.position, maxDirection * turretData.range);
        }
    }
    
    #endregion
}