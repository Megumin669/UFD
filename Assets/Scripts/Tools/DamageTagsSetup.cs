using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to automatically set up common damage tags in Unity
/// This prevents crashes when using damage tags that don't exist
/// </summary>
public class DamageTagsSetup : MonoBehaviour
{
    [Header("Damage Tags Setup")]
    [Tooltip("Click the button below to automatically add common damage tags to Unity")]
    [Space(10)]
    
    [Header("Common Tags")]
    public string[] commonDamageTags = { "Player", "Enemy", "NPC", "Destructible" };
    
    void Start()
    {
        // Check which tags exist
        CheckTagsExistence();
    }
    
    void CheckTagsExistence()
    {
        Debug.Log("[DamageTagsSetup] Checking damage tags...");
        
        foreach (string tagName in commonDamageTags)
        {
            bool tagExists = HasTag(tagName);
            Debug.Log($"[DamageTagsSetup] Tag '{tagName}': {(tagExists ? "EXISTS" : "MISSING")}");
        }
    }
    
    // Safe tag checking
    private bool HasTag(string tagName)
    {
        try
        {
            // Try to use the tag - this will throw if it doesn't exist
            gameObject.CompareTag(tagName);
            return true;
        }
        catch (UnityException)
        {
            return false;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DamageTagsSetup))]
public class DamageTagsSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        DamageTagsSetup setup = (DamageTagsSetup)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag Management", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Missing Damage Tags"))
        {
            AddMissingTags(setup.commonDamageTags);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will add common damage tags to Unity's Tag Manager:\n" +
            "• Player (usually exists)\n" +
            "• Enemy\n" +
            "• NPC\n" +
            "• Destructible\n\n" +
            "This prevents crashes when weapons try to check for these tags.",
            MessageType.Info);
        
        if (GUILayout.Button("Open Tags & Layers Settings"))
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            // Note: This opens Project Settings, user needs to navigate to Tags & Layers
        }
    }
    
    void AddMissingTags(string[] tagsToAdd)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        int addedCount = 0;
        
        foreach (string tagName in tagsToAdd)
        {
            if (!TagExists(tagsProp, tagName))
            {
                // Find first empty slot
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty tagProp = tagsProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(tagProp.stringValue))
                    {
                        tagProp.stringValue = tagName;
                        addedCount++;
                        break;
                    }
                }
                
                // If no empty slot found, add new one
                if (!TagExists(tagsProp, tagName))
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                    newTag.stringValue = tagName;
                    addedCount++;
                }
            }
        }
        
        tagManager.ApplyModifiedProperties();
        
        if (addedCount > 0)
        {
            Debug.Log($"[DamageTagsSetup] Added {addedCount} missing damage tags to Unity Tag Manager.");
            EditorUtility.DisplayDialog("Tags Added", $"Successfully added {addedCount} missing damage tags to Unity.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Changes", "All damage tags already exist in Unity.", "OK");
        }
    }
    
    bool TagExists(SerializedProperty tagsProp, string tagName)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty tagProp = tagsProp.GetArrayElementAtIndex(i);
            if (tagProp.stringValue == tagName)
            {
                return true;
            }
        }
        return false;
    }
}
#endif