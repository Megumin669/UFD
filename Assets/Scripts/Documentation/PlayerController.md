# Player Controller Documentation

The Player Controller provides first-person movement, camera control, and interaction systems for the game.

## 📊 Overview

The Player Controller system includes:
- **FirstPersonController**: Main player control script
- **Movement System**: WASD movement with physics
- **Camera Control**: Mouse look and camera management
- **Weapon Integration**: Weapon switching and attack handling
- **Health Integration**: Health component management

## 🔧 Core Components

### FirstPersonController.cs
Main player controller handling all player interactions:

**Key Systems:**
- First-person movement with physics
- Mouse look camera control
- Weapon system integration
- Health system management
- Input handling for attacks and interactions

### Movement Features
- **WASD Movement**: Standard FPS controls
- **Mouse Look**: Smooth camera rotation
- **Physics-Based**: Uses Rigidbody for movement
- **Ground Detection**: Prevents floating/falling through world

## 🎮 Controls

### Default Input Mapping
- **W/A/S/D**: Move forward/left/backward/right
- **Mouse**: Look around (camera control)
- **Left Mouse Button**: Attack with current weapon
- **Testing Keys** (with HealthTesterExample):
  - **T**: Take damage
  - **H**: Heal
  - **K**: Kill character
  - **R**: Revive character
  - **F**: Full heal

## 🔧 Setup Instructions

### Basic Player Setup
1. **Create Player GameObject**:
   ```
   GameObject → Create Empty → Name: "Player"
   ```

2. **Add Required Components**:
   - `FirstPersonController` script
   - `Health` component
   - `Rigidbody` (configured automatically)
   - `CapsuleCollider` for physics

3. **Setup Camera**:
   - Child GameObject with Camera component
   - Position at eye level (usually Y: 1.6f)
   - Tag as "MainCamera"

4. **Configure FirstPersonController**:
   - Assign player camera reference
   - Set movement speed and sensitivity
   - Configure ground detection layer

### Advanced Setup
```csharp
// Example FirstPersonController configuration
[SerializeField] private float movementSpeed = 6f;
[SerializeField] private float mouseSensitivity = 2f;
[SerializeField] private Camera playerCamera;
[SerializeField] private LayerMask groundLayerMask;
```

## 🔗 System Integration

### Health System Integration
```csharp
public class FirstPersonController : MonoBehaviour
{
    private Health healthComponent;
    
    void Awake()
    {
        healthComponent = GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandlePlayerDeath;
        }
    }
    
    // Public health access methods
    public void TakeDamage(int damage) => healthComponent?.TakeDamage(damage);
    public void Heal(int amount) => healthComponent?.Heal(amount);
    public bool IsAlive => healthComponent?.IsAlive ?? true;
}
```

### Weapon System Integration
```csharp
public class FirstPersonController : MonoBehaviour
{
    private BaseWeapon currentWeapon;
    
    void Update()
    {
        HandleWeaponInput();
    }
    
    void HandleWeaponInput()
    {
        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            currentWeapon.Attack(playerCamera);
        }
    }
    
    public void EquipWeapon(BaseWeapon weapon)
    {
        currentWeapon = weapon;
    }
}
```

## ⚙️ Configuration Options

### Movement Settings
- **Movement Speed**: Player walking speed
- **Mouse Sensitivity**: Camera rotation sensitivity
- **Ground Layer Mask**: What counts as ground for collision

### Camera Settings
- **Camera Reference**: Main player camera
- **Look Sensitivity**: Separate sensitivity for camera look
- **Vertical Look Limits**: Prevent over-rotation

### Physics Settings
- **Rigidbody Mass**: Player physics mass
- **Drag**: Air resistance for movement
- **Angular Drag**: Rotation resistance

## 🧪 Testing & Debugging

### Debug Features
- **Movement Debugging**: Console logs for movement state
- **Health Integration**: Works with HealthTesterExample
- **Weapon Testing**: Direct weapon attack testing

### Testing Tools
Use `HealthTesterExample` component alongside FirstPersonController:

```csharp
// Add to same GameObject as FirstPersonController
GetComponent<HealthTesterExample>();

// Testing controls available during play
// T, H, K, R, F keys for health testing
```

## 🔔 Events & Callbacks

The FirstPersonController integrates with various event systems:

### Health Events
```csharp
// Subscribe to health events
healthComponent.OnHealthChanged += UpdateHealthUI;
healthComponent.OnDeath += HandlePlayerDeath;
```

### Weapon Events
```csharp
// Subscribe to weapon events
currentWeapon.OnAttackStateChange += UpdateAttackUI;
currentWeapon.OnAnimationChange += TriggerPlayerAnimation;
```

## 🎯 Best Practices

### Performance Optimization
- Cache component references in Awake()
- Use FixedUpdate for physics-based movement
- Limit raycast frequency for ground detection

### Code Organization
```csharp
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 6f;
    
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    
    [Header("Health Integration")]
    private Health healthComponent;
    
    [Header("Weapon Integration")]
    private BaseWeapon currentWeapon;
}
```

### Input Handling
- Separate input detection from action execution
- Use events for loose coupling between systems
- Cache input values to avoid multiple Input calls

## 🐛 Known Issues & Limitations

### Current Limitations
- Single camera support (no camera switching)
- Basic physics movement (no advanced movement like wall-running)
- Limited to keyboard/mouse input (no gamepad support documented)

### Common Issues
- **Camera Clipping**: Ensure proper camera positioning
- **Ground Detection**: Configure ground layer mask correctly
- **Weapon Positioning**: Use WeaponSlotSetup for proper weapon placement

## 🔄 Future Enhancements

Potential improvements:
- **Advanced Movement**: Sprinting, crouching, jumping
- **Gamepad Support**: Controller input mapping
- **Multiple Cameras**: Camera switching system
- **Animation Integration**: Full player animation system
- **Sound Integration**: Footstep and movement audio

## 📋 Dependencies

The FirstPersonController depends on:
- **Health Component**: For health management
- **BaseWeapon**: For weapon functionality
- **Unity Input System**: For input handling
- **Physics System**: For movement and collision

---
*Last Updated: October 28, 2025*