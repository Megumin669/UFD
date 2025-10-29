using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "EFD/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName = "New Weapon";
    public WeaponType weaponType = WeaponType.Melee;
    public GameObject weaponPrefab;
    public Sprite weaponIcon;
    
    [Header("Combat Stats")]
    [Range(1, 100)] public int attackDamage = 10;
    [Range(0.1f, 10f)] public float attackSpeed = 1f;
    [Range(0.1f, 2f)] public float attackDelay = 0.4f;
    [Range(1f, 50f)] public float attackDistance = 3f;
    public LayerMask attackLayer = -1;
    
    [Header("Damage Target Configuration")]
    [Tooltip("Objects with these tags will receive damage from this weapon. Leave empty to damage all objects with Health or Actor components.")]
    public string[] damageableTags = { "Player" };
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    public AudioClip hitSound;
    
    [Header("Melee Audio")]
    public AudioClip[] swingSounds;
    public AudioClip[] comboSwingSounds;
    
    [Header("Effects")]
    public GameObject hitEffect;
    [Range(1f, 20f)] public float hitEffectDuration = 10f;
    
    [Header("Animation")]
    public string[] attackAnimations = { "Attack 1", "Attack 2" };
    
    [Header("Weapon Specific Settings")]
    [Space(10)]
    
    // Ranged weapon settings (Bow, Crossbow, etc.)
    [Header("Ranged Weapon Settings (if applicable)")]
    public GameObject arrowPrefab;
    [Range(10f, 100f)] public float projectileSpeed = 30f;
    [Range(0f, 45f)] public float maxDrawAngle = 30f;
    [Range(0.5f, 3f)] public float maxDrawTime = 2f;
    [Range(0.1f, 2f)] public float minDrawTime = 0.3f;
    public bool useGravity = true;
    [Range(0f, 5f)] public float gravityMultiplier = 1f;
    public bool canChargeDraw = true;
    public AudioClip drawSound;
    public AudioClip releaseSound;
    
    [Header("Ranged Spawn Point")]
    [Tooltip("Name of child object to use as arrow spawn point (e.g., 'ArrowSpawnPoint')")]
    public string arrowSpawnPointName = "ArrowSpawnPoint";
    
    // Magic weapon settings (Staff, Wand, etc.)
    [Header("Magic Weapon Settings (if applicable)")]
    public GameObject projectilePrefab;
    [Range(1f, 50f)] public float staffProjectileSpeed = 15f;
    [Range(1f, 20f)] public float explosionRadius = 5f;
    [Range(1, 100)] public int explosionDamage = 25;
    [Tooltip("Enable damage falloff based on distance from explosion center")]
    public bool useDamageFalloff = true;
    [Tooltip("Minimum damage percentage at explosion edge (0.5 = 50% damage at edge)")]
    [Range(0f, 1f)] public float minimumDamageMultiplier = 0.5f;
    public LayerMask explosionLayer = -1;
    public GameObject explosionEffect;
    [Range(1f, 10f)] public float explosionEffectDuration = 5f;
    public AudioClip castSound;
    public AudioClip explosionSound;
    
    [Header("Magic Projectile Settings")]
    [Tooltip("How long the projectile travels before self-destructing (longer = farther distance)")]
    [Range(0.5f, 10f)] public float projectileLifetime = 3f;
    
    [Header("Magic Spawn Point")]
    [Tooltip("Name of child object to use as projectile spawn point (e.g., 'ProjectileSpawnPoint')")]
    public string projectileSpawnPointName = "ProjectileSpawnPoint";
    
    // Melee weapon settings (Sword, Axe, etc.)
    [Header("Melee Weapon Settings (if applicable)")]
    [Range(0.5f, 3f)] public float slashRange = 1.5f;
    public bool canCombo = true;
    [Range(0.5f, 3f)] public float comboWindow = 1f;
    
    [Header("WeaponSlot Positioning")]
    public Vector3 weaponSlotPosition = Vector3.zero;
    public Vector3 weaponSlotRotation = Vector3.zero;
    public Vector3 weaponSlotScale = Vector3.one;
    
    [Header("Weapon Upgrade System")]
    [Tooltip("Base values that can be modified by upgrade system")]
    [Space(5)]
    
    [Header("Attack Timing Upgrades")]
    [Range(0.05f, 5f)] public float baseAttackSpeed = 1f;
    [Tooltip("Time between attacks (lower = faster attacks)")]
    [Range(0.05f, 3f)] public float baseAttackDelay = 0.4f;
    [Tooltip("Delay between combo attacks")]
    [Range(0.1f, 2f)] public float comboCooldown = 0.2f;
    
    [Header("Damage Upgrades")]
    [Tooltip("Additional damage on top of base attackDamage")]
    [Range(0, 100)] public int bonusDamage = 0;
    [Tooltip("Critical hit chance (0-1)")]
    [Range(0f, 1f)] public float criticalChance = 0.1f;
    [Tooltip("Critical hit damage multiplier")]
    [Range(1.1f, 5f)] public float criticalMultiplier = 2f;
    [Tooltip("Damage multiplier for combo attacks")]
    [Range(1f, 3f)] public float comboDamageMultiplier = 1.2f;
    

    
    [Header("Special Abilities")]
    [Tooltip("Chance to apply status effects on hit")]
    [Range(0f, 1f)] public float statusEffectChance = 0f;
    [Tooltip("Lifesteal percentage (0-1)")]
    [Range(0f, 0.5f)] public float lifestealPercentage = 0f;
    [Tooltip("Weapon can penetrate through enemies")]
    public bool canPenetrate = false;
    [Tooltip("Number of enemies weapon can penetrate")]
    [Range(1, 10)] public int penetrationCount = 1;
    
    [Header("Resource Management")]
    [Tooltip("Stamina cost per attack (0 = no cost)")]
    [Range(0f, 50f)] public float staminaCost = 0f;
    [Tooltip("Mana cost per attack (0 = no cost)")]
    [Range(0f, 100f)] public float manaCost = 0f;
    [Tooltip("Durability loss per attack (0 = no durability system)")]
    [Range(0f, 10f)] public float durabilityLoss = 0f;
    
    [Header("Upgrade Limits")]
    [Tooltip("Maximum upgrade level for this weapon")]
    [Range(1, 20)] public int maxUpgradeLevel = 10;
    [Tooltip("Current upgrade level (runtime)")]
    [Range(0, 20)] public int currentUpgradeLevel = 0;
    
    [Header("Pickup Settings")]
    public GameObject pickupPrefab;
    [Range(0.1f, 5f)] public float pickupRotationSpeed = 1f;
    [Range(0.1f, 2f)] public float pickupBobSpeed = 1f;
    [Range(0.1f, 1f)] public float pickupBobHeight = 0.2f;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "A powerful weapon for combat.";
}

public enum WeaponType
{
    Melee,
    Ranged,
    Magic
}