using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(FirstPersonController))]
public class FirstPersonControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FirstPersonController controller = (FirstPersonController)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Weapon Selection", EditorStyles.boldLabel);
        
        if (controller.availableWeapons != null && controller.availableWeapons.Length > 0)
        {
            string[] weaponNames = new string[controller.availableWeapons.Length];
            for (int i = 0; i < controller.availableWeapons.Length; i++)
            {
                weaponNames[i] = controller.availableWeapons[i] != null ? 
                    controller.availableWeapons[i].weaponName : "Unknown Weapon";
            }
            
            int newSelectedIndex = EditorGUILayout.Popup("Selected Weapon", controller.selectedWeaponIndex, weaponNames);
            
            if (newSelectedIndex != controller.selectedWeaponIndex)
            {
                controller.selectedWeaponIndex = newSelectedIndex;
                if (Application.isPlaying)
                {
                    controller.SetCurrentWeapon(newSelectedIndex);
                }
            }
            
            GUILayout.Space(5);
            GUILayout.Label($"Current Weapon: {controller.GetCurrentWeaponName()}", EditorStyles.helpBox);
            
            if (GUILayout.Button("Refresh Weapon List"))
            {
                controller.UpdateWeaponList();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No weapons found. Add weapon scripts as child objects.", MessageType.Info);
            if (GUILayout.Button("Refresh Weapon List"))
            {
                controller.UpdateWeaponList();
            }
        }
        
        if (Application.isPlaying)
        {
            GUILayout.Space(10);
            GUILayout.Label("Runtime Controls", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Weapon"))
            {
                controller.PreviousWeapon();
            }
            if (GUILayout.Button("Next Weapon"))
            {
                controller.NextWeapon();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif