using UnityEngine;

/// <summary>
/// ScriptableObject that defines all properties and behavior for an enemy type.
/// This allows for data-driven enemy creation and easy balancing.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "EFD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Properties")]
    [Tooltip("Display name of the enemy")]
    public string enemyName = "Unknown Enemy";
    
    [Tooltip("Brief description of the enemy")]
    [TextArea(2, 4)]
    public string description = "A mysterious enemy";
    
    [Tooltip("Enemy prefab reference for spawning")]
    public GameObject enemyPrefab;
    
    [Header("Health & Defense")]
    [Tooltip("Maximum health points")]
    [Range(1, 1000)] public int maxHealth = 50;
    
    [Tooltip("Starting health (0 = use max health)")]
    [Range(0, 1000)] public int startingHealth = 0;
    
    [Tooltip("Health regeneration per second (0 = no regen)")]
    [Range(0f, 50f)] public float healthRegenRate = 0f;
    
    [Tooltip("Delay before health regeneration starts")]
    [Range(0f, 10f)] public float healthRegenDelay = 3f;
    
    [Tooltip("Armor value - reduces incoming damage")]
    [Range(0, 50)] public int armor = 0;
    
    [Header("Movement & Speed")]
    [Tooltip("Base movement speed")]
    [Range(0.1f, 20f)] public float moveSpeed = 3f;
    
    [Tooltip("Sprint/charge speed multiplier")]
    [Range(1f, 5f)] public float sprintMultiplier = 1.5f;
    
    [Tooltip("Turn speed (how fast enemy rotates)")]
    [Range(1f, 360f)] public float turnSpeed = 180f;
    
    [Tooltip("Stopping distance from target")]
    [Range(0.1f, 10f)] public float stoppingDistance = 1.5f;
    
    [Header("Combat Stats")]
    [Tooltip("Base attack damage")]
    [Range(1, 200)] public int attackDamage = 10;
    
    [Tooltip("Attack range in units")]
    [Range(0.5f, 20f)] public float attackRange = 2f;
    
    [Tooltip("Time between attacks")]
    [Range(0.1f, 10f)] public float attackCooldown = 2f;
    
    [Tooltip("Damage types this enemy can resist")]
    public DamageType[] resistances;
    
    [Tooltip("Damage types this enemy is weak to")]
    public DamageType[] weaknesses;
    
    [Header("AI Behavior")]
    [Tooltip("Primary behavior type")]
    public EnemyBehaviorType behaviorType = EnemyBehaviorType.Aggressive;
    
    [Tooltip("Detection range for spotting targets")]
    [Range(1f, 50f)] public float detectionRange = 10f;
    
    [Tooltip("Range at which enemy gives up chase")]
    [Range(5f, 100f)] public float maxChaseRange = 25f;
    
    [Header("Target Priority System")]
    [Tooltip("Player detection range")]
    [Range(1f, 50f)] public float playerDetectionRange = 10f;
    
    [Tooltip("Defense/Turret detection range")]
    [Range(1f, 50f)] public float defenseDetectionRange = 8f;
    
    [Tooltip("Sanctum detection range")]
    [Range(5f, 100f)] public float sanctumDetectionRange = 50f;
    
    [Tooltip("Primary target preference - what to attack first when multiple targets are in range")]
    public PrimaryTargetType primaryTarget = PrimaryTargetType.Player;
    
    [Tooltip("Fallback targets in order of priority when primary target unavailable")]
    public TargetPriority[] fallbackTargets = {
        TargetPriority.Defenses,
        TargetPriority.Sanctum,
        TargetPriority.Closest
    };
    
    [Header("Special Abilities")]
    [Tooltip("Special abilities this enemy can use")]
    public EnemyAbility[] abilities;
    
    [Tooltip("Can this enemy fly/hover?")]
    public bool canFly = false;
    
    [Tooltip("Flying height above ground")]
    [Range(0.5f, 20f)] public float flyingHeight = 3f;
    
    [Tooltip("Can this enemy pass through/over walls?")]
    public bool ignoresWalls = false;
    
    [Tooltip("Can this enemy disable turrets/defenses?")]
    public bool canDisableTurrets = false;
    
    [Header("Rewards & Drops")]
    [Tooltip("Souls dropped when killed")]
    [Range(1, 100)] public int soulReward = 5;
    
    [Tooltip("Bonus souls for special kills (headshot, etc.)")]
    [Range(0, 50)] public int bonusSoulReward = 2;
    
    [Tooltip("Resources dropped when killed")]
    public ResourceDrop[] resourceDrops;
    
    [Tooltip("Chance to drop special items")]
    [Range(0f, 1f)] public float specialDropChance = 0.1f;
    
    [Header("Audio & Visual")]
    [Tooltip("Footstep/movement sounds")]
    public AudioClip[] movementSounds;
    
    [Tooltip("Attack sounds")]
    public AudioClip[] attackSounds;
    
    [Tooltip("Hurt/damage sounds")]
    public AudioClip[] hurtSounds;
    
    [Tooltip("Death sound")]
    public AudioClip deathSound;
    
    [Tooltip("Idle ambient sounds")]
    public AudioClip[] ambientSounds;
    
    [Tooltip("Visual effect when spawning")]
    public GameObject spawnEffect;
    
    [Tooltip("Visual effect when dying")]
    public GameObject deathEffect;
    
    [Tooltip("Duration for death effect")]
    [Range(0.1f, 10f)] public float deathEffectDuration = 2f;
    
    [Header("Advanced Settings")]
    [Tooltip("NavMesh agent type (for different sized enemies)")]
    public int navMeshAgentType = 0;
    
    [Tooltip("Layer mask for ground detection (flying enemies)")]
    public LayerMask groundLayer = 1;
    
    [Tooltip("Threat level (affects wave composition)")]
    [Range(1, 10)] public int threatLevel = 1;
    
    [Tooltip("Group size preference (1 = solo, higher = pack hunter)")]
    [Range(1, 10)] public int preferredGroupSize = 1;
    
    [Tooltip("Experience points awarded to nearby enemies when this enemy dies")]
    [Range(0, 50)] public int experienceReward = 0;
    
    /// <summary>
    /// Get effective health (starting health or max health)
    /// </summary>
    public int GetEffectiveStartingHealth()
    {
        return startingHealth > 0 ? startingHealth : maxHealth;
    }
    
    /// <summary>
    /// Check if this enemy has a specific ability
    /// </summary>
    public bool HasAbility(EnemyAbilityType abilityType)
    {
        if (abilities == null) return false;
        
        foreach (var ability in abilities)
        {
            if (ability.abilityType == abilityType)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get ability data for a specific ability type
    /// </summary>
    public EnemyAbility GetAbility(EnemyAbilityType abilityType)
    {
        if (abilities == null) return null;
        
        foreach (var ability in abilities)
        {
            if (ability.abilityType == abilityType)
                return ability;
        }
        return null;
    }
    
    /// <summary>
    /// Calculate damage after resistances and weaknesses
    /// </summary>
    public int CalculateDamage(int baseDamage, DamageType damageType)
    {
        float multiplier = 1f;
        
        // Check resistances
        if (resistances != null)
        {
            foreach (var resistance in resistances)
            {
                if (resistance == damageType)
                {
                    multiplier *= 0.5f; // 50% damage reduction
                    break;
                }
            }
        }
        
        // Check weaknesses
        if (weaknesses != null)
        {
            foreach (var weakness in weaknesses)
            {
                if (weakness == damageType)
                {
                    multiplier *= 2f; // 200% damage (double damage)
                    break;
                }
            }
        }
        
        // Apply armor reduction
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        finalDamage = Mathf.Max(1, finalDamage - armor); // Armor reduces damage, minimum 1
        
        return finalDamage;
    }
}

/// <summary>
/// Types of enemy behaviors
/// </summary>
public enum EnemyBehaviorType
{
    Aggressive,     // Attacks anything in range
    Defensive,      // Only attacks when attacked
    Coward,         // Flees when health is low
    Berserker,      // Gets faster/stronger when damaged
    Tactical,       // Uses cover and positioning
    Swarm,          // Coordinated group attacks
    Assassin,       // Targets player specifically
    Siege,          // Focuses on destroying defenses
    Support         // Buffs other enemies
}

/// <summary>
/// Primary target preference for enemy AI
/// </summary>
public enum PrimaryTargetType
{
    Player,         // Prioritize attacking the player
    Defenses,       // Prioritize attacking turrets/walls
    Sanctum,        // Go straight for the main objective
    Closest         // Attack whatever is nearest
}

/// <summary>
/// Target priority for enemy AI (fallback system)
/// </summary>
public enum TargetPriority
{
    Player,         // Attack the player first
    Defenses,       // Attack turrets/walls first
    Sanctum,        // Go straight for the objective
    Wounded,        // Target lowest health enemies first
    Closest,        // Attack whatever is nearest
    Strongest       // Target highest threat first
}

/// <summary>
/// Types of damage for resistances/weaknesses
/// </summary>
public enum DamageType
{
    Physical,       // Sword, arrow, blunt
    Fire,           // Fire spells, burning
    Ice,            // Frost spells, freezing
    Lightning,      // Electric attacks
    Poison,         // Damage over time
    Holy,           // Light/divine damage
    Dark,           // Shadow/necromantic damage
    Explosive       // Blast damage
}

/// <summary>
/// Enemy special abilities
/// </summary>
[System.Serializable]
public class EnemyAbility
{
    [Tooltip("Type of ability")]
    public EnemyAbilityType abilityType;
    
    [Tooltip("Cooldown between uses")]
    [Range(1f, 60f)] public float cooldown = 10f;
    
    [Tooltip("Range of the ability")]
    [Range(1f, 20f)] public float range = 5f;
    
    [Tooltip("Duration of the effect")]
    [Range(0.1f, 30f)] public float duration = 3f;
    
    [Tooltip("Power/intensity of the ability")]
    [Range(1f, 100f)] public float power = 10f;
    
    [Tooltip("Visual effect for the ability")]
    public GameObject effect;
    
    [Tooltip("Sound effect for the ability")]
    public AudioClip sound;
}

/// <summary>
/// Types of special abilities enemies can have
/// </summary>
public enum EnemyAbilityType
{
    Charge,         // Rush attack with extra damage
    Heal,           // Restore health to self or allies
    Buff,           // Increase allies' stats
    Debuff,         // Weaken player/defenses
    Teleport,       // Instant movement
    Shield,         // Temporary damage immunity
    Summon,         // Spawn additional enemies
    DisableTurret,  // Temporarily disable defenses
    Explode,        // Death explosion
    Regenerate,     // Fast health recovery
    Stealth,        // Become invisible
    Fear,           // Cause player control issues
    EMP,            // Disable electronic defenses
    Leap,           // Jump over walls
    Swarm           // Call nearby enemies
}

/// <summary>
/// Resource drops from enemies
/// </summary>
[System.Serializable]
public class ResourceDrop
{
    [Tooltip("Type of resource")]
    public ResourceType resourceType;
    
    [Tooltip("Minimum amount dropped")]
    [Range(0, 100)] public int minAmount = 1;
    
    [Tooltip("Maximum amount dropped")]
    [Range(1, 100)] public int maxAmount = 3;
    
    [Tooltip("Chance to drop this resource")]
    [Range(0f, 1f)] public float dropChance = 0.5f;
}

/// <summary>
/// Types of resources enemies can drop
/// </summary>
public enum ResourceType
{
    Bone,           // Basic construction material
    Essence,        // Magical component
    Crystal,        // Advanced technology
    Metal,          // Weapon/armor materials
    Wood,           // Building materials
    Stone,          // Wall construction
    Rune,           // Magical enhancement
    Gem,            // Valuable currency
    Herb,           // Healing materials
    Oil             // Mechanical lubricant
}