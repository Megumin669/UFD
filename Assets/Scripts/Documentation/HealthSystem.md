# Health System Documentation

The Health System provides a modular, event-driven health management solution for any character type in the game.

## 📊 Overview

The Health System consists of:
- **Health Component**: Main health management
- **HealthStats Class**: Configurable health parameters
- **Event System**: Health change notifications
- **Testing Tools**: Debug and validation helpers

## 🔧 Components

### Health.cs
Main health component that can be attached to any GameObject.

**Key Features:**
- Configurable max health and starting health
- Automatic health regeneration with delay
- Damage resistance percentage
- Death and revival system
- Event-driven notifications

### HealthStats Class
Serializable class containing all health configuration:
```csharp
[System.Serializable]
public class HealthStats
{
    public int maxHealth = 100;
    public int startingHealth = 100;
    public bool regenerationEnabled = false;
    public float regenerationRate = 1f;
    public float regenerationDelay = 3f;
    public float damageResistance = 0f; // 0-1 (0% to 100%)
}
```

## 🎯 Setup Instructions

### Basic Setup
1. **Add Health Component**: Attach `Health` script to any GameObject
2. **Configure Stats**: Set health parameters in the inspector
3. **Optional Testing**: Add `HealthTesterExample` for keyboard testing

### Advanced Setup
```csharp
// Get health component
Health playerHealth = GetComponent<Health>();

// Subscribe to events
playerHealth.OnHealthChanged += (current, max) => UpdateHealthBar(current, max);
playerHealth.OnDeath += () => HandlePlayerDeath();
playerHealth.OnRevived += () => HandlePlayerRevival();

// Use health methods
playerHealth.TakeDamage(25);
playerHealth.Heal(15);
```

## 📋 Public Methods

### Core Methods
- `TakeDamage(int amount)` - Apply damage with resistance calculation
- `Heal(int amount)` - Restore health points
- `Kill()` - Instantly set health to 0
- `Revive()` - Restore to full health and enable regeneration
- `FullHeal()` - Instantly restore to maximum health

### Status Methods
- `bool IsAlive` - Check if character is alive
- `bool IsDead` - Check if character is dead
- `float HealthPercentage` - Get health as 0-1 percentage
- `int CurrentHealth` - Get current health points
- `int MaxHealth` - Get maximum health points

## 🔔 Events

The Health System provides comprehensive event notifications:

```csharp
// Health change events
public UnityEvent<int, int> OnHealthChanged;        // (current, max)
public UnityEvent<int, int> OnDamageTaken;          // (damage, remaining)
public UnityEvent<int> OnHealthHealed;              // (heal amount)

// State change events  
public UnityEvent OnDeath;
public UnityEvent OnRevived;

// Regeneration events
public UnityEvent OnRegenerationStarted;
public UnityEvent OnRegenerationStopped;
```

## ⚙️ Configuration Options

### Health Stats
- **Max Health**: Maximum health points (default: 100)
- **Starting Health**: Health at game start (default: max health)

### Regeneration System
- **Regeneration Enabled**: Toggle auto-healing (default: false)
- **Regeneration Rate**: Health points per second (default: 1)
- **Regeneration Delay**: Delay after damage before regen starts (default: 3s)

### Damage System
- **Damage Resistance**: Percentage damage reduction 0-1 (default: 0)

## 🧪 Testing Tools

### HealthTesterExample.cs
Provides keyboard testing for health functionality:

**Testing Controls:**
- **T** - Take damage (configurable amount)
- **H** - Heal (configurable amount)  
- **K** - Kill character
- **R** - Revive character
- **F** - Full heal

**Setup:**
1. Add `HealthTesterExample` to GameObject with Health component
2. Configure damage/heal amounts in inspector
3. Enable keyboard testing checkbox
4. Use testing keys during play

## 🔗 Integration

### With Player Controller
The Health System integrates seamlessly with `FirstPersonController`:

```csharp
public class FirstPersonController : MonoBehaviour
{
    private Health healthComponent;
    
    void Awake()
    {
        healthComponent = GetComponent<Health>();
    }
    
    // Public methods for external access
    public void TakeDamage(int damage) => healthComponent?.TakeDamage(damage);
    public void Heal(int amount) => healthComponent?.Heal(amount);
    public bool IsAlive => healthComponent?.IsAlive ?? true;
}
```

### With Actor System (Legacy)
Maintains backward compatibility with existing Actor components:

```csharp
public class Actor : MonoBehaviour
{
    private Health healthComponent;
    
    void Awake()
    {
        // Try to use new Health system first
        healthComponent = GetComponent<Health>();
        
        if (healthComponent != null)
        {
            // Subscribe to health events
            healthComponent.OnHealthChanged += OnHealthChanged;
            healthComponent.OnDeath += OnDeath;
        }
    }
}
```

## 🐛 Known Issues & Limitations

### Current Limitations
- Regeneration stops permanently when health reaches 0
- No built-in maximum damage per hit limits
- No status effect integration (poison, burning, etc.)

### Best Practices
- Always check `IsAlive` before applying damage
- Use events for UI updates rather than polling health values
- Configure regeneration delay based on game pace
- Test damage resistance percentages thoroughly

## 🔄 Future Enhancements

Potential future additions:
- Status effects system (poison, regeneration buffs)
- Temporary health/shields system
- Health scaling based on level/stats
- Advanced damage types and resistances

---
*Last Updated: October 28, 2025*