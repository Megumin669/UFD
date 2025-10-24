using UnityEngine;

public class Sword : BaseWeapon
{
    [Header("Sword Specific")]
    [Range(0.5f, 3f)] public float slashRange = 1.5f;
    public bool canCombo = true;
    public float comboWindow = 1f;
    
    private float lastAttackTime;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set default values for sword
        if (weaponName == "Base Weapon") weaponName = "Sword";
        if (attackAnimations.Length == 0) 
        {
            attackAnimations = new string[] { "Sword Attack 1", "Sword Attack 2" };
        }
    }
    
    public override bool CanAttack()
    {
        return base.CanAttack();
    }
    
    public override void Attack(Camera playerCamera)
    {
        // Check combo timing
        if (canCombo && Time.time - lastAttackTime > comboWindow)
        {
            attackCount = 0; // Reset combo if too much time passed
        }
        
        lastAttackTime = Time.time;
        base.Attack(playerCamera);
    }
    
    protected override void PerformAttackRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Sword uses a wider attack pattern - multiple raycasts for slash effect
        Vector3[] directions = {
            cam.transform.forward,
            cam.transform.forward + cam.transform.right * 0.3f,
            cam.transform.forward - cam.transform.right * 0.3f
        };
        
        bool hitSomething = false;
        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(cam.transform.position, direction.normalized, out RaycastHit hit, attackDistance, attackLayer))
            {
                if (!hitSomething) // Only hit once per attack
                {
                    OnHit(hit);
                    hitSomething = true;
                }
            }
        }
    }
    
    protected override void DealDamage(Actor target)
    {
        // Sword can have critical hits based on combo
        int finalDamage = attackDamage;
        if (canCombo && attackCount > 1)
        {
            finalDamage = Mathf.RoundToInt(attackDamage * 1.2f); // 20% damage bonus for combo
        }
        
        target.TakeDamage(finalDamage);
    }
}
