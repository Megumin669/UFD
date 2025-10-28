using UnityEngine;

public class WeaponPickup : PickUpBase
{
    [Header("Weapon Pickup")]
    public WeaponData weaponData;
    public bool replaceCurrentWeapon = true;
    
    [Header("Visual")]
    public Transform weaponModel; // The visual model of the weapon
    
    protected override void Start()
    {
        base.Start();
        
        // Apply weapon data visual settings if available
        if (weaponData != null)
        {
            rotationSpeed = weaponData.pickupRotationSpeed;
            bobSpeed = weaponData.pickupBobSpeed;
            bobHeight = weaponData.pickupBobHeight;
        }
    }
    
    protected override void HandlePickup(FirstPersonController player)
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"WeaponPickup {gameObject.name} has no WeaponData assigned!");
            return;
        }
        
        if (replaceCurrentWeapon)
        {
            // Replace current weapon with this one
            ReplacePlayerWeapon(player);
        }
        else
        {
            // Add to weapon inventory (if implemented in the future)
            AddToPlayerInventory(player);
        }
        
        Debug.Log($"Player picked up: {weaponData.weaponName}");
    }
    
    void ReplacePlayerWeapon(FirstPersonController player)
    {
        // Find the WeaponSlot GameObject
        GameObject weaponSlot = FindWeaponSlot(player.transform);
        if (weaponSlot == null)
        {
            Debug.LogError($"No GameObject with tag 'WeaponSlot' found in player hierarchy!");
            return;
        }
        
        // Create new weapon from prefab
        if (weaponData.weaponPrefab != null)
        {
            GameObject newWeaponObj = Instantiate(weaponData.weaponPrefab, weaponSlot.transform);
            BaseWeapon newWeapon = newWeaponObj.GetComponent<BaseWeapon>();
            
            if (newWeapon != null)
            {
                // Apply position and rotation from WeaponData (saved via visual tool)
                newWeaponObj.transform.localPosition = weaponData.weaponSlotPosition;
                newWeaponObj.transform.localRotation = Quaternion.Euler(weaponData.weaponSlotRotation);
                newWeaponObj.transform.localScale = weaponData.weaponSlotScale;
                
                // Apply weapon data to the weapon (handled by the weapon's ApplyWeaponData method)
                switch (weaponData.weaponType)
                {
                    case WeaponType.Ranged:
                        if (newWeapon is RangedWeapon rangedWeapon)
                            rangedWeapon.ApplyWeaponData(weaponData);
                        break;
                    case WeaponType.Magic:
                        if (newWeapon is MagicWeapon magicWeapon)
                            magicWeapon.ApplyWeaponData(weaponData);
                        break;
                    case WeaponType.Melee:
                        if (newWeapon is MeleeWeapon meleeWeapon)
                            meleeWeapon.ApplyWeaponData(weaponData);
                        break;
                }
                
                // Use the player's new ReplaceCurrentWeapon method
                player.ReplaceCurrentWeapon(newWeapon);
                
                Debug.Log($"Replaced weapon with {weaponData.weaponName} in WeaponSlot");
            }
            else
            {
                Debug.LogError($"Weapon prefab {weaponData.weaponPrefab.name} doesn't have a BaseWeapon component!");
                Destroy(newWeaponObj);
            }
        }
        else
        {
            Debug.LogError($"WeaponData {weaponData.name} has no weapon prefab assigned!");
        }
    }
    
    GameObject FindWeaponSlot(Transform parent)
    {
        // Check if this object has the WeaponSlot tag
        if (parent.CompareTag("WeaponSlot"))
        {
            return parent.gameObject;
        }
        
        // Search through all children recursively
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject found = FindWeaponSlot(parent.GetChild(i));
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    void AddToPlayerInventory(FirstPersonController player)
    {
        // This would be for a future inventory system
        // For now, just replace the current weapon
        ReplacePlayerWeapon(player);
    }
    

    

    
    // Validation method for editor
    void OnValidate()
    {
        if (weaponData != null && weaponModel != null)
        {
            // You could auto-set the weapon model based on the weapon data
            gameObject.name = $"Pickup_{weaponData.weaponName}";
        }
    }
    
    // Enhanced debug visualization
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (weaponData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.2f);
            
            // Draw weapon type indicator
            switch (weaponData.weaponType)
            {
                case WeaponType.Melee:
                    Gizmos.color = Color.red;
                    break;
                case WeaponType.Ranged:
                    Gizmos.color = Color.blue;
                    break;
                case WeaponType.Magic:
                    Gizmos.color = Color.magenta;
                    break;
            }
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, 0.3f);
        }
    }
}