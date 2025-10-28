using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeaponSlotSetup : MonoBehaviour
{
    [Header("WeaponSlot Setup")]
    [SerializeField] private bool autoFindWeaponSlot = true;
    [SerializeField] private GameObject weaponSlotObject;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Position Management")]
    [SerializeField] private bool showPositionGizmos = true;
    
    void Start()
    {
        if (autoFindWeaponSlot)
        {
            FindOrCreateWeaponSlot();
        }
    }
    
    public void FindOrCreateWeaponSlot()
    {
        weaponSlotObject = FindWeaponSlotInHierarchy(transform);
        
        if (weaponSlotObject == null)
        {
            CreateWeaponSlot();
        }
        else if (showDebugInfo)
        {
            Debug.Log($"WeaponSlot found: {weaponSlotObject.name}");
        }
    }
    
    GameObject FindWeaponSlotInHierarchy(Transform parent)
    {
        // Check if this object has the WeaponSlot tag
        if (parent.CompareTag("WeaponSlot"))
        {
            return parent.gameObject;
        }
        
        // Search through all children recursively
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject found = FindWeaponSlotInHierarchy(parent.GetChild(i));
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    public void CreateWeaponSlot()
    {
        GameObject newWeaponSlot = new GameObject("WeaponSlot");
        newWeaponSlot.transform.SetParent(transform);
        newWeaponSlot.transform.localPosition = Vector3.zero;
        newWeaponSlot.transform.localRotation = Quaternion.identity;
        newWeaponSlot.tag = "WeaponSlot";
        
        weaponSlotObject = newWeaponSlot;
        
        if (showDebugInfo)
        {
            Debug.Log($"Created WeaponSlot: {newWeaponSlot.name}");
        }
    }
    
    public GameObject GetWeaponSlot()
    {
        if (weaponSlotObject == null)
        {
            FindOrCreateWeaponSlot();
        }
        return weaponSlotObject;
    }
    
    // Method to move existing weapons to WeaponSlot
    [ContextMenu("Move Existing Weapons to WeaponSlot")]
    public void MoveExistingWeaponsToSlot()
    {
        if (weaponSlotObject == null)
        {
            FindOrCreateWeaponSlot();
        }
        
        BaseWeapon[] weapons = GetComponentsInChildren<BaseWeapon>();
        int movedCount = 0;
        
        foreach (BaseWeapon weapon in weapons)
        {
            // Only move weapons that are not already in the WeaponSlot
            if (!IsChildOfWeaponSlot(weapon.transform))
            {
                weapon.transform.SetParent(weaponSlotObject.transform);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
                movedCount++;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Moved weapon {weapon.WeaponName} to WeaponSlot");
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Moved {movedCount} weapons to WeaponSlot");
        }
    }
    
    bool IsChildOfWeaponSlot(Transform weaponTransform)
    {
        Transform current = weaponTransform.parent;
        while (current != null)
        {
            if (current.CompareTag("WeaponSlot"))
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }
    
    // Save current weapon positions to their WeaponData
    [ContextMenu("Save Weapon Positions to WeaponData")]
    public void SaveWeaponPositionsToData()
    {
        if (weaponSlotObject == null)
        {
            FindOrCreateWeaponSlot();
        }
        
        BaseWeapon[] weapons = weaponSlotObject.GetComponentsInChildren<BaseWeapon>();
        int savedCount = 0;
        
        foreach (BaseWeapon weapon in weapons)
        {
            WeaponData weaponData = GetWeaponData(weapon);
            if (weaponData != null)
            {
                weaponData.weaponSlotPosition = weapon.transform.localPosition;
                weaponData.weaponSlotRotation = weapon.transform.localRotation.eulerAngles;
                weaponData.weaponSlotScale = weapon.transform.localScale;
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(weaponData);
                #endif
                
                savedCount++;
                if (showDebugInfo)
                {
                    Debug.Log($"Saved position for {weapon.WeaponName}: Pos{weaponData.weaponSlotPosition}, Rot{weaponData.weaponSlotRotation}");
                }
            }
        }
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
        
        if (showDebugInfo)
        {
            Debug.Log($"Saved positions for {savedCount} weapons to WeaponData assets");
        }
    }
    
    // Load weapon positions from their WeaponData
    [ContextMenu("Load Weapon Positions from WeaponData")]
    public void LoadWeaponPositionsFromData()
    {
        if (weaponSlotObject == null)
        {
            FindOrCreateWeaponSlot();
        }
        
        BaseWeapon[] weapons = weaponSlotObject.GetComponentsInChildren<BaseWeapon>();
        int loadedCount = 0;
        
        foreach (BaseWeapon weapon in weapons)
        {
            WeaponData weaponData = GetWeaponData(weapon);
            if (weaponData != null)
            {
                weapon.transform.localPosition = weaponData.weaponSlotPosition;
                weapon.transform.localRotation = Quaternion.Euler(weaponData.weaponSlotRotation);
                weapon.transform.localScale = weaponData.weaponSlotScale;
                
                loadedCount++;
                if (showDebugInfo)
                {
                    Debug.Log($"Loaded position for {weapon.WeaponName}: Pos{weaponData.weaponSlotPosition}, Rot{weaponData.weaponSlotRotation}");
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Loaded positions for {loadedCount} weapons from WeaponData assets");
        }
    }
    
    // Helper method to get WeaponData from different weapon types
    WeaponData GetWeaponData(BaseWeapon weapon)
    {
        // Use reflection to get weaponData field from any weapon type
        var weaponDataField = weapon.GetType().GetField("weaponData");
        if (weaponDataField != null && weaponDataField.FieldType == typeof(WeaponData))
        {
            return weaponDataField.GetValue(weapon) as WeaponData;
        }
        
        return null;
    }
    
    // Quick preset positioning methods
    [ContextMenu("Apply Right-Hand Grip Preset")]
    public void ApplyRightHandGripPreset()
    {
        ApplyPositionPreset("Right-Hand Grip", new Vector3(0.5f, -0.2f, 0.3f), new Vector3(0f, 90f, 0f));
    }
    
    [ContextMenu("Apply Left-Hand Grip Preset")]
    public void ApplyLeftHandGripPreset()
    {
        ApplyPositionPreset("Left-Hand Grip", new Vector3(-0.5f, -0.2f, 0.3f), new Vector3(0f, -90f, 0f));
    }
    
    [ContextMenu("Apply Back Sheath Preset")]
    public void ApplyBackSheathPreset()
    {
        ApplyPositionPreset("Back Sheath", new Vector3(0f, 0.5f, -0.3f), new Vector3(45f, 0f, 0f));
    }
    
    [ContextMenu("Apply Hip Holster Preset")]
    public void ApplyHipHolsterPreset()
    {
        ApplyPositionPreset("Hip Holster", new Vector3(0.8f, -0.5f, 0f), new Vector3(0f, 0f, -45f));
    }
    
    void ApplyPositionPreset(string presetName, Vector3 position, Vector3 rotation)
    {
        if (weaponSlotObject == null)
        {
            FindOrCreateWeaponSlot();
        }
        
        BaseWeapon[] weapons = weaponSlotObject.GetComponentsInChildren<BaseWeapon>();
        int appliedCount = 0;
        
        foreach (BaseWeapon weapon in weapons)
        {
            weapon.transform.localPosition = position;
            weapon.transform.localRotation = Quaternion.Euler(rotation);
            weapon.transform.localScale = Vector3.one;
            appliedCount++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Applied {presetName} preset to {appliedCount} weapons: Pos{position}, Rot{rotation}");
        }
    }
    
    // Copy position from one weapon to WeaponData for use as template
    [ContextMenu("Copy Selected Weapon Position as Template")]  
    public void CopySelectedWeaponPositionAsTemplate()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            BaseWeapon selectedWeapon = UnityEditor.Selection.activeGameObject.GetComponent<BaseWeapon>();
            if (selectedWeapon != null)
            {
                Vector3 pos = selectedWeapon.transform.localPosition;
                Vector3 rot = selectedWeapon.transform.localRotation.eulerAngles;
                Vector3 scale = selectedWeapon.transform.localScale;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Template Position for {selectedWeapon.WeaponName}:");
                    Debug.Log($"Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                    Debug.Log($"Rotation: ({rot.x:F1}, {rot.y:F1}, {rot.z:F1})");
                    Debug.Log($"Scale: ({scale.x:F2}, {scale.y:F2}, {scale.z:F2})");
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (weaponSlotObject != null)
        {
            // Draw WeaponSlot
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(weaponSlotObject.transform.position, Vector3.one * 0.3f);
            
            // Draw line from player to weapon slot
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, weaponSlotObject.transform.position);
            
            if (showPositionGizmos)
            {
                // Draw weapon positions and orientations
                BaseWeapon[] weapons = weaponSlotObject.GetComponentsInChildren<BaseWeapon>();
                foreach (BaseWeapon weapon in weapons)
                {
                    if (weapon != null)
                    {
                        // Draw weapon position
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(weapon.transform.position, 0.1f);
                        
                        // Draw weapon forward direction
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(weapon.transform.position, weapon.transform.forward * 0.3f);
                        
                        // Draw weapon up direction
                        Gizmos.color = Color.green;
                        Gizmos.DrawRay(weapon.transform.position, weapon.transform.up * 0.2f);
                        
                        // Draw weapon right direction
                        Gizmos.color = Color.blue;
                        Gizmos.DrawRay(weapon.transform.position, weapon.transform.right * 0.2f);
                    }
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Always show WeaponSlot location (less prominent)
        if (weaponSlotObject != null && showPositionGizmos)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(weaponSlotObject.transform.position, Vector3.one * 0.2f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponSlotSetup))]
public class WeaponSlotSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        WeaponSlotSetup setup = (WeaponSlotSetup)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("WeaponSlot Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find/Create WeaponSlot"))
        {
            setup.FindOrCreateWeaponSlot();
        }
        
        if (GUILayout.Button("Move Existing Weapons to WeaponSlot"))
        {
            setup.MoveExistingWeaponsToSlot();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Position Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Positions", GUILayout.Height(30)))
        {
            setup.SaveWeaponPositionsToData();
        }
        if (GUILayout.Button("Load Positions", GUILayout.Height(30)))
        {
            setup.LoadWeaponPositionsFromData();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Copy Selected Weapon Position as Template"))
        {
            setup.CopySelectedWeaponPositionAsTemplate();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Position Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Right Grip"))
        {
            setup.ApplyRightHandGripPreset();
        }
        if (GUILayout.Button("Left Grip"))
        {
            setup.ApplyLeftHandGripPreset();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Back Sheath"))
        {
            setup.ApplyBackSheathPreset();
        }
        if (GUILayout.Button("Hip Holster"))
        {
            setup.ApplyHipHolsterPreset();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (setup.GetWeaponSlot() != null)
        {
            EditorGUILayout.HelpBox($"WeaponSlot found: {setup.GetWeaponSlot().name}", MessageType.Info);
            
            BaseWeapon[] weapons = setup.GetWeaponSlot().GetComponentsInChildren<BaseWeapon>();
            if (weapons.Length > 0)
            {
                EditorGUILayout.HelpBox($"Found {weapons.Length} weapon(s) in WeaponSlot", MessageType.Info);
                
                // Show current weapon positions
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Weapon Positions:", EditorStyles.boldLabel);
                
                foreach (BaseWeapon weapon in weapons)
                {
                    if (weapon != null)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"🗡️ {weapon.WeaponName}", EditorStyles.boldLabel);
                        
                        Vector3 pos = weapon.transform.localPosition;
                        Vector3 rot = weapon.transform.localRotation.eulerAngles;
                        
                        EditorGUILayout.LabelField($"Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                        EditorGUILayout.LabelField($"Rotation: ({rot.x:F1}, {rot.y:F1}, {rot.z:F1})");
                        
                        if (GUILayout.Button($"Select {weapon.WeaponName}", GUILayout.Height(20)))
                        {
                            UnityEditor.Selection.activeGameObject = weapon.gameObject;
                            UnityEditor.SceneView.FrameLastActiveSceneView();
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No WeaponSlot found. Click 'Find/Create WeaponSlot' to set one up.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("💡 Workflow Tips:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. Place weapon in WeaponSlot\n" +
            "2. Use Scene View to position visually\n" +
            "3. Click 'Save Positions' to store in WeaponData\n" +
            "4. Test pickup to verify positioning", 
            MessageType.Info);
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(setup);
        }
    }
}
#endif