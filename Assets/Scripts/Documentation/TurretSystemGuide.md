# Turret System - Complete Implementation Guide

## System Overview

The turret system uses a ScriptableObject-based architecture similar to the weapon and enemy systems, providing maximum flexibility and easy configuration without coding.

### Core Components

1. **TurretData.cs** - ScriptableObject containing all turret configuration
2. **Turret.cs** - Main turret component handling AI, targeting, and shooting
3. **TurretProjectile.cs** - Projectile component with physics-free movement

## Quick Setup Steps

### Step 1: Create TurretData Asset
1. Right-click in Project → Create → Defense → Turret Data
2. Name it (e.g., "BallistaTurretData")
3. Configure all settings (see configuration examples below)

### Step 2: Create Turret Prefab
1. Create GameObject hierarchy:
   ```
   MyTurret (Root)
   ├── TurretBase (Static)
   ├── TurretHead (Rotating)
   │   └── ProjectileSpawnPoint
   ```
2. Add required components to root:
   - Turret (Script)
   - Health (Script)  
   - AudioSource
   - Collider

3. Assign references in Turret component:
   - Turret Data: [Your TurretData asset]
   - Turret Head: [TurretHead transform]
   - Projectile Spawn Point: [ProjectileSpawnPoint transform]

### Step 3: Create Projectile Prefab
1. Create GameObject with TurretProjectile component
2. Add visual model (mesh pointing forward)
3. Optional: Add TrailRenderer, Light, etc.
4. Assign to TurretData.projectilePrefab

### Step 4: Test and Configure
1. Place turret in scene
2. Add enemies to test targeting
3. Adjust TurretData settings as needed
4. Fine-tune rotation speed, damage, range, etc.

## Configuration Examples

### Basic Ballista Turret
```csharp
// TurretData Configuration
turretName = "Ballista"
description = "Balanced anti-ground turret"

// Stats
maxHealth = 250
armor = 3
buildCost = 75

// Combat  
damage = 40
fireRate = 1.2f
range = 18f
accuracy = 0.9f

// Targeting
targetPriority = TurretTargetPriority.Closest
canTargetAir = false
canTargetGround = true

// Projectile
projectileType = ProjectileBehavior.Linear
projectileSpeed = 25f
piercing = false
```

### Anti-Air Turret
```csharp
turretName = "Flak Cannon"
behaviorType = TurretBehaviorType.AntiAir
targetPriority = TurretTargetPriority.Flying
canTargetAir = true
canTargetGround = false
explosionRadius = 3f
explosionDamage = 20
projectileType = ProjectileBehavior.Explosive
```

### Sniper Turret
```csharp
turretName = "Sniper Tower"
behaviorType = TurretBehaviorType.Sniper
damage = 80
fireRate = 3.0f  // Slow but powerful
range = 30f      // Long range
accuracy = 0.98f // High accuracy
projectileType = ProjectileBehavior.Instant // Hitscan
```

### Rapid Fire Turret
```csharp
turretName = "Gatling Gun"
behaviorType = TurretBehaviorType.Rapid
damage = 15      // Low damage per shot
fireRate = 0.3f  // Very fast firing
accuracy = 0.7f  // Lower accuracy
projectileSpeed = 40f // Fast projectiles
```

### Mortar Turret
```csharp
turretName = "Mortar"
behaviorType = TurretBehaviorType.Mortar
damage = 50      // High damage
fireRate = 2.5f  // Slow firing (reload time)
range = 25f      // Long range
explosionRadius = 5f    // Large area damage
explosionDamage = 30    // Good splash damage
projectileType = ProjectileBehavior.Ballistic // Arc trajectory
arcHeightMultiplier = 1.5f // Moderate arc (1=low, 2=medium, 3=high)
accuracy = 0.85f // Decent accuracy
projectileSpeed = 15f // Moderate speed

// Mortars use enhanced collision detection:
// - Larger collision sphere (0.5 radius)
// - Backup OverlapSphere check
// - Proximity detonation (explodes within 1 unit of target)
```

## Turret Behavior Types

### Standard
- Basic target and shoot behavior
- No special modifiers
- Good all-around choice

### AntiAir
- +30% damage vs flying enemies
- -30% damage vs ground enemies
- +20% range vs flying enemies

### Heavy
- +50% damage
- +50% fire rate (slower)
- Prefers high-value targets

### Rapid
- -40% fire rate (faster)
- -20% damage per shot
- Lower accuracy for balance

### Sniper
- +100% damage
- +100% fire rate (much slower)
- +50% range
- Very high accuracy

### Area
- Explosion-based damage
- Good against groups
- Area denial capabilities

### Support
- Special abilities
- Buffs nearby turrets
- Slows enemies

### Mortar
- +20% damage
- +30% fire rate (slower)
- High arc trajectory shots
- Best with area damage
- Can fire over obstacles

## Projectile Behaviors

### Instant (Hitscan/Laser)
- Immediate hit detection
- Perfect for sniper turrets
- Creates visual laser effect
- No travel time

### Linear
- Straight line movement
- Most common projectile type
- Predictable trajectory
- Good for balanced turrets

### Guided
- Tracks target while flying
- Good against fast enemies
- Slightly slower than linear
- More expensive computationally

### Ballistic
- Arc trajectory with gravity
- Realistic physics simulation
- Perfect for mortar turrets
- **Balanced arc that still hits enemies effectively**
- Can fire over obstacles and walls
- Arc height controlled by arcHeightMultiplier:
  - **1.0** = Low arc, fast and accurate
  - **1.5** = Medium arc (recommended for most mortars)
  - **2.0-3.0** = High arc, dramatic but slower
- Base angle: 45° (optimal trajectory)
- Includes height boost for visible arc

### Piercing
- Goes through multiple enemies
- Configurable max targets
- Great against groups
- Higher damage potential

### Explosive
- **Explodes on contact with ANY surface** (terrain, enemies, walls)
- Area damage on impact
- Splash damage radius
- Perfect for mortar and area-denial turrets
- Good crowd control

## Target Priority Options

### Closest
- Targets nearest enemy
- Most common choice
- Good for defensive positioning

### Furthest
- Targets enemy closest to goal
- Good for area denial
- Strategic positioning

### HighestHealth
- Focuses on tanky enemies
- Good for heavy turrets
- Efficient damage dealing

### LowestHealth
- Cleanup weak enemies
- Good for rapid fire turrets
- Finishing off damaged enemies

### FastestMoving
- Counters speed runners
- Good for guided projectiles
- Prevents enemy rushes

### Flying
- Prioritizes air units
- Essential for anti-air turrets
- Ignores ground when possible

### HighestThreat
- Based on enemy threat level
- Most strategic option
- Requires threat system

## Integration Guidelines

### With Existing Systems

**Health System:**
- Turrets use Health component
- Automatic health configuration from TurretData
- Supports damage events and death handling

**Enemy System:**
- Compatible with Enemy.cs and legacy Actor.cs
- Respects enemy flying state
- Uses enemy threat levels for targeting

**Audio System:**
- Requires AudioSource component
- Automatic sound playing
- 3D spatial audio support

### Performance Considerations

**Target Scanning:**
- Scans every 0.2 seconds (5 FPS)
- Adjustable TARGET_SCAN_INTERVAL
- Uses Physics.OverlapSphere for efficiency

**Line of Sight:**
- Optional raycast checking
- Can be disabled for performance
- Configurable per turret type

**Projectile Optimization:**
- Automatic cleanup after lifetime
- Pooling system can be added
- LOD system for distant turrets

## Debugging and Testing

### Debug Features

**Turret Component:**
- **showDebugInfo**: Comprehensive console logging system
  - Operational status checks (Health/Power/TurretData validation)
  - Target detection and tag validation
  - Fire rate cooldown progress
  - Aim angle calculations
  - Shooting events
- **showRangeInScene**: Visual range indicators in Scene view
- Target line visualization
- Rotation constraint visualization

**TurretProjectile Component:**
- **showDebugInfo**: Projectile logging
- Movement direction visualization
- Explosion radius preview
- Hit detection debugging

### Debug Logging Output

When `showDebugInfo` is enabled, you'll see detailed logs for troubleshooting:

**Operational Status:**
```
[Turret] Turret NOT operational - Health: True, Power: True, Data: False
```
→ Fix: Assign TurretData ScriptableObject

**Tag Validation:**
```
[Turret] Tag mismatch: Enemy has 'Enemy', expected one of: [EnemyUnit]
```
→ Fix: Update TurretData.targetableTags or enemy tags

**Target Detection:**
```
[Turret] Found valid target: Skeleton(Clone) at distance 12.5
[Turret] Lost target or target became invalid
```
→ Normal operation logging

**Shooting System:**
```
[Turret] Cooldown: 0.85/1.20
[Turret] Aim check: angle=3.2°, tolerance=1.9°, aimed=False
[Turret] FIRING at Skeleton(Clone)!
```
→ Shows fire rate progress and aim calculations

**Common Debug Messages:**
- "Cannot fire - canFire is false" → Enable `canFire` checkbox on Turret component
- "turretHead is null" → Assign Turret Head transform reference
- "No projectile prefab assigned" → Add projectile to TurretData
- "No targets in scan range" → Increase detection range or verify enemy positions

### Testing Checklist

**Basic Functionality:**
- [ ] Health initialization (check debug: "Turret operational")
- [ ] Target detection (check debug: "Found valid target")
- [ ] Rotation towards targets (visual check)
- [ ] Projectile spawning (check debug: "FIRING")
- [ ] Damage dealing (enemy health decreases)
- [ ] Fire rate timing (check debug cooldown values)

**Advanced Features:**
- [ ] Multiple target types
- [ ] Line of sight checking
- [ ] Projectile behaviors
- [ ] Explosion damage
- [ ] Piercing mechanics
- [ ] Audio/visual effects

**Performance:**
- [ ] Multiple turrets
- [ ] Many targets
- [ ] Rapid firing
- [ ] Projectile cleanup
- [ ] Memory usage

## Common Issues and Solutions

### Turret Won't Shoot/Detect Enemies
**Debug Steps:**
1. Enable "Show Debug Info" on Turret component
2. Enter Play Mode and check Console for specific issues:

**"Turret NOT operational"** →
- Missing Health component: Add Health to turret GameObject
- Missing TurretData: Assign TurretData ScriptableObject
- Missing Power component: Add if using power system

**"Tag mismatch: Enemy has 'X', expected one of: [Y]"** →
- Enemy tag doesn't match TurretData.targetableTags array
- Fix: Either change enemy tag to match, or add tag to targetableTags

**"No targets in scan range"** →
- Enemies too far away: Increase TurretData.range
- Wrong layer: Ensure enemies are on correct layer for Physics.OverlapSphere
- 360° detection verified: Uses spherical scanning, not directional

**"Cannot fire - canFire is false"** →
- Check "Can Fire" checkbox on Turret component
- Verify not disabled by power/resource system

**"turretHead is null"** →
- Assign Turret Head transform in Turret component
- Ensure TurretHead GameObject exists in prefab hierarchy

**"No projectile prefab assigned"** →
- Assign projectile prefab in TurretData ScriptableObject
- Verify prefab has TurretProjectile component

### Turret Won't Rotate
- Check turretHead assignment
- Verify TurretData has rotation speed > 0
- Ensure target detection is working (check debug logs)
- Confirm target is valid (not null, in range, correct tags)

### Projectiles Don't Spawn
- Check projectilePrefab assignment in TurretData
- Verify ProjectileSpawnPoint is assigned
- Check TurretProjectile component on prefab
- Look for "FIRING" in debug logs (if missing, shooting logic not reached)

### No Damage Dealt
- Verify enemies have Health or Actor components  
- Check targetableTags array matches enemy tags
- Ensure projectiles have TurretProjectile component
- Verify projectile collides with enemies (check layers)

### Performance Issues
- Reduce target scan frequency
- Disable line of sight checking
- Implement object pooling for projectiles
- Use LOD system for distant turrets

This system provides a solid foundation for any tower defense game with extensive customization options and easy expansion capabilities!