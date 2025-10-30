using UnityEngine;

/// <summary>
/// Turret behavior types for different AI patterns
/// </summary>
public enum TurretBehaviorType
{
    Standard,           // Basic target and shoot
    AntiAir,           // Prioritizes flying enemies
    Heavy,             // Slow but powerful, prefers high-value targets
    Rapid,             // Fast shooting, lower damage
    Sniper,            // Long range, high damage, slow reload
    Area,              // Area denial with splash damage
    Support            // Buffs nearby turrets or slows enemies
}

/// <summary>
/// Turret target priority system
/// </summary>
public enum TurretTargetPriority
{
    Closest,           // Nearest enemy
    Furthest,          // Enemy closest to goal
    HighestHealth,     // Tankiest enemy
    LowestHealth,      // Weakest enemy (for cleanup)
    FastestMoving,     // Speediest enemy
    Flying,            // Prioritize air units
    HighestThreat      // Based on enemy threat level
}

/// <summary>
/// Projectile behavior for turret shots
/// </summary>
public enum ProjectileBehavior
{
    Instant,           // Hitscan/laser - instant hit
    Linear,            // Straight line projectile
    Guided,            // Tracks target while flying
    Ballistic,         // Arc trajectory with gravity
    Piercing,          // Goes through multiple enemies
    Explosive          // Area damage on impact
}

/// <summary>
/// ScriptableObject containing all turret configuration data
/// Allows for easy creation of different turret types without coding
/// </summary>
[CreateAssetMenu(fileName = "New Turret Data", menuName = "Defense/Turret Data")]
public class TurretData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Display name of the turret")]
    public string turretName = "Basic Turret";
    
    [Tooltip("Description for UI tooltips")]
    [TextArea(3, 5)]
    public string description = "A basic defensive turret.";
    
    [Tooltip("Icon for UI display")]
    public Sprite turretIcon;
    
    [Tooltip("Prefab to spawn when building this turret")]
    public GameObject turretPrefab;
    
    [Header("Stats")]
    [Tooltip("Maximum health of the turret")]
    [Range(50, 2000)] public int maxHealth = 200;
    
    [Tooltip("Armor value - reduces incoming damage")]
    [Range(0, 50)] public int armor = 5;
    
    [Tooltip("Build cost in resources")]
    [Range(10, 1000)] public int buildCost = 50;
    
    [Header("Combat Settings")]
    [Tooltip("Damage per shot")]
    [Range(1, 200)] public int damage = 25;
    
    [Tooltip("Time between shots (seconds)")]
    [Range(0.1f, 10f)] public float fireRate = 1.5f;
    
    [Tooltip("Maximum range to detect and engage enemies")]
    [Range(5f, 50f)] public float range = 15f;
    
    [Tooltip("Accuracy - 1.0 = perfect aim, 0.0 = random")]
    [Range(0f, 1f)] public float accuracy = 0.95f;
    
    [Header("Targeting")]
    [Tooltip("How this turret prioritizes targets")]
    public TurretTargetPriority targetPriority = TurretTargetPriority.Closest;
    
    [Tooltip("Tags this turret can target")]
    public string[] targetableTags = { "Enemy" };
    
    [Tooltip("Can this turret target flying enemies?")]
    public bool canTargetAir = true;
    
    [Tooltip("Can this turret target ground enemies?")]
    public bool canTargetGround = true;
    
    [Tooltip("Require line of sight to target enemies? (uncheck for better detection, may target through walls)")]
    public bool requireLineOfSight = false;
    
    [Header("Rotation")]
    [Tooltip("How fast the turret head rotates (degrees per second)")]
    [Range(30f, 360f)] public float rotationSpeed = 90f;
    
    [Tooltip("Can the turret rotate 360 degrees?")]
    public bool canRotate360 = true;
    
    [Tooltip("Minimum rotation angle (if not 360)")]
    [Range(-180f, 180f)] public float minRotationAngle = -90f;
    
    [Tooltip("Maximum rotation angle (if not 360)")]
    [Range(-180f, 180f)] public float maxRotationAngle = 90f;
    
    [Header("Projectile Settings")]
    [Tooltip("Projectile behavior type")]
    public ProjectileBehavior projectileType = ProjectileBehavior.Linear;
    
    [Tooltip("Projectile prefab to spawn")]
    public GameObject projectilePrefab;
    
    [Tooltip("Projectile speed (if not instant)")]
    [Range(1f, 100f)] public float projectileSpeed = 20f;
    
    [Tooltip("Projectile lifetime before self-destruct")]
    [Range(0.5f, 10f)] public float projectileLifetime = 3f;
    
    [Tooltip("Can projectiles pierce through enemies?")]
    public bool piercing = false;
    
    [Tooltip("Maximum enemies a projectile can hit (if piercing)")]
    [Range(1, 10)] public int maxPierceTargets = 1;
    
    [Header("Area of Effect")]
    [Tooltip("Explosion radius for area damage (0 = no explosion)")]
    [Range(0f, 15f)] public float explosionRadius = 0f;
    
    [Tooltip("Explosion damage (if has explosion)")]
    [Range(0, 100)] public int explosionDamage = 0;
    
    [Header("Behavior")]
    [Tooltip("Turret behavior pattern")]
    public TurretBehaviorType behaviorType = TurretBehaviorType.Standard;
    
    [Tooltip("Does this turret require power to function?")]
    public bool requiresPower = false;
    
    [Tooltip("Power consumption per second")]
    [Range(0, 50)] public int powerConsumption = 5;
    
    [Header("Effects")]
    [Tooltip("Muzzle flash effect when firing")]
    public GameObject muzzleFlashEffect;
    
    [Tooltip("Hit effect when projectile impacts")]
    public GameObject hitEffect;
    
    [Tooltip("Explosion effect (if has explosion)")]
    public GameObject explosionEffect;
    
    [Header("Audio")]
    [Tooltip("Sound when turret fires")]
    public AudioClip fireSound;
    
    [Tooltip("Sound when turret rotates")]
    public AudioClip rotationSound;
    
    [Tooltip("Sound when turret is destroyed")]
    public AudioClip destroySound;
    
    [Header("Upgrades")]
    [Tooltip("Can this turret be upgraded?")]
    public bool canUpgrade = true;
    
    [Tooltip("Next tier turret (if upgradeable)")]
    public TurretData upgradeTo;
    
    [Tooltip("Cost to upgrade")]
    [Range(0, 500)] public int upgradeCost = 25;
    
    /// <summary>
    /// Calculate damage after armor reduction
    /// </summary>
    public int CalculateArmorDamage(int incomingDamage)
    {
        int reducedDamage = Mathf.Max(1, incomingDamage - armor);
        return reducedDamage;
    }
    
    /// <summary>
    /// Get effective range for specific target type
    /// </summary>
    public float GetEffectiveRange(bool isFlying)
    {
        if (isFlying && !canTargetAir) return 0f;
        if (!isFlying && !canTargetGround) return 0f;
        
        // Some turret types might have different ranges for air vs ground
        switch (behaviorType)
        {
            case TurretBehaviorType.AntiAir:
                return isFlying ? range * 1.2f : range * 0.8f;
            case TurretBehaviorType.Sniper:
                return range * 1.5f; // Sniper has extended range
            default:
                return range;
        }
    }
    
    /// <summary>
    /// Get fire rate modifier based on behavior
    /// </summary>
    public float GetEffectiveFireRate()
    {
        switch (behaviorType)
        {
            case TurretBehaviorType.Rapid:
                return fireRate * 0.6f; // Rapid turrets fire faster
            case TurretBehaviorType.Heavy:
                return fireRate * 1.5f; // Heavy turrets fire slower
            case TurretBehaviorType.Sniper:
                return fireRate * 2.0f; // Sniper turrets fire much slower
            default:
                return fireRate;
        }
    }
    
    /// <summary>
    /// Get damage modifier based on behavior and target
    /// </summary>
    public int GetEffectiveDamage(bool isFlying, int enemyThreatLevel = 1)
    {
        float damageMultiplier = 1f;
        
        switch (behaviorType)
        {
            case TurretBehaviorType.AntiAir:
                damageMultiplier = isFlying ? 1.3f : 0.7f; // Bonus vs air, penalty vs ground
                break;
            case TurretBehaviorType.Heavy:
                damageMultiplier = 1.5f; // Heavy turrets do more damage
                break;
            case TurretBehaviorType.Sniper:
                damageMultiplier = 2.0f; // Sniper turrets do high damage
                break;
            case TurretBehaviorType.Rapid:
                damageMultiplier = 0.8f; // Rapid turrets do less damage per shot
                break;
        }
        
        return Mathf.RoundToInt(damage * damageMultiplier);
    }
}