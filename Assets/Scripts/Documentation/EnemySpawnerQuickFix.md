# EnemySpawner Quick Setup Guide

## CRITICAL: Preventing "Null EnemyData found in wave X!" Crash

This error occurs when the EnemySpawner cannot find any valid enemy configurations. Here's how to fix it:

## Minimum Configuration Required

**You MUST configure at least ONE of the following:**

### Option 1: Fallback Enemies (Recommended for beginners)
1. In EnemySpawner inspector, find "Fallback Enemies" array
2. Set size to at least 1
3. Assign any EnemyData asset to element 0
4. This provides enemies for any unconfigured waves

### Option 2: Wave Configurations 
1. In EnemySpawner inspector, find "Wave Configurations" array
2. Set size to at least 1
3. Configure wave 1:
   - Set Wave Number = 1
   - Set enemies array size to 1
   - Assign an EnemyData and set count = 1

### Option 3: Boss Enemies (Emergency fallback only)
1. In EnemySpawner inspector, find "Boss Enemies" array
2. Set size to at least 1
3. Assign any EnemyData asset
4. NOTE: This will use boss enemies for regular waves (not ideal)

## Quick Fix for Current Crash

**Immediate Solution:**
1. Open EnemySpawner in inspector
2. Expand "Procedural Wave Settings"
3. Find "Fallback Enemies" array
4. Set Size = 1
5. Drag any EnemyData ScriptableObject to Element 0
6. Click Play - crash should be resolved

## Recommended Setup

```
EnemySpawner Configuration:
├── Wave Configuration
│   ├── Use Wave System: ✓ Enabled
│   ├── Time Between Waves: 60
│   └── Wave Configurations: [Configure specific waves here]
│
├── Procedural Wave Settings  
│   ├── Fallback Enemies: [At least 1 EnemyData] ← CRITICAL
│   ├── Base Enemies Per Wave: 5
│   └── Enemy Scaling Per Wave: 1.2
│
└── Special Events
    ├── Boss Enemies: [Optional boss EnemyData]
    └── Boss Wave Interval: 5
```

## Error Messages and Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| "Null EnemyData found in wave X!" | No valid enemy configuration | Add fallback enemies or wave configs |
| "No enemy configuration found for wave X!" | Missing enemies for that wave | Add fallback enemies |
| "No regular enemies configured" | Only boss enemies assigned | Add fallback enemies for regular waves |

## Validation Messages

The EnemySpawner will now show helpful warnings:
- ✅ "Using configured wave: [Name]" - Specific wave config found
- ⚠️ "Using procedural wave generation" - Using fallback enemies  
- ❌ "CRITICAL: No enemy types assigned!" - Nothing configured (will crash)

## Example Minimal Setup

**Create a basic EnemyData:**
1. Right-click in Project → Create → Enemy Data
2. Name it "BasicEnemy"
3. Set basic values (health: 100, damage: 10, etc.)
4. Assign a prefab with Enemy component

**Configure EnemySpawner:**
1. Set Fallback Enemies size = 1
2. Assign your BasicEnemy to element 0
3. Done! All waves will now spawn BasicEnemy

## Testing Configuration

Use the validation messages in Console:
- No errors = Configuration valid
- Warnings = Will work but could be improved  
- Errors = Will crash, requires immediate fix

The spawner will now provide detailed feedback about what's missing and suggest specific fixes.