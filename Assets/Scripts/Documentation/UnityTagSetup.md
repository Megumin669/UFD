# Unity Tag Setup Guide for EFD Enemy System

## 🏷️ Required Tags

The Enemy system requires these tags to be defined in Unity's Tag Manager:

### **Essential Tags:**
- **`Player`** - For player detection and targeting
- **`Defense`** - For turrets, walls, and defensive structures  
- **`Sanctum`** - For the main objective/base to defend

### **Optional Tags:**
- **`Enemy`** - For enemy identification (useful for friendly fire prevention)

## 🔧 How to Add Tags in Unity

### **Step 1: Open Tag Manager**
1. Go to **Edit → Project Settings**
2. Select **Tags and Layers** in the left panel
3. Expand the **Tags** section

### **Step 2: Add Required Tags**
1. Click the **+** button to add a new tag
2. Enter the tag name exactly as shown above
3. Repeat for each required tag

### **Step 3: Apply Tags to GameObjects**

**Player GameObject:**
- Add `Player` tag to your player character

**Defense Structures:**
```
Turrets → Tag: "Defense"
Walls → Tag: "Defense"  
Barriers → Tag: "Defense"
Traps → Tag: "Defense"
```

**Main Objective:**
```
Sanctum/Base → Tag: "Sanctum"
```

## ⚙️ Enemy Configuration Examples

### **Player-Priority Enemy (Guard)**
```
Primary Target: Player
Player Detection Range: 12
Defense Detection Range: 6  
Sanctum Detection Range: 20
```
*Attacks player first if in range, otherwise goes for defenses*

### **Siege Enemy (Tank)**
```
Primary Target: Defenses
Player Detection Range: 8
Defense Detection Range: 15
Sanctum Detection Range: 30
```
*Focuses on destroying turrets and walls first*

### **Objective-Focused Enemy (Runner)**
```
Primary Target: Sanctum
Player Detection Range: 6
Defense Detection Range: 4
Sanctum Detection Range: 50
```
*Tries to reach the main objective, avoiding fights*

## 🛡️ Safe Fallback System

If tags are not defined, the system will:
- ✅ **Not crash** - Safe exception handling prevents game crashes
- ⚠️ **Log warnings** - Console messages indicate missing tags
- 🔄 **Continue functioning** - Enemy will use alternative targeting methods
- 🎯 **Find targets** - Falls back to Health component detection

## 🧪 Testing the System

### **Debug Mode:**
1. Enable `showDebugInfo` on Enemy component
2. Watch console for targeting messages
3. Check for tag-related warnings

### **Visual Debugging:**
1. Select enemy in Scene view
2. Gizmos show detection ranges:
   - **Yellow**: General detection range
   - **Red**: Attack range  
   - **Orange**: Max chase range
   - **Magenta**: Line to current target

## 🚨 Troubleshooting

### **"Tag not defined" Error:**
- Open Tag Manager and add the missing tag
- Apply the tag to appropriate GameObjects
- Restart play mode

### **Enemy Not Finding Targets:**
- Check if GameObjects have correct tags
- Verify detection ranges aren't too small
- Enable debug info to see targeting attempts

### **Enemy Stuck in Idle:**
- Ensure NavMesh is baked in scene
- Check that targets are within detection ranges
- Verify enemy has NavMeshAgent component

## 📋 Quick Setup Checklist

- [ ] Add Player tag to Tag Manager
- [ ] Add Defense tag to Tag Manager  
- [ ] Add Sanctum tag to Tag Manager
- [ ] Tag player GameObject as "Player"
- [ ] Tag turrets/walls as "Defense"
- [ ] Tag main base as "Sanctum"
- [ ] Test enemy targeting with debug info enabled
- [ ] Verify no console errors related to tags

Once tags are properly set up, enemies will intelligently prioritize targets based on their configuration!