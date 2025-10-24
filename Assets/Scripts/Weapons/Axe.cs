using UnityEngine;

public class Axe : BaseWeapon
{
    [Header("Axe Specific")]
    [Range(0.8f, 2f)] public float heavyAttackMultiplier = 1.5f;
    public bool canHeavyAttack = true;
    public float heavyAttackChargeTime = 1f;
    
    private bool isChargingHeavyAttack = false;
    private float chargeStartTime;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set default values for axe
        if (weaponName == "Base Weapon") weaponName = "Axe";
        if (attackAnimations.Length == 0) 
        {
            attackAnimations = new string[] { "Axe Swing", "Axe Heavy Swing" };
        }
        
        // Axes typically hit harder but slower
        attackSpeed = Mathf.Max(attackSpeed, 1.2f);
        attackDamage = Mathf.Max(attackDamage, 2);
    }
    
    public override void Attack(Camera playerCamera)
    {
        base.Attack(playerCamera);
    }
    
    protected override void PerformAttackRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Axe has a powerful single-target attack
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            OnHit(hit);
        }
    }
    
    protected override void DealDamage(Actor target)
    {
        // Axes deal more damage but are slower
        int finalDamage = Mathf.RoundToInt(attackDamage * heavyAttackMultiplier);
        target.TakeDamage(finalDamage);
    }
    
    protected override void TriggerAttackAnimation()
    {
        // Axes have fewer but more impactful animations
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            string animationToPlay = attackAnimations[0]; // Usually just one heavy swing
            OnAnimationChange?.Invoke(animationToPlay);
        }
    }
}