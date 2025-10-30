# Ballista Turret Configuration

## Ballista Turret Specifications

### Stats Profile
- **Type**: Mid-tier balanced turret
- **Role**: General purpose anti-ground defense
- **Damage**: Medium (35-45)
- **Fire Rate**: Medium (1.2 seconds between shots)
- **Range**: Medium-Long (18 units)
- **Health**: Medium (250 HP)
- **Armor**: Light (3 armor)

### Behavior
- **Target Priority**: Closest enemy
- **Projectile Type**: Linear (fast-moving bolt)
- **Special Features**: None (balanced baseline turret)

### Visual Design
- **Turret Base**: Static stone/wood foundation
- **Turret Head**: Rotating ballista mechanism
- **Projectile**: Fast-moving bolt/spear
- **Effects**: Wooden creak sounds, bolt impact sparks

## TurretData Configuration

Create a new TurretData asset with these settings:

```csharp
// Basic Info
turretName = "Ballista"
description = "A reliable ballista turret that fires fast-moving bolts at ground enemies. Balanced damage, range, and fire rate make it effective against most threats."

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
targetableTags = ["Enemy"]
canTargetAir = false      // Ground-only turret
canTargetGround = true

// Rotation
rotationSpeed = 120f
canRotate360 = true

// Projectile
projectileType = ProjectileBehavior.Linear
projectileSpeed = 25f
projectileLifetime = 2f
piercing = false
maxPierceTargets = 1

// Behavior
behaviorType = TurretBehaviorType.Standard
requiresPower = false
```

## Prefab Setup Requirements

### GameObject Structure
```
BallistaTurret (Root)
├── TurretBase (Static)
│   └── [Base Mesh/Model]
├── TurretHead (Rotating)
│   ├── [Ballista Mesh/Model]
│   └── ProjectileSpawnPoint
│       └── [Empty GameObject at barrel tip]
└── Components:
    ├── Turret (Script)
    ├── Health (Script)
    ├── AudioSource
    ├── Collider (for health/damage)
    └── [Optional: Animator for firing animation]
```

### Component Configuration

**Turret Component:**
- Turret Data: [Assign Ballista TurretData asset]
- Turret Head: [Assign TurretHead transform]
- Projectile Spawn Point: [Assign ProjectileSpawnPoint transform]
- Show Debug Info: true (for testing)
- Show Range In Scene: true (for positioning)

**Health Component:**
- Will be configured automatically by TurretData
- Starting Health: 250 (set by turretData.maxHealth)

**AudioSource Component:**
- 3D Spatial Blend: 1.0
- Volume: 0.7
- Min Distance: 5
- Max Distance: 25

### Projectile Prefab Requirements

Create a ballista bolt projectile with:

```
BallistaBolt (Root)
├── Components:
│   ├── TurretProjectile (Script)
│   ├── MeshRenderer (bolt model)
│   ├── [Optional: TrailRenderer for motion trail]
│   └── [Optional: Light for glow effect]
└── [Bolt mesh/model pointing forward (Z-axis)]
```

**TurretProjectile Component:**
- Show Debug Info: false (true for testing)

## Testing Checklist

### Basic Functionality
- [ ] **Enable Debug Mode**: Check "Show Debug Info" on Turret component
- [ ] Turret spawns with correct health (250) - Watch for "Turret operational" log
- [ ] Turret head rotates smoothly towards enemies
- [ ] Projectiles spawn at correct location and rotation - Look for "FIRING at [enemy]" log
- [ ] Projectiles deal correct damage (40)
- [ ] Fire rate timing is correct (1.2 seconds) - Debug shows cooldown progress
- [ ] Range detection works (18 units) - Check for "Found valid target" logs

### Targeting System
- [ ] Targets ground enemies within range - Debug shows "Found valid target at distance X"
- [ ] Ignores flying enemies (canTargetAir = false)
- [ ] Prioritizes closest enemy
- [ ] Stops targeting dead enemies - "Lost target or target became invalid"
- [ ] Line of sight checking works
- [ ] Tag matching works - No "Tag mismatch" errors in console

### Debug Console Output (When showDebugInfo = true)
**Expected Logs:**
```
[BallistaTurret] Turret operational: Health=True, Power=True, Data=True
[BallistaTurret] Scanning for targets...
[BallistaTurret] Found valid target: Skeleton(Clone) at distance 12.5
[BallistaTurret] Aim check: angle=2.1°, tolerance=1.9°, aimed=True
[BallistaTurret] FIRING at Skeleton(Clone)!
[BallistaTurret] Cooldown: 0.45/1.20
```

**Troubleshooting Logs:**
- "Turret NOT operational - Health: False" → Add Health component
- "Turret NOT operational - Data: False" → Assign TurretData ScriptableObject
- "Tag mismatch: Enemy has 'Enemy', expected one of: [EnemyUnit]" → Fix tag configuration
- "No targets in scan range" → Enemies too far or wrong layer
- "Cannot fire - canFire is false" → Enable canFire checkbox
- "turretHead is null" → Assign TurretHead transform reference
- "No projectile prefab assigned" → Add projectile to TurretData
- "Not aimed at target yet" → Normal - turret is rotating to aim

### Audio/Visual
- [ ] Fire sound plays when shooting
- [ ] Muzzle flash effect spawns
- [ ] Hit effects appear on projectile impact
- [ ] Turret head rotation is smooth
- [ ] Range visualization appears in scene view (when showRangeInScene = true)

### Edge Cases
- [ ] Handles no enemies gracefully
- [ ] Stops firing when out of range
- [ ] Turret destruction works properly
- [ ] Projectile cleanup after lifetime
- [ ] Performance with multiple turrets

## Integration with Existing Systems

### Enemy Compatibility
- Works with existing Enemy.cs component
- Uses Health component for damage dealing
- Falls back to Actor component if needed
- Respects enemy flying state

### Game Integration
- Requires existing Health system
- Compatible with power systems (if implemented)
- Supports upgrade system (if turretData.canUpgrade = true)
- Integrates with resource/building systems

## Balancing Notes

### Strengths
- Reliable damage output
- Good range coverage
- Cost-effective
- No special requirements

### Weaknesses
- Cannot target air units
- No special abilities
- Vulnerable to fast enemies
- Single-target only

### Upgrade Path
Consider creating "Heavy Ballista" as upgrade:
- Higher damage (60)
- Longer range (22)
- Slower fire rate (2.0s)
- Higher cost (150)