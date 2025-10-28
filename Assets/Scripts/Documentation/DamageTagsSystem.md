# Weapon Damage Tags System

The damage tags system allows you to control which objects can be damaged by your weapons. This is particularly useful for scenarios like:

- Staff explosions damaging the player (friendly fire)
- Weapons that only damage enemies
- Weapons that can damage both players and enemies
- Environmental weapons that damage everything

## How It Works

### 1. WeaponData Configuration

In your WeaponData ScriptableObject, you'll find a new section called **"Damage Target Configuration"**:

```
Damageable Tags: ["Player", "Enemy"]
```

- Add the tags of objects you want this weapon to damage
- Leave empty to damage all objects with Actor or Health components
- Examples:
  - `["Enemy"]` - Only damages enemies
  - `["Player", "Enemy"]` - Damages both players and enemies
  - `["Player"]` - Only damages player (useful for self-damage scenarios)
  - `[]` (empty) - Damages any object with health/actor component

### 2. GameObject Setup

Objects that should receive damage need:

1. **Proper Tag**: Set the GameObject's tag to match what's in your weapon's damageable tags
2. **Health Component**: Either `Health` component (new system) or `Actor` component (legacy)

### 3. Weapon Type Coverage

The damage tags system works with all weapon types:

- **Melee Weapons**: Direct attacks check tags before dealing damage
- **Ranged Weapons**: Arrows check tags on impact
- **Magic Weapons**: Projectile explosions check tags for all objects in blast radius

## Example Scenarios

### Staff Self-Damage (Friendly Fire)

To make staff explosions damage the player:

1. **Set Player Tag**: Make sure your player GameObject has tag "Player"
2. **Add Health**: Ensure player has `Health` or `Actor` component
3. **Configure Staff**: In your staff's WeaponData, set Damageable Tags to `["Player", "Enemy"]`

### Enemy-Only Weapons

For weapons that should only damage enemies:

1. **Set Enemy Tags**: Ensure enemy GameObjects have tag "Enemy"
2. **Configure Weapon**: Set Damageable Tags to `["Enemy"]`

### Universal Damage

For weapons that damage everything:

1. **Configure Weapon**: Leave Damageable Tags empty `[]` or include all relevant tags

## Implementation Details

### Backward Compatibility

- If Damageable Tags is empty, weapons behave as before (damage any Actor/Health component)
- Existing weapons will continue to work without modification
- Layer masks still apply in addition to tag checking

### Performance

- Tag checking is efficient and happens before component lookups
- No performance impact on existing weapons that don't use tags
- Explosion damage checks tags first, then components

### Debugging

Use the `DamageTagsExample` script to test your setup:

1. Attach it to any GameObject
2. Use the inspector buttons to quickly set tags and add Health/Actor components
3. Check the console for setup information

#### Testing Controls (When DamageTagsExample is attached):
- **T** - Take damage
- **H** - Heal
- **K** - Kill character  
- **R** - Revive character
- **F** - Full heal

## Code Example

```csharp
// In WeaponData
damageableTags = new string[] { "Player", "Enemy" };

// Weapon checks tags before dealing damage
if (CanDamageTarget(hitObject) && hitObject.TryGetComponent<Health>(out health))
{
    health.TakeDamage(damage);
}
```

## Troubleshooting

**Q: My weapon isn't damaging anything**
- Check that target GameObjects have the correct tags
- Verify targets have Health or Actor components
- Ensure WeaponData has appropriate tags in Damageable Tags array

**Q: Staff explosions aren't damaging the player**
- Confirm player GameObject tag matches weapon's damageable tags
- Check that player has Health component (recommended) or Actor component
- Verify explosion layer mask includes player's layer

**Q: I want to disable tag checking**
- Leave Damageable Tags array empty in WeaponData
- Weapon will damage any object with Health/Actor component (legacy behavior)