# Weapon System Documentation

The Weapon System provides a comprehensive, data-driven framework for all weapon types with upgrade capabilities and modular design.

## 📊 Overview

The Weapon System consists of:
- **WeaponData ScriptableObjects**: Data-driven weapon configuration
- **BaseWeapon**: Core weapon functionality 
- **Specialized Weapons**: Melee, Ranged, and Magic weapon types
- **Upgrade System**: Weapon enhancement and progression
- **Pickup System**: Weapon collection and switching

## 🔧 Core Architecture

### WeaponData.cs (ScriptableObject)
Central configuration for all weapon properties:

**Basic Properties:**
- Weapon name, type, prefab, and icon
- Attack damage, speed, delay, and distance
- Layer masks and targeting configuration

**Audio & Effects:**
- Attack sounds, hit sounds, swing sounds
- Hit effects and particle systems
- Animation trigger names

**Weapon-Specific Settings:**
- Melee: Slash range, combo system
- Ranged: Projectile speed, arrow prefabs, gravity
- Magic: Explosion radius, projectile lifetime

### BaseWeapon.cs (Abstract)
Foundation class providing common weapon functionality:

**Core Features:**
- Attack timing and cooldown management
- Audio playback and effect spawning
- Upgrade value calculations
- Event system for animation/state changes

**Protected Methods:**
- `PerformAttackRaycast()` - Execute weapon attack
- `OnHit(RaycastHit hit)` - Handle collision detection
- `DealDamage(Actor target)` - Apply damage to targets
- `CalculateFinalDamage()` - Calculate damage with modifiers

## ⚔️ Weapon Types

### Melee Weapons
Close-combat weapons like swords and axes.

**Features:**
- Combo attack system
- Slash range detection  
- Weapon-specific swing sounds
- Multiple attack animations

**Example Setup:**
```csharp
// In WeaponData
weaponType = WeaponType.Melee;
slashRange = 1.5f;
canCombo = true;
comboWindow = 1f;
swingSounds = [swing1, swing2, swing3];
```

### Ranged Weapons  
Projectile-based weapons like bows and crossbows.

**Features:**
- Draw and release mechanics
- Projectile physics with gravity
- Arrow spawn point configuration
- Charge-based damage scaling

**Example Setup:**
```csharp
// In WeaponData
weaponType = WeaponType.Ranged;
arrowPrefab = arrowPrefab;
projectileSpeed = 30f;
useGravity = true;
canChargeDraw = true;
```

### Magic Weapons
Spell-casting weapons like staffs and wands.

**Features:**
- Explosive projectiles
- Area of effect damage
- Projectile lifetime control
- Distance-based damage falloff

**Example Setup:**
```csharp
// In WeaponData  
weaponType = WeaponType.Magic;
projectilePrefab = magicProjectile;
explosionRadius = 5f;
explosionDamage = 25;
projectileLifetime = 3f;
```

## 📈 Upgrade System

### Upgrade Categories

**Attack Timing:**
- `baseAttackSpeed` - Time between attacks
- `baseAttackDelay` - Delay before attack executes
- `comboCooldown` - Delay between combo attacks

**Damage Enhancements:**
- `bonusDamage` - Additional damage on top of base
- `criticalChance` - Critical hit probability (0-1)
- `criticalMultiplier` - Critical damage multiplier
- `comboDamageMultiplier` - Combo attack damage bonus

**Special Abilities:**
- `statusEffectChance` - Chance to apply effects
- `lifestealPercentage` - Health recovery on hit
- `canPenetrate` - Weapon pierces through enemies
- `penetrationCount` - Number of enemies to pierce

**Resource Management:**
- `staminaCost` - Stamina required per attack
- `manaCost` - Mana required per attack
- `durabilityLoss` - Durability reduction per use

## 🔧 Setup Instructions

### Creating New Weapons

1. **Create WeaponData:**
   ```
   Right-click in Project → Create → EFD → Weapon Data
   ```

2. **Configure Basic Properties:**
   - Set weapon name, type, and prefab
   - Configure damage, speed, and range
   - Assign audio clips and effects

3. **Set Weapon-Specific Settings:**
   - Melee: Configure slash range and combos
   - Ranged: Set up arrow prefab and physics
   - Magic: Configure explosion properties

4. **Create Weapon Prefab:**
   - Add appropriate weapon script (MeleeWeapon, RangedWeapon, MagicWeapon)
   - Assign the WeaponData ScriptableObject
   - Configure spawn points for projectiles if needed

### Weapon Positioning
Use the WeaponSlotSetup system for precise weapon positioning:

```csharp
// In WeaponData
weaponSlotPosition = new Vector3(0.1f, -0.2f, 0.3f);
weaponSlotRotation = new Vector3(0, 15, 0);
weaponSlotScale = Vector3.one;
```

## 🎮 Integration

### With Player Controller
```csharp
public class FirstPersonController : MonoBehaviour
{
    private BaseWeapon currentWeapon;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            currentWeapon.Attack(playerCamera);
        }
    }
    
    public void SwitchWeapon(BaseWeapon newWeapon)
    {
        currentWeapon = newWeapon;
    }
}
```

### With Pickup System
```csharp
public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponData;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<FirstPersonController>(out var controller))
        {
            controller.PickupWeapon(weaponData);
        }
    }
}
```

## 🔔 Events System

Weapons provide event notifications for external systems:

```csharp
// Subscribe to weapon events
weapon.OnAnimationChange += (animationName) => PlayAnimation(animationName);
weapon.OnAttackStateChange += (isAttacking) => UpdateUI(isAttacking);
```

## 🧪 Testing & Debugging

### WeaponSlotSetup Tool
Visual tool for positioning weapons in the player's hand:

**Features:**
- Real-time position adjustment
- Visual gizmos for spawn points
- Save/load positioning to WeaponData
- Preview weapon placement

**Usage:**
1. Add `WeaponSlotSetup` to weapon GameObject
2. Adjust position/rotation in inspector
3. Click "Save to WeaponData" when satisfied

### Debug Visualizations
All weapons include editor gizmos:
- Attack range visualization
- Projectile spawn points
- Explosion radius preview
- Trajectory indicators

## ⚙️ Advanced Configuration

### Layer Masks
Configure what objects weapons can hit:
```csharp
// In WeaponData
attackLayer = LayerMask.GetMask("Enemy", "Destructible");
```

### Animation Integration
Weapons trigger animations through events:
```csharp
// In WeaponData
attackAnimations = new string[] { "Attack 1", "Attack 2", "Power Attack" };
```

### Audio Management
Comprehensive audio support:
```csharp
// In WeaponData
attackSounds = [attack1, attack2];     // Random attack sounds
swingSounds = [swing1, swing2];        // Melee swing sounds
hitSound = hitImpact;                  // Impact sound
```

## 🐛 Known Issues & Limitations

### Current Limitations
- Maximum 20 upgrade levels per weapon
- No weapon durability system implementation
- Limited to 3 weapon types (Melee, Ranged, Magic)

### Performance Considerations
- Use object pooling for frequently spawned projectiles
- Limit maximum simultaneous weapons for performance
- Cache weapon data references to avoid repeated lookups

## 🔄 Future Enhancements

Planned improvements:
- Weapon crafting and modification system
- Additional weapon types (thrown, area-effect)
- Advanced combo system with timing windows
- Weapon mastery and skill trees

---
*Last Updated: October 28, 2025*