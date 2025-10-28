using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class WeaponDataCreator
{
    [MenuItem("EFD/Create Weapon Data/Sword")]
    public static void CreateSwordData()
    {
        WeaponData swordData = CreateWeaponData("SwordData", WeaponType.Melee);
        swordData.attackDamage = 15;
        swordData.attackSpeed = 1.2f;
        swordData.attackDistance = 2.5f;
        swordData.slashRange = 1.8f;
        swordData.canCombo = true;
        swordData.comboWindow = 1.2f;
        // Default sword positioning (adjust as needed)
        swordData.weaponSlotPosition = new Vector3(0.5f, -0.2f, 0.3f);
        swordData.weaponSlotRotation = new Vector3(0f, 90f, 0f);
        swordData.description = "A sharp blade for close combat. Reliable and balanced with combo potential.";
        
        AssetDatabase.CreateAsset(swordData, "Assets/ScriptableObjects/WeaponData/SwordData.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = swordData;
    }
    
    [MenuItem("EFD/Create Weapon Data/Axe")]
    public static void CreateAxeData()
    {
        WeaponData axeData = CreateWeaponData("AxeData", WeaponType.Melee);
        axeData.attackDamage = 25;
        axeData.attackSpeed = 0.8f;
        axeData.attackDistance = 2.8f;
        axeData.slashRange = 1.2f;
        axeData.canCombo = false; // Axes are too heavy for combos
        axeData.comboWindow = 0f;
        axeData.description = "A heavy axe that deals massive damage but swings slowly. No combo capability.";
        
        AssetDatabase.CreateAsset(axeData, "Assets/ScriptableObjects/WeaponData/AxeData.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = axeData;
    }
    
    [MenuItem("EFD/Create Weapon Data/Bow")]
    public static void CreateBowData()
    {
        WeaponData bowData = CreateWeaponData("BowData", WeaponType.Ranged);
        bowData.attackDamage = 20;
        bowData.attackSpeed = 1.5f;
        bowData.attackDistance = 30f;
        bowData.projectileSpeed = 30f;
        bowData.maxDrawTime = 2f;
        bowData.minDrawTime = 0.3f;
        bowData.useGravity = true;
        bowData.gravityMultiplier = 1f;
        bowData.canChargeDraw = true;
        bowData.maxDrawAngle = 30f;
        bowData.description = "A ranged weapon that fires arrows. Hold to draw for more power, release to fire.";
        
        AssetDatabase.CreateAsset(bowData, "Assets/ScriptableObjects/WeaponData/BowData.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = bowData;
    }
    
    [MenuItem("EFD/Create Weapon Data/Staff")]
    public static void CreateStaffData()
    {
        WeaponData staffData = CreateWeaponData("StaffData", WeaponType.Magic);
        staffData.attackDamage = 10; // Base damage, explosion damage is separate
        staffData.attackSpeed = 1.0f;
        staffData.attackDistance = 25f;
        staffData.staffProjectileSpeed = 15f;
        staffData.explosionRadius = 5f;
        staffData.explosionDamage = 25;
        staffData.explosionLayer = -1; // Hit all layers by default
        staffData.explosionEffectDuration = 5f;
        staffData.description = "A magical staff that fires explosive projectiles with area damage.";
        
        AssetDatabase.CreateAsset(staffData, "Assets/ScriptableObjects/WeaponData/StaffData.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = staffData;
    }
    
    private static WeaponData CreateWeaponData(string name, WeaponType type)
    {
        WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();
        weaponData.weaponName = name.Replace("Data", "");
        weaponData.weaponType = type;
        weaponData.pickupRotationSpeed = 1f;
        weaponData.pickupBobSpeed = 1f;
        weaponData.pickupBobHeight = 0.2f;
        
        return weaponData;
    }
}
#endif