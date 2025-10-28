using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MeleeWeapon))]
public class MeleeWeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeleeWeapon meleeWeapon = (MeleeWeapon)target;
        
        // Draw WeaponData field
        EditorGUILayout.LabelField("Melee Weapon Settings", EditorStyles.boldLabel);
        
        SerializedProperty weaponDataProp = serializedObject.FindProperty("weaponData");
        EditorGUILayout.PropertyField(weaponDataProp);
        
        if (meleeWeapon.weaponData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("All weapon stats are loaded from the WeaponData ScriptableObject above.\nBase weapon inspector fields are hidden to prevent confusion.", MessageType.Info);
            
            // Show current values from WeaponData
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Values (From WeaponData):", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Weapon Name", meleeWeapon.weaponData.weaponName);
            EditorGUILayout.IntField("Attack Damage", meleeWeapon.weaponData.attackDamage);
            EditorGUILayout.FloatField("Attack Speed", meleeWeapon.weaponData.attackSpeed);
            EditorGUILayout.FloatField("Attack Distance", meleeWeapon.weaponData.attackDistance);
            EditorGUILayout.FloatField("Slash Range", meleeWeapon.weaponData.slashRange);
            EditorGUILayout.Toggle("Can Combo", meleeWeapon.weaponData.canCombo);
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button($"Edit {meleeWeapon.weaponData.name}", GUILayout.Height(25)))
            {
                Selection.activeObject = meleeWeapon.weaponData;
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Assign a WeaponData ScriptableObject to configure this weapon.", MessageType.Warning);
            
            // Still show base weapon settings if no WeaponData
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Base Weapon Settings (Fallback)", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(RangedWeapon))]
public class RangedWeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RangedWeapon rangedWeapon = (RangedWeapon)target;
        
        EditorGUILayout.LabelField("Ranged Weapon Settings", EditorStyles.boldLabel);
        
        SerializedProperty weaponDataProp = serializedObject.FindProperty("weaponData");
        EditorGUILayout.PropertyField(weaponDataProp);
        
        if (rangedWeapon.weaponData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("All weapon stats are loaded from the WeaponData ScriptableObject above.", MessageType.Info);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Values (From WeaponData):", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Weapon Name", rangedWeapon.weaponData.weaponName);
            EditorGUILayout.IntField("Attack Damage", rangedWeapon.weaponData.attackDamage);
            EditorGUILayout.FloatField("Projectile Speed", rangedWeapon.weaponData.projectileSpeed);
            EditorGUILayout.FloatField("Max Draw Time", rangedWeapon.weaponData.maxDrawTime);
            EditorGUILayout.Toggle("Use Gravity", rangedWeapon.weaponData.useGravity);
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button($"Edit {rangedWeapon.weaponData.name}", GUILayout.Height(25)))
            {
                Selection.activeObject = rangedWeapon.weaponData;
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Assign a WeaponData ScriptableObject to configure this weapon.", MessageType.Warning);
            DrawDefaultInspector();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(MagicWeapon))]
public class MagicWeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MagicWeapon magicWeapon = (MagicWeapon)target;
        
        EditorGUILayout.LabelField("Magic Weapon Settings", EditorStyles.boldLabel);
        
        SerializedProperty weaponDataProp = serializedObject.FindProperty("weaponData");
        EditorGUILayout.PropertyField(weaponDataProp);
        
        if (magicWeapon.weaponData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("All weapon stats are loaded from the WeaponData ScriptableObject above.", MessageType.Info);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Values (From WeaponData):", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Weapon Name", magicWeapon.weaponData.weaponName);
            EditorGUILayout.IntField("Attack Damage", magicWeapon.weaponData.attackDamage);
            EditorGUILayout.FloatField("Projectile Speed", magicWeapon.weaponData.staffProjectileSpeed);
            EditorGUILayout.FloatField("Explosion Radius", magicWeapon.weaponData.explosionRadius);
            EditorGUILayout.IntField("Explosion Damage", magicWeapon.weaponData.explosionDamage);
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button($"Edit {magicWeapon.weaponData.name}", GUILayout.Height(25)))
            {
                Selection.activeObject = magicWeapon.weaponData;
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Assign a WeaponData ScriptableObject to configure this weapon.", MessageType.Warning);
            DrawDefaultInspector();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif