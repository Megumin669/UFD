# ScriptableObjects Documentation

Data-driven architecture using Unity's ScriptableObject system for flexible and maintainable game configuration.

## 📊 Overview

The ScriptableObjects system provides:
- **WeaponData**: Complete weapon configuration
- **Data-Driven Design**: Separate data from logic
- **Inspector Integration**: Easy editing in Unity Editor
- **Runtime Flexibility**: Modify behavior without code changes

## 🔧 Core ScriptableObjects

### WeaponData.cs
Central configuration ScriptableObject for all weapons.

**Creation:**
```
Right-click in Project → Create → EFD → Weapon Data
```

**Key Sections:**

#### Basic Information
```csharp
[Header("Basic Info")]
public string weaponName = "New Weapon";
public WeaponType weaponType = WeaponType.Melee;
public GameObject weaponPrefab;
public Sprite weaponIcon;
```

#### Combat Statistics
```csharp
[Header("Combat Stats")]
[Range(1, 100)] public int attackDamage = 10;
[Range(0.1f, 10f)] public float attackSpeed = 1f;
[Range(0.1f, 2f)] public float attackDelay = 0.4f;
[Range(1f, 50f)] public float attackDistance = 3f;
public LayerMask attackLayer = -1;
```

#### Damage Targeting
```csharp
[Header("Damage Target Configuration")]
[Tooltip("Objects with these tags will receive damage from this weapon")]
public string[] damageableTags = { "Player" };
```

#### Audio Configuration
```csharp
[Header("Audio")]
public AudioClip[] attackSounds;
public AudioClip hitSound;

[Header("Melee Audio")]
public AudioClip[] swingSounds;
public AudioClip[] comboSwingSounds;
```

#### Visual Effects
```csharp
[Header("Effects")]
public GameObject hitEffect;
[Range(1f, 20f)] public float hitEffectDuration = 10f;
```

#### Weapon-Specific Settings

**Melee Weapons:**
```csharp
[Header("Melee Weapon Settings")]
[Range(0.5f, 3f)] public float slashRange = 1.5f;
public bool canCombo = true;
[Range(0.5f, 3f)] public float comboWindow = 1f;
```

**Ranged Weapons:**
```csharp
[Header("Ranged Weapon Settings")]
public GameObject arrowPrefab;
[Range(10f, 100f)] public float projectileSpeed = 30f;
[Range(0f, 45f)] public float maxDrawAngle = 30f;
public bool useGravity = true;
```

**Magic Weapons:**
```csharp
[Header("Magic Weapon Settings")]
public GameObject projectilePrefab;
[Range(1f, 50f)] public float staffProjectileSpeed = 15f;
[Range(1f, 20f)] public float explosionRadius = 5f;
[Range(1, 100)] public int explosionDamage = 25;
[Range(0.5f, 10f)] public float projectileLifetime = 3f;
```

#### Upgrade System
```csharp
[Header("Weapon Upgrade System")]
[Range(0, 100)] public int bonusDamage = 0;
[Range(0f, 1f)] public float criticalChance = 0.1f;
[Range(1.1f, 5f)] public float criticalMultiplier = 2f;
[Range(0f, 1f)] public float statusEffectChance = 0f;
[Range(0f, 0.5f)] public float lifestealPercentage = 0f;
```

#### Positioning Data
```csharp
[Header("WeaponSlot Positioning")]
public Vector3 weaponSlotPosition = Vector3.zero;
public Vector3 weaponSlotRotation = Vector3.zero;
public Vector3 weaponSlotScale = Vector3.one;
```

## 🎯 Design Patterns

### Data-Driven Architecture
Separates configuration from implementation:

```csharp
// Logic in MonoBehaviour
public class MeleeWeapon : BaseWeapon
{
    public WeaponData weaponData;
    
    void ApplyWeaponData(WeaponData data)
    {
        attackDamage = data.attackDamage;
        attackSpeed = data.attackSpeed;
        // Apply all data-driven values
    }
}

// Data in ScriptableObject
// No logic, only configuration values
```

### Inspector Integration
ScriptableObjects provide rich editor experience:

**Custom Attributes:**
```csharp
[Range(0.1f, 10f)] public float attackSpeed = 1f;     // Slider control
[TextArea(3, 5)] public string description;           // Multi-line text
[Tooltip("Damage per attack")] public int damage;     // Hover help
[Header("Combat Stats")] public int attackDamage;     // Section headers
```

**Conditional Fields:**
```csharp
// Show field only for specific weapon types
[ConditionalField("weaponType", WeaponType.Ranged)]
public GameObject arrowPrefab;
```

## 🔧 Usage Patterns

### Creating New Weapons
1. **Create WeaponData Asset:**
   ```
   Right-click → Create → EFD → Weapon Data
   ```

2. **Configure All Settings:**
   - Basic info (name, type, prefab)
   - Combat stats (damage, speed, range)
   - Audio clips and effects
   - Weapon-specific settings
   - Upgrade parameters

3. **Reference in Weapon Script:**
   ```csharp
   public class SwordWeapon : MeleeWeapon
   {
       [SerializeField] private WeaponData weaponData;
       
       void Start()
       {
           ApplyWeaponData(weaponData);
       }
   }
   ```

### Runtime Modification
ScriptableObjects can be modified at runtime:

```csharp
// Modify weapon data at runtime
weaponData.attackDamage += upgradeBonus;
weaponData.attackSpeed *= speedMultiplier;

// Changes persist until scene reload
// Use Object.Instantiate() for permanent copies
```

### Asset Management
Organize ScriptableObjects effectively:

**Folder Structure:**
```
Assets/
├── Data/
│   ├── Weapons/
│   │   ├── Melee/
│   │   │   ├── Sword_Data.asset
│   │   │   └── Axe_Data.asset
│   │   ├── Ranged/
│   │   │   └── Bow_Data.asset
│   │   └── Magic/
│   │       └── Staff_Data.asset
```

## 🔔 Events & Integration

### Weapon System Integration
WeaponData integrates with all weapon components:

```csharp
// BaseWeapon applies ScriptableObject data
protected virtual void ApplyUpgradeValues(WeaponData data)
{
    attackDamage = data.attackDamage + data.bonusDamage;
    attackSpeed = Mathf.Max(0.1f, data.baseAttackSpeed);
    criticalChance = data.criticalChance;
    // Apply all configuration values
}
```

### Pickup System Integration
```csharp
public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponToGive;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<FirstPersonController>(out var controller))
        {
            controller.GiveWeapon(weaponToGive);
        }
    }
}
```

## ⚙️ Advanced Features

### Validation
ScriptableObjects can include validation:

```csharp
void OnValidate()
{
    // Ensure attack speed is reasonable
    attackSpeed = Mathf.Clamp(attackSpeed, 0.1f, 10f);
    
    // Validate weapon-specific settings
    if (weaponType == WeaponType.Ranged && arrowPrefab == null)
    {
        Debug.LogWarning($"Ranged weapon '{weaponName}' missing arrow prefab!");
    }
}
```

### Default Values
Provide sensible defaults for new weapons:

```csharp
[CreateAssetMenu(fileName = "New Weapon Data", menuName = "EFD/Weapon Data")]
public class WeaponData : ScriptableObject
{
    // Provide good default values
    [Header("Basic Info")]
    public string weaponName = "New Weapon";
    public WeaponType weaponType = WeaponType.Melee;
    
    [Header("Combat Stats")]
    [Range(1, 100)] public int attackDamage = 10;      // Reasonable starting damage
    [Range(0.1f, 10f)] public float attackSpeed = 1f;  // Standard attack speed
}
```

### Asset References
Link related assets efficiently:

```csharp
// Reference other assets
public GameObject weaponPrefab;        // 3D model
public Sprite weaponIcon;              // UI icon
public AudioClip[] attackSounds;       // Sound effects
public GameObject hitEffect;           // Particle effects
```

## 🐛 Common Issues & Solutions

### Issue: Values Not Updating
**Problem:** Changes to ScriptableObject not reflected in game
**Solution:** Ensure weapon scripts call `ApplyWeaponData()` properly

### Issue: Missing References
**Problem:** Null reference exceptions with asset references
**Solution:** Use validation and null checks:
```csharp
void OnValidate()
{
    if (weaponPrefab == null)
        Debug.LogWarning($"Weapon '{weaponName}' missing prefab reference!");
}
```

### Issue: Runtime Changes Not Persisting
**Problem:** Runtime modifications lost on scene reload
**Solution:** Create asset instances for permanent changes:
```csharp
// Create instance for runtime modification
WeaponData runtimeData = Object.Instantiate(originalWeaponData);
runtimeData.attackDamage += upgrade;
```

## 🔄 Best Practices

### Organization
- **Consistent Naming**: Use clear, descriptive names
- **Folder Structure**: Organize by weapon type
- **Version Control**: Include .meta files for proper references

### Performance
- **Reference Caching**: Cache frequently accessed data
- **Minimal Runtime Changes**: Avoid frequent ScriptableObject modifications
- **Asset Loading**: Use Resources.Load() judiciously

### Maintainability
- **Documentation**: Add tooltips and descriptions
- **Validation**: Include OnValidate() methods
- **Default Values**: Provide sensible defaults for all fields

## 🔄 Future Enhancements

Planned ScriptableObject improvements:
- **Custom Editors**: Advanced inspector layouts
- **Asset Bundles**: Dynamic weapon loading
- **Serialization**: Custom serialization for complex data
- **Validation System**: Comprehensive asset validation

---
*Last Updated: October 28, 2025*