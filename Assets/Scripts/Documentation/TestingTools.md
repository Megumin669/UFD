# Testing Tools Documentation

Comprehensive testing and debugging tools for the EFD game system.

## 📊 Overview

The Testing Tools provide:
- **Health Testing**: Interactive health system validation
- **Damage Tags Testing**: Weapon targeting system testing  
- **Setup Tools**: Automated configuration helpers
- **Debug Visualizations**: Editor gizmos and visual aids

## 🧪 Health Testing Tools

### HealthTesterExample.cs
Interactive testing component for the Health System.

**Features:**
- Keyboard-based health manipulation
- Real-time health status display
- Event system testing
- Configurable damage/heal amounts

**Setup:**
1. Add `HealthTesterExample` to GameObject with Health component
2. Configure test values in inspector
3. Enable keyboard testing checkbox
4. Use testing keys during play

**Testing Controls:**
- **T** - Take damage (configurable amount)
- **H** - Heal (configurable amount)
- **K** - Kill character instantly
- **R** - Revive character to full health
- **F** - Full heal to maximum

**Inspector Configuration:**
```csharp
[Header("Test Values")]
[Range(1, 100)] public int damageAmount = 10;
[Range(1, 100)] public int healAmount = 15;

[Header("Health Testing")]
[SerializeField] private bool enableKeyboardTesting = true;
```

**Event Monitoring:**
The tool automatically subscribes to all health events and logs them:
```csharp
// Example logged output
[Player] Health Changed: 85/100 (85%)
[Player] Damage Taken: -15 (Remaining: 85)
[Player] CHARACTER DIED!
[Player] CHARACTER REVIVED!
```

## 🎯 Damage Tags Testing

### DamageTagsExample.cs
Testing and setup helper for the Damage Tags System.

**Features:**
- Tag configuration assistance
- Component setup automation
- Visual feedback for configuration
- Integration testing support

**Inspector Tools:**
- **Set Tag: Player** - Quick player tag setup
- **Set Tag: Enemy** - Quick enemy tag setup  
- **Add Health Component** - Automatic Health component addition
- **Add Actor Component** - Legacy Actor component addition

**Setup Validation:**
Automatically checks and reports:
- Current GameObject tag
- Presence of Health component
- Presence of Actor component
- Configuration status

### DamageTagsSetup.cs
Automated Unity tag setup tool.

**Features:**
- Automatic tag creation in Unity Tag Manager
- Missing tag detection
- Batch tag setup for common damage tags
- Project configuration assistance

**Usage:**
1. Add `DamageTagsSetup` component to any GameObject
2. Click "Add Missing Damage Tags" in inspector
3. Tool automatically adds: Player, Enemy, NPC, Destructible tags

## 🔧 Setup & Configuration Tools

### WeaponSlotSetup (Weapon System)
Visual tool for weapon positioning and configuration.

**Features:**
- Real-time position adjustment
- Visual gizmos for weapon placement
- Save/load to WeaponData
- Spawn point visualization

**Controls:**
- Drag handles to adjust position
- Use inspector sliders for precise control
- "Save to WeaponData" button to persist changes
- Gizmos show weapon orientation and spawn points

### Editor Visualizations
Debug gizmos available throughout the system:

**Weapon System:**
- Attack range spheres and rays
- Projectile spawn points and trajectories
- Explosion radius previews
- Weapon orientation indicators

**Health System:**
- Health bar previews (when UI integrated)
- Damage resistance visualizations
- Regeneration status indicators

## 🎮 Runtime Testing

### Console Logging
Comprehensive logging system for debugging:

**Health System Logs:**
```
[Player] Health Tester initialized - Use keys: T, H, K, R, F
[Player] Health Changed: 75/100 (75%)
[Player] Damage Taken: -25 (Remaining: 75)
[Player] Regeneration Started
```

**Damage Tags Logs:**
```
[DamageTagsSetup] Checking damage tags...
[DamageTagsSetup] Tag 'Player': EXISTS
[DamageTagsSetup] Tag 'Enemy': MISSING
[Enemy] Damage Tags Example initialized
```

**Weapon System Logs:**
```
[Staff] Magic weapon initialized with projectile lifetime: 3.0s
[Bow] Arrow fired with damage: 15, speed: 30
[Sword] Combo attack triggered: Attack 2
```

## 🛠️ Development Tools

### Error Prevention
Tools help prevent common setup errors:

**Tag Validation:**
- Safe tag checking prevents crashes
- Automatic fallback for missing tags
- Clear error messages for configuration issues

**Component Validation:**
- Automatic dependency checking
- Missing component warnings
- Setup assistance through inspector buttons

### Performance Testing
Built-in performance considerations:

**Efficient Testing:**
- Minimal performance impact when testing disabled
- Event-driven updates reduce polling overhead
- Cached references prevent repeated GetComponent calls

## 📋 Testing Workflows

### New Feature Testing
1. **Implement Feature**: Create new functionality
2. **Add Testing Script**: Create dedicated testing component
3. **Document Controls**: Update documentation with test keys/methods
4. **Validate Integration**: Test with existing systems

### System Integration Testing
1. **Health + Weapons**: Test damage application
2. **Damage Tags + All Weapons**: Test targeting system
3. **Player + All Systems**: Test complete integration
4. **Performance Testing**: Monitor frame rates during testing

### Bug Reproduction
1. **Isolate Issue**: Use individual testing components
2. **Log Everything**: Enable verbose logging
3. **Step-by-Step**: Use keyboard controls for precise testing
4. **Document Results**: Record exact reproduction steps

## 🔔 Testing Events

Testing tools integrate with the event system:

```csharp
// Health testing events
healthComponent.OnHealthChanged += LogHealthChange;
healthComponent.OnDamageTaken += LogDamage;
healthComponent.OnDeath += LogDeath;

// Weapon testing events  
weapon.OnAttackStateChange += LogAttackState;
weapon.OnAnimationChange += LogAnimation;
```

## ⚙️ Configuration Best Practices

### Testing Setup
- **Separate Testing Scene**: Create dedicated test scene
- **Multiple Test Objects**: Test different configurations simultaneously
- **Clear Naming**: Use descriptive names for test objects
- **Documentation**: Comment test setups for future reference

### Debugging Workflow
1. **Start Simple**: Test individual components first
2. **Add Complexity**: Gradually integrate systems
3. **Log Everything**: Enable comprehensive logging
4. **Visual Feedback**: Use gizmos and UI elements
5. **Document Issues**: Record bugs and solutions

## 🐛 Common Testing Scenarios

### Health System Testing
```csharp
// Test damage resistance
health.TakeDamage(100); // Should apply resistance

// Test regeneration
health.TakeDamage(50);  // Wait for regen delay
// Observe automatic healing

// Test death/revival cycle
health.Kill();          // Should trigger death events
health.Revive();        // Should restore full health
```

### Weapon System Testing
```csharp
// Test different weapon types
SwitchWeapon(meleeWeapon);   // Test melee attacks
SwitchWeapon(rangedWeapon);  // Test arrow shooting
SwitchWeapon(magicWeapon);   // Test explosion damage

// Test upgrade effects
ApplyUpgrades(weapon);       // Test damage/speed changes
```

### Damage Tags Testing
```csharp
// Test tag targeting
SetObjectTag("Player");      // Should be damaged by weapons
SetObjectTag("Enemy");       // Should be damaged by weapons  
SetObjectTag("Untagged");    // Should not be damaged (if tags specified)
```

## 🔄 Future Testing Enhancements

Planned testing improvements:
- **Automated Test Suite**: Unit testing framework integration
- **Performance Profiling**: Built-in performance monitoring
- **Visual Test Results**: UI panels for test status
- **Batch Testing**: Automated testing across multiple scenarios

---
*Last Updated: October 28, 2025*