using UnityEngine;

/// <summary>
/// Simple console commands for debugging stamina system at runtime
/// Add this to any GameObject to enable stamina debugging commands
/// </summary>
public class StaminaDebugConsole : MonoBehaviour
{
    [Header("Debug Console Settings")]
    [SerializeField] private bool enableConsoleCommands = true;
    [SerializeField] private KeyCode debugToggleKey = KeyCode.F1;
    
    [Header("Target Stamina Component")]
    [SerializeField] private Stamina targetStamina;
    [SerializeField] private bool autoFindStamina = true;
    
    private bool showDebugGUI = false;
    private Vector2 scrollPosition;
    
    void Start()
    {
        if (autoFindStamina && targetStamina == null)
        {
            targetStamina = FindFirstObjectByType<Stamina>();
            if (targetStamina == null)
            {
                Debug.LogWarning("[StaminaDebugConsole] No Stamina component found in scene!");
            }
        }
        
        if (enableConsoleCommands)
        {
            Debug.Log("[StaminaDebugConsole] Debug console initialized. Press F1 to toggle debug GUI.");
        }
    }
    
    void Update()
    {
        if (enableConsoleCommands && Input.GetKeyDown(debugToggleKey))
        {
            showDebugGUI = !showDebugGUI;
            Debug.Log($"[StaminaDebugConsole] Debug GUI {(showDebugGUI ? "SHOWN" : "HIDDEN")}");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugGUI || targetStamina == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Stamina Debug Console", GUI.skin.label);
        GUILayout.Space(10);
        
        // Current Status
        GUILayout.Label("Current Status:", GUI.skin.label);
        GUILayout.Label($"Stamina: {targetStamina.CurrentStamina}/{targetStamina.MaxStamina}");
        GUILayout.Label($"Percentage: {targetStamina.StaminaPercentage:P1}");
        GUILayout.Label($"Exhausted: {targetStamina.IsExhausted}");
        GUILayout.Label($"Regenerating: {targetStamina.IsRegenerating}");
        GUILayout.Label($"Can Act: {targetStamina.CanPerformAction}");
        
        GUILayout.Space(10);
        
        // Quick Actions
        GUILayout.Label("Quick Actions:", GUI.skin.label);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Consume 10"))
        {
            targetStamina.ConsumeStamina(10);
            Debug.Log("[Debug] Consumed 10 stamina");
        }
        if (GUILayout.Button("Restore 10"))
        {
            targetStamina.RestoreStamina(10);
            Debug.Log("[Debug] Restored 10 stamina");
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Consume 25"))
        {
            targetStamina.ConsumeStamina(25);
            Debug.Log("[Debug] Consumed 25 stamina");
        }
        if (GUILayout.Button("Restore 25"))
        {
            targetStamina.RestoreStamina(25);
            Debug.Log("[Debug] Restored 25 stamina");
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Exhaust"))
        {
            targetStamina.DEBUG_SetStamina(0);
            Debug.Log("[Debug] Character exhausted");
        }
        if (GUILayout.Button("Full Restore"))
        {
            targetStamina.FullRestore();
            Debug.Log("[Debug] Full stamina restore");
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Console Logging
        GUILayout.Label("Console Logging:", GUI.skin.label);
        if (GUILayout.Button("Toggle Console Debug"))
        {
            targetStamina.ToggleConsoleDebug();
        }
        if (GUILayout.Button("Log Current Status"))
        {
            targetStamina.LogCurrentStatus();
        }
        
        GUILayout.Space(10);
        
        // Stamina Stats Info
        GUILayout.Label("Configuration:", GUI.skin.label);
        var stats = targetStamina.GetStaminaStats();
        GUILayout.Label($"Max: {stats.maxStamina}");
        GUILayout.Label($"Regen Rate: {stats.regenerationRate}/sec");
        GUILayout.Label($"Regen Delay: {stats.regenerationDelay}s");
        GUILayout.Label($"Melee Cost: {stats.meleeAttackCost}");
        GUILayout.Label($"Sprint Cost: {stats.sprintCostPerSecond}/sec");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    // Public methods for external use
    public void SetTargetStamina(Stamina stamina)
    {
        targetStamina = stamina;
        Debug.Log($"[StaminaDebugConsole] Target set to: {(stamina != null ? stamina.gameObject.name : "null")}");
    }
    
    public void EnableConsoleDebug()
    {
        if (targetStamina != null)
        {
            targetStamina.ToggleConsoleDebug();
        }
    }
    
    public void LogStaminaStatus()
    {
        if (targetStamina != null)
        {
            targetStamina.LogCurrentStatus();
        }
    }
}