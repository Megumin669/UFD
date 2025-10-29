using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Base enemy component that uses EnemyData for configuration.
/// This replaces the old Actor system with a more flexible, data-driven approach.
/// </summary>
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [Tooltip("Enemy data asset that defines this enemy's properties")]
    public EnemyData enemyData;
    
    [Header("Runtime State")]
    [Tooltip("Current target (automatically assigned)")]
    public GameObject currentTarget;
    
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    // Component references
    private Health healthComponent;
    private NavMeshAgent navAgent;
    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody rigidBody;
    
    // AI State
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime = 0f;
    private float lastAbilityTime = 0f;
    private float detectionTimer = 0f;
    private bool hasDetectedTarget = false;
    
    // Target search persistence
    private float noTargetTimer = 0f;
    private const float MAX_NO_TARGET_TIME = 20f; // 20 seconds before giving up
    private bool hasHadTarget = false; // Track if enemy ever had a target
    
    // Flying enemy support
    private float currentFlyingHeight = 0f;
    private bool isFlying = false;
    
    // Ability cooldowns
    private Dictionary<EnemyAbilityType, float> abilityCooldowns = new Dictionary<EnemyAbilityType, float>();
    
    // Events
    public System.Action<Enemy> OnEnemyDeath;
    public System.Action<Enemy, GameObject> OnTargetChanged;
    public System.Action<Enemy, EnemyAbilityType> OnAbilityUsed;
    
    // Properties
    public EnemyData Data => enemyData;
    public EnemyState State => currentState;
    public bool IsDead => healthComponent != null ? healthComponent.IsDead : false;
    public bool IsFlying => isFlying;
    public float DistanceToTarget => currentTarget != null ? Vector3.Distance(transform.position, currentTarget.transform.position) : float.MaxValue;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Get required components
        healthComponent = GetComponent<Health>();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        
        // Subscribe to health events
        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandleDeath;
            healthComponent.OnHealthChanged += HandleHealthChanged;
        }
    }
    
    void Start()
    {
        if (enemyData != null)
        {
            InitializeFromData();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No EnemyData assigned! Enemy will not function correctly.", this);
        }
    }
    
    void Update()
    {
        if (IsDead || enemyData == null) return;
        
        UpdateAI();
        UpdateAbilityCooldowns();
        
        if (isFlying)
        {
            UpdateFlying();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDeath;
            healthComponent.OnHealthChanged -= HandleHealthChanged;
        }
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize enemy from EnemyData configuration
    /// </summary>
    void InitializeFromData()
    {
        if (enemyData == null) return;
        
        // Apply health settings
        if (healthComponent != null)
        {
            // Directly modify the health component's stats
            healthComponent.healthStats.maxHealth = enemyData.maxHealth;
            healthComponent.healthStats.startingHealth = enemyData.GetEffectiveStartingHealth();
            healthComponent.healthStats.enableRegeneration = enemyData.healthRegenRate > 0;
            healthComponent.healthStats.regenerationRate = enemyData.healthRegenRate;
            healthComponent.healthStats.regenerationDelay = enemyData.healthRegenDelay;
            
            // Reinitialize health with new settings
            healthComponent.Initialize();
        }
        
        // Apply NavMesh settings
        if (navAgent != null)
        {
            navAgent.speed = enemyData.moveSpeed;
            navAgent.angularSpeed = enemyData.turnSpeed;
            navAgent.stoppingDistance = enemyData.stoppingDistance;
            navAgent.agentTypeID = enemyData.navMeshAgentType;
        }
        
        // Setup flying behavior
        if (enemyData.canFly)
        {
            EnableFlying();
        }
        
        // Initialize ability cooldowns
        abilityCooldowns.Clear(); // Clear any existing cooldowns
        if (enemyData.abilities != null)
        {
            foreach (var ability in enemyData.abilities)
            {
                if (ability != null)
                {
                    abilityCooldowns[ability.abilityType] = 0f;
                }
            }
        }
        
        // Set initial state
        ChangeState(EnemyState.Patrol);
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Enemy initialized: {enemyData.enemyName} - Health: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth} - Speed: {enemyData.moveSpeed}");
        }
    }
    
    /// <summary>
    /// Apply EnemyData to existing enemy (for runtime changes)
    /// </summary>
    public void ApplyEnemyData(EnemyData data)
    {
        enemyData = data;
        InitializeFromData();
    }
    
    #endregion
    
    #region AI State Machine
    
    /// <summary>
    /// Main AI update loop
    /// </summary>
    void UpdateAI()
    {
        // Update detection
        UpdateTargetDetection();
        
        // Update behavior based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Patrol:
                HandlePatrolState();
                break;
            case EnemyState.Chase:
                HandleChaseState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
            case EnemyState.Flee:
                HandleFleeState();
                break;
            case EnemyState.Ability:
                HandleAbilityState();
                break;
        }
    }
    
    void UpdateTargetDetection()
    {
        if (currentTarget == null || DistanceToTarget > enemyData.maxChaseRange)
        {
            // Lost target or don't have one - try to find a new one
            FindBestTarget();
            
            // Only start the give-up timer if we've had a target before AND still can't find one
            if (currentTarget == null)
            {
                if (hasHadTarget)
                {
                    noTargetTimer += Time.deltaTime;
                    
                    // Only give up after a long time without ANY targets
                    if (noTargetTimer >= MAX_NO_TARGET_TIME)
                    {
                        Debug.Log($"[{gameObject.name}] No target found for {MAX_NO_TARGET_TIME} seconds. Enemy giving up.");
                        HandleGiveUp();
                        return;
                    }
                }
                // If we've never had a target, don't start the timer - keep searching indefinitely
            }
            else
            {
                // Found a target - reset timer and mark that we've had one
                noTargetTimer = 0f;
                if (!hasHadTarget)
                {
                    hasHadTarget = true;
                    Debug.Log($"[{gameObject.name}] First target acquired: {currentTarget.name}");
                }
            }
        }
        else
        {
            // We have a valid target in range - reset timer
            noTargetTimer = 0f;
        }
    }
    
    void FindBestTarget()
    {
        GameObject bestTarget = null;
        float bestScore = float.MinValue;
        
        // First, try to find primary target
        GameObject primaryTarget = FindPrimaryTarget();
        if (primaryTarget != null)
        {
            bestTarget = primaryTarget;
            bestScore = 1000f; // High priority for primary target
        }
        
        // If no primary target, search fallback targets
        if (bestTarget == null && enemyData.fallbackTargets != null)
        {
            foreach (var priority in enemyData.fallbackTargets)
            {
                GameObject target = FindTargetByPriority(priority);
                if (target != null)
                {
                    float score = CalculateTargetScore(target, priority);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = target;
                    }
                }
            }
        }
        
        if (bestTarget != currentTarget)
        {
            SetTarget(bestTarget);
        }
    }
    
    GameObject FindPrimaryTarget()
    {
        switch (enemyData.primaryTarget)
        {
            case PrimaryTargetType.Player:
                return FindPlayerInRange(enemyData.playerDetectionRange);
            case PrimaryTargetType.Defenses:
                return FindDefenseInRange(enemyData.defenseDetectionRange);
            case PrimaryTargetType.Sanctum:
                return FindSanctumInRange(enemyData.sanctumDetectionRange);
            case PrimaryTargetType.Closest:
                return FindClosestTargetInRange(enemyData.detectionRange);
            default:
                return null;
        }
    }
    
    GameObject FindTargetByPriority(TargetPriority priority)
    {
        switch (priority)
        {
            case TargetPriority.Player:
                return FindPlayerTarget();
            case TargetPriority.Defenses:
                return FindDefenseTarget();
            case TargetPriority.Sanctum:
                return FindSanctumTarget();
            case TargetPriority.Closest:
                return FindClosestTarget();
            default:
                return null;
        }
    }
    
    float CalculateTargetScore(GameObject target, TargetPriority priority)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        float score = 100f - distance; // Closer targets score higher
        
        // Apply priority weighting
        switch (priority)
        {
            case TargetPriority.Player:
                score += 50f;
                break;
            case TargetPriority.Sanctum:
                score += 30f;
                break;
            case TargetPriority.Defenses:
                score += 20f;
                break;
        }
        
        return score;
    }
    
    #endregion
    
    #region State Handlers
    
    void HandleIdleState()
    {
        // Try to find a target if we don't have one
        if (currentTarget == null)
        {
            FindBestTarget();
        }
        
        // If we found a target and it's in range, start chasing
        if (currentTarget != null && DistanceToTarget <= GetDetectionRangeForTarget(currentTarget))
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    void HandlePatrolState()
    {
        // Try to find a target if we don't have one
        if (currentTarget == null)
        {
            FindBestTarget();
        }
        
        // If we found a target and it's in range, start chasing
        if (currentTarget != null && DistanceToTarget <= GetDetectionRangeForTarget(currentTarget))
        {
            ChangeState(EnemyState.Chase);
        }
        
        // Basic patrol behavior - can be expanded later
        if (navAgent != null && !navAgent.hasPath)
        {
            // Simple random wandering when no target
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y; // Keep same height
            
            NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, 1);
            navAgent.SetDestination(hit.position);
        }
    }
    
    void HandleChaseState()
    {
        if (currentTarget == null)
        {
            // Don't immediately give up - go to Idle to keep searching more actively
            ChangeState(EnemyState.Idle);
            return;
        }
        
        float distanceToTarget = DistanceToTarget;
        
        if (distanceToTarget <= enemyData.attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distanceToTarget > enemyData.maxChaseRange)
        {
            ChangeState(EnemyState.Patrol);
        }
        else
        {
            // Move towards target
            if (navAgent != null)
            {
                navAgent.SetDestination(currentTarget.transform.position);
            }
        }
    }
    
    void HandleAttackState()
    {
        if (currentTarget == null || DistanceToTarget > enemyData.attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // Face target
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToTarget);
        
        // Attack if cooldown is ready
        if (Time.time - lastAttackTime >= enemyData.attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }
    
    void HandleFleeState()
    {
        // Flee logic - move away from threats
        if (currentTarget != null)
        {
            Vector3 fleeDirection = transform.position - currentTarget.transform.position;
            Vector3 fleePosition = transform.position + fleeDirection.normalized * 10f;
            
            if (navAgent != null)
            {
                navAgent.SetDestination(fleePosition);
            }
        }
    }
    
    void HandleAbilityState()
    {
        // Ability state handling - return to previous state after ability
        // This is managed by individual abilities
    }
    
    #endregion
    
    #region Combat & Abilities
    
    void PerformAttack()
    {
        if (currentTarget == null) return;
        
        // Deal damage to target
        if (currentTarget.TryGetComponent<Health>(out Health targetHealth))
        {
            int damage = enemyData.attackDamage;
            targetHealth.TakeDamage(damage);
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] Attacked {currentTarget.name} for {damage} damage");
            }
        }
        
        // Play attack sound
        PlaySound(enemyData.attackSounds);
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    public bool TryUseAbility(EnemyAbilityType abilityType)
    {
        if (enemyData == null) return false;
        if (!enemyData.HasAbility(abilityType)) return false;
        if (!IsAbilityReady(abilityType)) return false;
        
        var ability = enemyData.GetAbility(abilityType);
        if (ability == null) return false;
        
        // Set cooldown safely
        if (!abilityCooldowns.ContainsKey(abilityType))
        {
            abilityCooldowns.Add(abilityType, Time.time + ability.cooldown);
        }
        else
        {
            abilityCooldowns[abilityType] = Time.time + ability.cooldown;
        }
        
        // Execute ability
        ExecuteAbility(ability);
        
        OnAbilityUsed?.Invoke(this, abilityType);
        return true;
    }
    
    void ExecuteAbility(EnemyAbility ability)
    {
        switch (ability.abilityType)
        {
            case EnemyAbilityType.Charge:
                StartCoroutine(ChargeAbility(ability));
                break;
            case EnemyAbilityType.Heal:
                HealAbility(ability);
                break;
            case EnemyAbilityType.DisableTurret:
                DisableTurretAbility(ability);
                break;
            // Add more abilities as needed
        }
        
        // Play ability effect and sound
        if (ability.effect != null)
        {
            GameObject effect = Instantiate(ability.effect, transform.position, transform.rotation);
            Destroy(effect, ability.duration);
        }
        
        if (ability.sound != null)
        {
            PlaySound(ability.sound);
        }
    }
    
    #endregion
    
    #region Flying System
    
    void EnableFlying()
    {
        isFlying = true;
        currentFlyingHeight = enemyData.flyingHeight;
        
        if (navAgent != null)
        {
            navAgent.enabled = false; // Disable NavMesh for flying enemies
        }
        
        if (rigidBody != null)
        {
            rigidBody.useGravity = false;
        }
    }
    
    void UpdateFlying()
    {
        // Maintain flying height
        Vector3 position = transform.position;
        
        // Raycast down to find ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, enemyData.groundLayer))
        {
            float targetHeight = hit.point.y + currentFlyingHeight;
            position.y = Mathf.Lerp(position.y, targetHeight, Time.deltaTime * 2f);
            transform.position = position;
        }
        
        // Manual movement for flying enemies
        if (currentTarget != null && currentState == EnemyState.Chase)
        {
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += direction * enemyData.moveSpeed * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] State changed to: {newState}");
        }
    }
    
    void SetTarget(GameObject target)
    {
        currentTarget = target;
        OnTargetChanged?.Invoke(this, target);
        
        if (showDebugInfo && target != null)
        {
            Debug.Log($"[{gameObject.name}] Target set to: {target.name}");
        }
    }
    
    float GetDetectionRangeForTarget(GameObject target)
    {
        if (target == null) return enemyData.detectionRange;
        
        // Check target type and return appropriate range
        if (target.CompareTag("Player"))
        {
            return enemyData.playerDetectionRange;
        }
        else if (target.CompareTag("Defense"))
        {
            return enemyData.defenseDetectionRange;
        }
        else if (target.CompareTag("Sanctum"))
        {
            return enemyData.sanctumDetectionRange;
        }
        
        // Default detection range for other targets
        return enemyData.detectionRange;
    }
    
    GameObject FindPlayerTarget()
    {
        return FindPlayerInRange(enemyData.detectionRange);
    }
    
    GameObject FindPlayerInRange(float range)
    {
        try
        {
            // Find player by tag or component
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= range)
            {
                return player;
            }
            return null;
        }
        catch (UnityException ex)
        {
            if (ex.Message.Contains("Tag") && ex.Message.Contains("not defined"))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[{gameObject.name}] Player tag not defined in Tag Manager. Skipping player targeting.");
                }
                return null;
            }
            throw; // Re-throw other exceptions
        }
    }
    
    GameObject FindDefenseTarget()
    {
        return FindDefenseInRange(enemyData.detectionRange);
    }
    
    GameObject FindDefenseInRange(float range)
    {
        try
        {
            // Find nearest defense structure
            GameObject[] defenses = GameObject.FindGameObjectsWithTag("Defense");
            GameObject nearest = null;
            float nearestDistance = range;
            
            foreach (var defense in defenses)
            {
                if (defense == null) continue;
                
                float distance = Vector3.Distance(transform.position, defense.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = defense;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }
        catch (UnityException ex)
        {
            if (ex.Message.Contains("Tag") && ex.Message.Contains("not defined"))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[{gameObject.name}] Defense tag not defined in Tag Manager. Skipping defense targeting.");
                }
                return null;
            }
            throw; // Re-throw other exceptions
        }
    }
    
    GameObject FindSanctumTarget()
    {
        return FindSanctumInRange(enemyData.maxChaseRange);
    }
    
    GameObject FindSanctumInRange(float range)
    {
        try
        {
            // Find the main objective
            GameObject[] sanctums = GameObject.FindGameObjectsWithTag("Sanctum");
            if (sanctums.Length > 0)
            {
                GameObject sanctum = sanctums[0];
                if (sanctum != null && Vector3.Distance(transform.position, sanctum.transform.position) <= range)
                {
                    return sanctum;
                }
            }
            return null;
        }
        catch (UnityException ex)
        {
            if (ex.Message.Contains("Tag") && ex.Message.Contains("not defined"))
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[{gameObject.name}] Sanctum tag not defined in Tag Manager. Skipping sanctum targeting.");
                }
                return null;
            }
            throw; // Re-throw other exceptions
        }
    }
    
    GameObject FindClosestTarget()
    {
        return FindClosestTargetInRange(enemyData.detectionRange);
    }
    
    GameObject FindClosestTargetInRange(float range)
    {
        // Find any valid target within range
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;
            
            // Check if it's a valid target (has Health component)
            if (collider.GetComponent<Health>() != null)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closest = collider.gameObject;
                    closestDistance = distance;
                }
            }
        }
        
        return closest;
    }
    
    bool IsAbilityReady(EnemyAbilityType abilityType)
    {
        // Ability is ready if it's not in cooldown (not in dictionary) or cooldown time has passed
        return !abilityCooldowns.ContainsKey(abilityType) || Time.time >= abilityCooldowns[abilityType];
    }
    
    void UpdateAbilityCooldowns()
    {
        // Update cooldown tracking - create a copy of keys to avoid modification during enumeration
        if (abilityCooldowns.Count == 0) return;
        
        var abilityKeys = new List<EnemyAbilityType>(abilityCooldowns.Keys);
        
        foreach (var abilityType in abilityKeys)
        {
            if (abilityCooldowns.ContainsKey(abilityType) && 
                Time.time >= abilityCooldowns[abilityType] && 
                enemyData != null && 
                enemyData.HasAbility(abilityType))
            {
                // Ability is ready, potentially use it based on AI logic
                if (Random.Range(0f, 1f) < 0.1f) // 10% chance per frame when ready
                {
                    TryUseAbility(abilityType);
                }
            }
        }
    }
    
    void PlaySound(AudioClip[] soundArray)
    {
        if (soundArray != null && soundArray.Length > 0 && audioSource != null)
        {
            AudioClip clip = soundArray[Random.Range(0, soundArray.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    void HandleDeath()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Enemy died - awarding {enemyData.soulReward} souls");
        }
        
        // Spawn death effect
        if (enemyData.deathEffect != null)
        {
            GameObject effect = Instantiate(enemyData.deathEffect, transform.position, transform.rotation);
            Destroy(effect, enemyData.deathEffectDuration);
        }
        
        // Play death sound
        PlaySound(enemyData.deathSound);
        
        // Award souls and resources (implement in game manager)
        OnEnemyDeath?.Invoke(this);
        
        // Destroy after a short delay to allow effects to play
        Destroy(gameObject, 0.5f);
    }
    
    void HandleGiveUp()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] Enemy giving up after {MAX_NO_TARGET_TIME} seconds without target");
        }
        
        // Play a different sound or effect for giving up (optional)
        // Could spawn a different effect than death
        
        // Award reduced souls for giving up (optional - could be 0)
        // OnEnemyGiveUp?.Invoke(this);
        
        // Destroy with same delay as death
        Destroy(gameObject, 0.5f);
    }
    
    void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        // React to health changes - we can detect damage by comparing to previous health
        PlaySound(enemyData.hurtSounds);
        
        // Consider fleeing if health is low and behavior supports it
        if (enemyData.behaviorType == EnemyBehaviorType.Coward && currentHealth < maxHealth * 0.3f)
        {
            ChangeState(EnemyState.Flee);
        }
    }
    
    #endregion
    
    #region Ability Implementations
    
    IEnumerator ChargeAbility(EnemyAbility ability)
    {
        ChangeState(EnemyState.Ability);
        
        if (currentTarget != null)
        {
            Vector3 chargeDirection = (currentTarget.transform.position - transform.position).normalized;
            float chargeSpeed = enemyData.moveSpeed * 3f;
            float chargeTime = ability.duration;
            
            while (chargeTime > 0)
            {
                transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
                chargeTime -= Time.deltaTime;
                yield return null;
            }
        }
        
        ChangeState(EnemyState.Chase);
    }
    
    void HealAbility(EnemyAbility ability)
    {
        if (healthComponent != null)
        {
            int healAmount = Mathf.RoundToInt(ability.power);
            healthComponent.Heal(healAmount);
        }
    }
    
    void DisableTurretAbility(EnemyAbility ability)
    {
        // Find nearby turrets and disable them
        Collider[] turrets = Physics.OverlapSphere(transform.position, ability.range);
        
        foreach (var turret in turrets)
        {
            if (turret.gameObject.CompareTag("Defense"))
            {
                // Disable turret for duration (implement in turret system)
                // This is a placeholder - actual implementation depends on turret system
                Debug.Log($"[{gameObject.name}] Disabled turret: {turret.name} for {ability.duration} seconds");
            }
        }
    }
    
    #endregion
    
    #region Debug Visualization
    
    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        
        // Max chase range
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, enemyData.maxChaseRange);
        
        // Flying height
        if (enemyData.canFly)
        {
            Gizmos.color = Color.cyan;
            Vector3 flyPos = transform.position;
            flyPos.y += enemyData.flyingHeight;
            Gizmos.DrawWireCube(flyPos, Vector3.one * 0.5f);
        }
        
        // Current target line
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
    
    #endregion
}

/// <summary>
/// Enemy AI states
/// </summary>
public enum EnemyState
{
    Idle,       // Standing still, waiting
    Patrol,     // Moving on patrol route
    Chase,      // Pursuing a target
    Attack,     // Attacking a target
    Flee,       // Running away from danger
    Ability,    // Using a special ability
    Stunned,    // Temporarily incapacitated
    Dead        // Enemy has died
}