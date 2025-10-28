using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(BaseWeapon), true)]
public class BaseWeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BaseWeapon baseWeapon = (BaseWeapon)target;
        
        // Check if this is a unified weapon (has WeaponData field)
        bool isUnifiedWeapon = HasWeaponDataField(baseWeapon);
        
        if (isUnifiedWeapon)
        {
            // Let the specific weapon editor handle it
            return;
        }
        
        // For non-unified weapons, show default inspector
        DrawDefaultInspector();
    }
    
    bool HasWeaponDataField(BaseWeapon weapon)
    {
        var fields = weapon.GetType().GetFields();
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(WeaponData) && field.Name == "weaponData")
            {
                return true;
            }
        }
        return false;
    }
}
#endif