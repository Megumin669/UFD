# Stamina System Documentation

The Stamina System provides a modular, event-driven energy management solution for character actions like attacking, sprinting, and special abilities.

## 📊 Overview

The Stamina System consists of:
- **Stamina Component**: Main stamina management system
- **StaminaStats Class**: Configurable stamina parameters
- **Event System**: Stamina change notifications and state updates
- **Integration Systems**: Weapon and movement stamina consumption
- **Testing Tools**: Debug and validation helpers

## 🔧 Components

### Stamina.cs
Main stamina component that can be attached to any GameObject.

**Key Features:**
- Configurable maximum stamina and starting values
- Automatic stamina regeneration with customizable delay
- Exhaustion state management
- Action prevention when stamina is insufficient
- Event-driven notifications for UI and game systems

### StaminaStats Class
Comprehensive stamina configuration system:
```csharp
[System.Serializable]
public class StaminaStats
{
    // Basic Settings
    public int maxStamina = 100;
    public int startingStamina = 100;
    
    // Regeneration
    public bool regenerationEnabled = true;
    public float regenerationRate = 10f;          // Points per second
    public float regenerationDelay = 2f;          // Delay after consumption
    
    // Combat Costs
    public int meleeAttackCost = 10;
    public int rangedAttackCost = 5;
    public int magicAttackCost = 15;
    
    // Movement Costs
    public float sprintCostPerSecond = 5f;
    public int jumpCost = 15;
    public int dodgeCost = 15;
}
```

## 🎯 Setup Instructions

### Basic Setup
1. **Add Stamina Component**: Attach `Stamina` script to player GameObject
2. **Configure Stats**: Set stamina parameters in the inspector
3. **Optional Testing**: Add `StaminaTesterExample` for keyboard testing

### Player Integration
```csharp
// FirstPersonController automatically detects Stamina component
private Stamina staminaComponent;

void Awake()
{
    staminaComponent = GetComponent<Stamina>();
}

// Stamina is consumed automatically during:
// - Sprinting (5 stamina/second by default)
// - Jumping (15 stamina per jump by default)
// - Weapon attacks (10 stamina for melee by default)
```

### Weapon Integration
```csharp
// BaseWeapon automatically detects and uses stamina
public virtual bool CanAttack()
{
    if (staminaComponent != null && staminaCost > 0)
    {
        return staminaComponent.HasSufficientStamina(staminaCost);
    }
    return true;
}
```

## 📋 Public Methods

### Core Methods
- `ConsumeStamina(int amount)` - Use stamina for actions
- `ConsumeStaminaOverTime(float rate)` - Continuous stamina drain
- `RestoreStamina(int amount)` - Restore stamina points
- `FullRestore()` - Instantly restore to maximum
- `HasSufficientStamina(int required)` - Check availability

### Status Methods
- `bool CanPerformAction` - Check if character can act
- `bool IsExhausted` - Check exhaustion state
- `bool IsRegenerating` - Check if stamina is regenerating
- `float StaminaPercentage` - Get stamina as 0-1 percentage
- `int CurrentStamina` - Get current stamina points
- `int MaxStamina` - Get maximum stamina points

## 🔔 Events

The Stamina System provides comprehensive event notifications:

```csharp
// Stamina change events
public UnityEvent<int, int> OnStaminaChanged;        // (current, max)
public UnityEvent<int, int> OnStaminaConsumed;       // (consumed, remaining)
public UnityEvent<int> OnStaminaRestored;            // (restored amount)

// State change events
public UnityEvent OnStaminaExhausted;                // When stamina reaches 0
public UnityEvent OnStaminaRecovered;                // When stamina recovers from exhaustion

// Regeneration events
public UnityEvent OnRegenerationStarted;             // When regeneration begins
public UnityEvent OnRegenerationStopped;             // When regeneration stops
```

## ⚙️ Configuration Options

### Basic Stamina Settings
- **Max Stamina**: Maximum stamina points (default: 100)
- **Starting Stamina**: Stamina at initialization (default: 100)

### Regeneration System
- **Regeneration Enabled**: Toggle automatic regeneration (default: true)
- **Regeneration Rate**: Stamina points per second (default: 10)
- **Regeneration Delay**: Delay after consumption before regen starts (default: 2s)

### Action Settings
- **Prevent Action on Low Stamina**: Block actions when insufficient stamina (default: true)

### Combat Stamina Costs
- **Melee Attack Cost**: Stamina for melee attacks (default: 10)
- **Ranged Attack Cost**: Stamina for ranged attacks (default: 5)
- **Magic Attack Cost**: Stamina for magic attacks (default: 15)

### Movement Stamina Costs
- **Sprint Cost Per Second**: Continuous stamina drain while sprinting (default: 5)
- **Jump Cost**: Stamina for jumping (default: 8)
- **Dodge Cost**: Stamina for dodging/rolling (default: 15)

## 🧪 Testing & Debug Tools

### StaminaTesterExample.cs
Provides comprehensive keyboard testing for stamina functionality:

### StaminaDebugConsole.cs
Runtime debug GUI and console logging system:

**Features:**
- Real-time stamina monitoring GUI (F1 to toggle)
- Console debug logging with configurable intervals
- Quick action buttons for testing
- Automatic stamina component detection
- Runtime stamina configuration display

**Testing Controls:**
- **Q** - Consume stamina (configurable amount)
- **E** - Restore stamina (configurable amount)
- **X** - Exhaust character (set stamina to 0)
- **C** - Full restore to maximum
- **V** - Toggle continuous stamina consumption
- **B** - Toggle exhaustion state (debug)
- **I** - Display current stamina status
- **U** - Toggle console debug logging
- **Y** - Force log current status to console
- **J** - Test jump stamina consumption (uses configured jump cost)

**Setup:**
1. Add `StaminaTesterExample` to GameObject with Stamina component
2. Configure test amounts in inspector
3. Enable keyboard testing checkbox
4. Use testing keys during play

**Event Monitoring:**
All stamina events are automatically logged for debugging:
```
[Player] Stamina Consumed: -10 (Remaining: 90)
[Player] Stamina Changed: 90/100 (90%)
[Player] CHARACTER EXHAUSTED!
[Player] Stamina regeneration started
```

### Console Debug System
The stamina system includes comprehensive console debugging:

**Debug Features:**
- **Real-time Monitoring**: Automatic stamina status logging at configurable intervals
- **Action Logging**: Every stamina change is logged with before/after values
- **Color Coding**: Warnings for low stamina and exhaustion states
- **Toggle Controls**: Enable/disable debug logging at runtime

**Console Debug Setup:**
```csharp
// Enable console debug in inspector or via code
stamina.ToggleConsoleDebug();  // Enable/disable console logging
stamina.LogCurrentStatus();    // Force log current status

// Console output examples:
[STAMINA DEBUG] Player
Current: 75/100 (75.0%)
Exhausted: False | Regenerating: True
Can Act: True | Regen Rate: 10/sec

[STAMINA] Player CONSUMED: -10 (85 → 75)
[STAMINA] Player RESTORED: +15 (75 → 90)
```

## 🔗 System Integration

### With Player Controller
```csharp
public class FirstPersonController : MonoBehaviour
{
    private Stamina staminaComponent;
    
    void Awake()
    {
        staminaComponent = GetComponent<Stamina>();
    }
    
    // Sprint stamina consumption
    if (isSprinting && staminaComponent != null)
    {
        var stats = staminaComponent.GetStaminaStats();
        if (!staminaComponent.ConsumeStaminaOverTime(stats.sprintCostPerSecond))
        {
            isSprinting = false; // Stop sprinting if exhausted
        }
    }
}
```

### With Weapon System
```csharp
public class BaseWeapon : MonoBehaviour
{
    protected Stamina staminaComponent;
    protected int staminaCost = 10;
    
    public virtual bool CanAttack()
    {
        if (staminaComponent != null && staminaCost > 0)
        {
            return staminaComponent.HasSufficientStamina(staminaCost);
        }
        return true;
    }
    
    public virtual void Attack(Camera playerCamera)
    {
        if (staminaComponent != null)
        {
            staminaComponent.ConsumeStamina(staminaCost);
        }
        // Perform attack...
    }
}
```

### With UI Systems
```csharp
// Subscribe to stamina events for UI updates
staminaComponent.OnStaminaChanged += UpdateStaminaBar;
staminaComponent.OnStaminaExhausted += ShowExhaustionWarning;

void UpdateStaminaBar(int current, int max)
{
    staminaBar.fillAmount = (float)current / max;
}
```

## 🎮 Usage Examples

### Basic Stamina Management
```csharp
// Get stamina component
Stamina stamina = GetComponent<Stamina>();

// Check if action is possible
if (stamina.HasSufficientStamina(15))
{
    // Perform special attack
    stamina.ConsumeStamina(15);
    SpecialAttack();
}

// Restore stamina with potion
stamina.RestoreStamina(50);
```

### Continuous Actions
```csharp
// For actions that drain stamina over time
void Update()
{
    if (isChannelingSpell)
    {
        if (!stamina.ConsumeStaminaOverTime(spellCostPerSecond))
        {
            StopChanneling(); // Stop if out of stamina
        }
    }
}
```

### Event-Driven UI
```csharp
void Start()
{
    stamina.OnStaminaExhausted += () => {
        exhaustionWarning.SetActive(true);
        DisableSpecialAbilities();
    };
    
    stamina.OnStaminaRecovered += () => {
        exhaustionWarning.SetActive(false);
        EnableSpecialAbilities();
    };
}
```

## 🔧 Upgrade System Integration

The Stamina System is designed for future upgrade integration:

### Upgrade Categories
- **Stamina Capacity**: Increase maximum stamina
- **Regeneration**: Improve regeneration rate and reduce delay
- **Efficiency**: Reduce stamina costs for actions
- **Recovery**: Faster recovery from exhaustion

### Example Upgrade Implementation
```csharp
// Apply stamina upgrades
public void ApplyStaminaUpgrade(StaminaUpgrade upgrade)
{
    var stats = staminaComponent.GetStaminaStats();
    
    stats.maxStamina += upgrade.capacityBonus;
    stats.regenerationRate += upgrade.regenRateBonus;
    stats.meleeAttackCost = Mathf.Max(1, stats.meleeAttackCost - upgrade.efficiencyBonus);
    
    staminaComponent.UpdateStaminaStats(stats);
}
```

## 🐛 Known Issues & Limitations

### Current Limitations
- Regeneration stops permanently when exhausted (design choice)
- No built-in status effects affecting stamina (poison, buffs, etc.)
- Stamina costs are integer-based (no fractional costs)

### Best Practices
- Always check `CanPerformAction` before expensive operations
- Use events for UI updates rather than polling stamina values
- Configure regeneration delay based on game pacing
- Test exhaustion recovery mechanics thoroughly

## 🔄 Future Enhancements

Potential future additions:
- **Status Effects**: Stamina-affecting buffs and debuffs
- **Stamina Types**: Different stamina pools (physical, mental, magical)
- **Dynamic Costs**: Stamina costs that scale with character stats
- **Recovery Items**: Potions and consumables for stamina restoration
- **Mastery System**: Reduced costs through skill progression

## 📊 Performance Considerations

### Optimization Tips
- Cache stamina component references in Awake()
- Use events to minimize direct stamina queries
- Avoid ConsumeStaminaOverTime() in FixedUpdate() unless necessary
- Consider object pooling for stamina-related UI elements

### Memory Usage
- StaminaStats is lightweight and serializable
- Event subscriptions are cleaned up automatically
- No significant memory overhead for stamina system

---
*Last Updated: October 28, 2025*