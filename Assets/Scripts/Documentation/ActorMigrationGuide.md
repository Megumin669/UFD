# Migration Guide: Actor.cs → Enemy System

## 📋 Overview

The old `Actor.cs` system has been replaced with a new ScriptableObject-based Enemy system that provides much more flexibility and configuration options.

## 🔄 Migration Steps

### **Step 1: Understanding the New System**

**Old System (Actor.cs)**:
- Single component with hardcoded health values
- Limited to basic health/damage functionality
- No AI or behavior configuration
- Health values set in Inspector per-instance

**New System (Enemy.cs + EnemyData.cs)**:
- ScriptableObject-based configuration (EnemyData)
- Advanced AI with state machine behavior
- Configurable abilities, resistances, rewards
- Reusable enemy types across multiple instances

### **Step 2: Convert Existing Enemies**

For each enemy currently using Actor.cs:

1. **Remove Actor Component**:
   ```csharp
   // Remove this component from enemy prefabs
   ```

2. **Add Required Components**:
   ```csharp
   // Add these components to enemy prefabs:
   - Enemy component
   - Health component  
   - NavMeshAgent component (for pathfinding)
   - AudioSource component (optional, for sounds)
   ```

3. **Create EnemyData Asset**:
   ```
   Right-click in Project → Create → EFD → Enemy Data
   Configure enemy properties in Inspector
   ```

4. **Configure Enemy Component**:
   ```csharp
   // Assign EnemyData asset to Enemy component
   // Enable showDebugInfo during testing
   ```

### **Step 3: Example Migration**

**Before (Actor.cs)**:
```csharp
public class Actor : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    
    void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Death();
        }
    }
    
    void Death()
    {
        Destroy(gameObject);
    }
}
```

**After (EnemyData + Enemy + Health)**:

**EnemyData Asset Configuration**:
```
Enemy Name: "Skeleton Warrior"
Max Health: 50
Move Speed: 3
Attack Damage: 10
Behavior Type: Aggressive
Detection Range: 10
Attack Range: 2
Soul Reward: 5
```

**Components on Prefab**:
- `Enemy` component (references EnemyData)
- `Health` component (auto-configured from EnemyData)
- `NavMeshAgent` component
- Colliders and visual components

### **Step 4: Weapon System Compatibility**

The weapon system maintains backward compatibility:

**Current Priority Order**:
1. **Health Component** (preferred - new system)
2. **Actor Component** (fallback - legacy system)

**Example Damage Logic**:
```csharp
// Weapons now prioritize Health over Actor
if (hitTarget.TryGetComponent<Health>(out Health health))
{
    health.TakeDamage(damage); // New system
}
else if (hitTarget.TryGetComponent<Actor>(out Actor actor))
{
    actor.TakeDamage(damage); // Legacy fallback
}
```

### **Step 5: Testing the Migration**

1. **Verify Weapon Damage**:
   - Test melee, ranged, and magic weapons against new enemies
   - Ensure damage is being dealt correctly
   - Check health regeneration if enabled

2. **Test AI Behavior**:
   - Verify enemies can pathfind to targets
   - Check state transitions (Idle → Chase → Attack)
   - Test special abilities if configured

3. **Validate Events**:
   - Ensure OnEnemyDeath events fire correctly
   - Check soul/resource reward distribution
   - Verify spawn/death effects play

## 🗂️ File Structure Changes

**Files to Keep**:
- `Actor.cs` - Keep temporarily for backward compatibility
- All weapon scripts - Already updated to support both systems

**New Files**:
- `EnemyData.cs` - ScriptableObject configuration system
- `Enemy.cs` - Advanced AI component
- `EnemySpawner.cs` - Wave-based spawning system

**Future Cleanup**:
- Once all enemies use the new system, `Actor.cs` can be safely removed
- Update weapon scripts to remove Actor fallback code

## 🎯 Benefits of Migration

### **Immediate Benefits**:
- **Easier Balancing**: Adjust enemy stats in ScriptableObject assets
- **Better AI**: State-based behavior with target prioritization
- **Special Abilities**: Configurable powers with cooldowns
- **Flying Enemies**: Built-in support for aerial units
- **Wave Scaling**: Automatic difficulty progression

### **Long-term Benefits**:
- **Modding Support**: External enemy packs through asset bundles
- **Team Collaboration**: Designers can create enemies without coding
- **Data-Driven Design**: Easy A/B testing of enemy configurations
- **Performance**: More efficient AI and component architecture

## 🚨 Migration Checklist

- [ ] Create EnemyData assets for each enemy type
- [ ] Update enemy prefabs with new components
- [ ] Test weapon damage against new enemies
- [ ] Verify AI pathfinding and behavior
- [ ] Configure spawn points for EnemySpawner
- [ ] Test wave progression and scaling
- [ ] Update any custom scripts that reference Actor
- [ ] Document new enemy creation workflow for team

## 📞 Support

If you encounter issues during migration:
1. Check console for debug messages (enable showDebugInfo on Enemy component)
2. Verify NavMesh is baked for enemy pathfinding
3. Ensure EnemyData assets are properly configured
4. Test with simple enemy configurations first

The new system is designed to be much more powerful while maintaining compatibility with existing weapon systems.