using UnityEngine;

/// <summary>
/// Example script showing how to setup damage tags for weapons
/// This demonstrates how staff explosions can damage the player or other objects
/// </summary>
public class DamageTagsExample : MonoBehaviour
{
    [Header("Damage Tags Demonstration")]
    [Tooltip("This object will be damaged by weapons that target these tags")]
    [Space(10)]
    
    [Header("Instructions")]
    [TextArea(6, 10)]
    public string instructions = 
        "1. Set this GameObject's tag to 'Player' or 'Enemy'\n" +
        "2. Configure your weapon's Damageable Tags in WeaponData\n" +
        "3. Staff explosions will now damage objects with matching tags\n" +
        "4. Example: Add 'Player' to staff tags for self-damage from explosions\n\n" +
        "Testing Controls:\n" +
        "T - Take damage | H - Heal | K - Kill | R - Revive | F - Full heal";
    
    [Header("Current Configuration")]
    public string currentTag = "";
    
    void Start()
    {
        // Show current tag for reference
        currentTag = gameObject.tag;
        
        // Ensure we have a Health or Actor component for damage
        if (!TryGetComponent<Health>(out _) && !TryGetComponent<Actor>(out _))
        {
            Debug.LogWarning($"[{gameObject.name}] DamageTagsExample: No Health or Actor component found! " +
                "Add a Health or Actor component to receive damage.", this);
        }
        
        // Display current setup
        Debug.Log($"[{gameObject.name}] Damage Tags Example initialized:\n" +
                 $"GameObject Tag: '{gameObject.tag}'\n" +
                 $"Has Health Component: {TryGetComponent<Health>(out _)}\n" +
                 $"Has Actor Component: {TryGetComponent<Actor>(out _)}");
    }
    
    void OnValidate()
    {
        // Update current tag display in inspector
        currentTag = gameObject.tag;
    }
}

/// <summary>
/// Editor script to help set up damage tags configuration
/// </summary>
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DamageTagsExample))]
public class DamageTagsExampleEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        DamageTagsExample example = (DamageTagsExample)target;
        
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Quick Setup", UnityEditor.EditorStyles.boldLabel);
        
        UnityEditor.EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Tag: Player"))
        {
            example.gameObject.tag = "Player";
            example.currentTag = "Player";
        }
        if (GUILayout.Button("Set Tag: Enemy"))
        {
            example.gameObject.tag = "Enemy";
            example.currentTag = "Enemy";
        }
        UnityEditor.EditorGUILayout.EndHorizontal();
        
        UnityEditor.EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Health Component"))
        {
            if (!example.TryGetComponent<Health>(out _))
            {
                example.gameObject.AddComponent<Health>();
            }
        }
        if (GUILayout.Button("Add Actor Component"))
        {
            if (!example.TryGetComponent<Actor>(out _))
            {
                example.gameObject.AddComponent<Actor>();
            }
        }
        UnityEditor.EditorGUILayout.EndHorizontal();
        
        // Show weapon configuration hint
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.HelpBox(
            "Don't forget to configure your WeaponData!\n" +
            "• Open your weapon's ScriptableObject\n" +
            "• Set 'Damageable Tags' to include tags you want to damage\n" +
            "• Example: Add 'Player' to staff tags for self-damage from explosions", 
            UnityEditor.MessageType.Info);
    }
}
#endif