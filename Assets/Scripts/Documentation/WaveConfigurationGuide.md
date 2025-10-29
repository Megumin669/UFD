# Wave Configuration System - Implementation Guide

## Overview

The EnemySpawner now supports detailed wave configurations using an array-based system. Each wave can have different enemy types, counts, spawn intervals, and constraints.

## New Configuration Classes

### `WaveEnemyConfig`
Defines a single enemy type within a wave:
```csharp
[System.Serializable]
public class WaveEnemyConfig
{
    public EnemyData enemyData;           // Enemy type to spawn
    public int count = 1;                 // Number to spawn
    public float spawnWeight = 1f;        // Weight for random selection
    public int minWaveNumber = 1;         // Minimum wave for this enemy
    public int maxWaveNumber = 0;         // Maximum wave (0 = no limit)
}
```

### `WaveConfiguration`
Defines a complete wave setup:
```csharp
[System.Serializable]
public class WaveConfiguration
{
    public int waveNumber = 1;            // Wave identifier
    public string waveName = "";          // Custom wave name
    public WaveEnemyConfig[] enemies;     // Enemy types in this wave
    public float spawnInterval = 2f;      // Time between spawns
    public bool isBossWave = false;       // Is this a boss wave?
    public string description = "";       // Wave description
}
```

## Updated EnemySpawner Configuration

### New Inspector Fields

**Wave Configuration Section:**
- `waveConfigurations[]` - Array of specific wave setups
- `fallbackEnemies[]` - Enemy types for procedural waves beyond configured ones

**Procedural Wave Settings:**
- `baseEnemiesPerWave` - Base count for auto-generated waves  
- `enemyScalingPerWave` - Count multiplier per wave for procedural waves

### Removed Fields
- `availableEnemies[]` - Replaced by wave configurations and fallback enemies

## How It Works

### Wave Selection Logic
1. **Configured Waves**: If `waveConfigurations` contains a setup for the current wave number, use it
2. **Boss Waves**: If wave number matches boss interval and no config exists, spawn boss
3. **Procedural Waves**: Use `fallbackEnemies` with automatic scaling for unconfigured waves

### Enemy Selection Within Waves
- **Weight-Based Selection**: Enemies with higher `spawnWeight` are more likely to be chosen
- **Wave Constraints**: Enemies only spawn if current wave is between their `minWaveNumber` and `maxWaveNumber`
- **Count Enforcement**: Each enemy type spawns exactly `count` times per wave

## Configuration Examples

### Example 1: Early Game Wave (Wave 1)
```csharp
WaveConfiguration wave1 = new WaveConfiguration()
{
    waveNumber = 1,
    waveName = "First Contact",
    spawnInterval = 3f,
    enemies = new WaveEnemyConfig[]
    {
        new WaveEnemyConfig()
        {
            enemyData = goblinWarriorData,
            count = 3,
            spawnWeight = 1f,
            minWaveNumber = 1,
            maxWaveNumber = 5
        },
        new WaveEnemyConfig()
        {
            enemyData = goblinArcherData,
            count = 2,
            spawnWeight = 0.7f,
            minWaveNumber = 1,
            maxWaveNumber = 3
        }
    }
};
```

### Example 2: Mixed Wave (Wave 5)
```csharp
WaveConfiguration wave5 = new WaveConfiguration()
{
    waveNumber = 5,
    waveName = "The Horde Approaches",
    spawnInterval = 2f,
    enemies = new WaveEnemyConfig[]
    {
        new WaveEnemyConfig()
        {
            enemyData = goblinWarriorData,
            count = 5,
            spawnWeight = 1f
        },
        new WaveEnemyConfig()
        {
            enemyData = orcBruteData,
            count = 2,
            spawnWeight = 0.8f,
            minWaveNumber = 4
        },
        new WaveEnemyConfig()
        {
            enemyData = flyingScoutData,
            count = 3,
            spawnWeight = 0.6f,
            minWaveNumber = 3
        }
    }
};
```

### Example 3: Boss Wave (Wave 10)
```csharp
WaveConfiguration wave10 = new WaveConfiguration()
{
    waveNumber = 10,
    waveName = "Goblin King's Assault",
    isBossWave = true,
    spawnInterval = 1f,
    enemies = new WaveEnemyConfig[]
    {
        new WaveEnemyConfig()
        {
            enemyData = goblinKingData,
            count = 1,
            spawnWeight = 1f
        },
        new WaveEnemyConfig()
        {
            enemyData = goblinGuardData,
            count = 4,
            spawnWeight = 1f
        }
    }
};
```

## Setup Instructions

### 1. Configure Specific Waves
1. In EnemySpawner inspector, expand "Wave Configuration"
2. Set size of `waveConfigurations` array
3. For each wave:
   - Set `waveNumber` (must be unique)
   - Set `waveName` (optional, for identification)
   - Configure `enemies` array with desired enemy types and counts
   - Set `spawnInterval` for pacing
   - Mark `isBossWave` if appropriate

### 2. Configure Fallback System
1. Assign `fallbackEnemies[]` with enemy types for procedural waves
2. Set `baseEnemiesPerWave` for auto-generated wave size
3. Set `enemyScalingPerWave` for difficulty progression

### 3. Maintain Boss System
- Keep `bossEnemies[]` for automatic boss waves
- Set `bossWaveInterval` for boss frequency
- Boss waves override procedural generation but not configured waves

## Wave Constraints System

### Min/Max Wave Numbers
- `minWaveNumber`: Enemy won't appear before this wave
- `maxWaveNumber`: Enemy won't appear after this wave (0 = no limit)
- Useful for phasing out early enemies and introducing advanced ones

### Example Progression:
```csharp
// Goblin Warrior: Waves 1-10
minWaveNumber = 1, maxWaveNumber = 10

// Orc Brute: Waves 5+
minWaveNumber = 5, maxWaveNumber = 0

// Dragon: Waves 15+ only
minWaveNumber = 15, maxWaveNumber = 0
```

## Weight-Based Selection

When multiple enemy types are valid for a wave, selection uses weighted randomization:

```csharp
// High priority enemy (appears often)
spawnWeight = 2f

// Normal priority enemy  
spawnWeight = 1f

// Low priority enemy (appears rarely)
spawnWeight = 0.3f
```

## Migration from Old System

### Old Configuration (Removed):
```csharp
public EnemyData[] availableEnemies; // REMOVED
```

### New Configuration:
```csharp
// For specific wave setups
public WaveConfiguration[] waveConfigurations;

// For procedural waves beyond configured ones  
public EnemyData[] fallbackEnemies;
```

### Migration Steps:
1. Move enemy types from old `availableEnemies` to `fallbackEnemies`
2. Create specific wave configurations for key waves (bosses, difficulty spikes)
3. Set up enemy progression using min/max wave constraints
4. Test wave flow and adjust spawn intervals and counts

## Debug and Validation

### Console Messages:
- `"Using configured wave: [WaveName]"` - Specific wave configuration found
- `"Using procedural wave generation"` - Falling back to automatic generation
- `"No valid enemies found for wave X"` - Configuration error, check constraints

### Validation Features:
- Automatic validation of all enemy data references
- Warnings for missing spawn points
- Error reporting for null enemy data
- Fallback mechanisms prevent crashes

## Performance Considerations

- Wave configurations are looked up once per wave (minimal overhead)
- Weight calculations only occur during enemy selection
- No performance impact on gameplay after spawning begins
- Memory usage scales with number of configured waves (typically minimal)

## Best Practices

1. **Start Simple**: Configure key waves (1, 5, 10, boss waves) first
2. **Use Constraints**: Phase out early enemies and introduce advanced ones progressively  
3. **Balance Weights**: Higher weights make enemies more common, lower weights make them special
4. **Test Progression**: Play through multiple waves to ensure difficulty curve feels right
5. **Maintain Fallbacks**: Always have `fallbackEnemies` configured for unconfigured waves
6. **Boss Integration**: Use both configured boss waves and automatic boss intervals for variety