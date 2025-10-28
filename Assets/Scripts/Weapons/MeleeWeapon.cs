using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    [Header("Melee Weapon Settings")]
    public WeaponData weaponData;
    
    // Private fields - values come from WeaponData
    private float slashRange = 1.5f;
    private bool canCombo = true;
    private float comboWindow = 1f;
    private float lastAttackTime;
    
    // Audio fields from WeaponData
    private AudioClip[] swingSounds;
    private AudioClip[] comboSwingSounds;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Apply weapon data if available
        if (weaponData != null)
        {
            ApplyWeaponData(weaponData);
        }
    }
    
    // Method to apply WeaponData (called by pickup system or manually)
    public void ApplyWeaponData(WeaponData data)
    {
        if (data == null) return;
        
        weaponData = data;
        
        // Apply base weapon data with upgrade calculations
        ApplyUpgradeValues(data);
        
        // Apply basic properties
        weaponName = data.weaponName;
        attackLayer = data.attackLayer;
        hitEffect = data.hitEffect;
        hitEffectDuration = data.hitEffectDuration;
        attackSounds = data.attackSounds;
        hitSound = data.hitSound;
        attackAnimations = data.attackAnimations;
        
        // Apply damage targeting settings
        damageableTags = data.damageableTags;
        
        // Apply melee-specific settings
        slashRange = data.slashRange;
        canCombo = data.canCombo;
        comboWindow = data.comboWindow;
        
        // Apply melee stamina cost (use melee-specific cost from StaminaStats)
        if (staminaComponent != null)
        {
            var staminaStats = staminaComponent.GetStaminaStats();
            staminaCost = staminaStats.meleeAttackCost;
        }
        else
        {
            // Fallback to WeaponData stamina cost if no stamina component
            staminaCost = Mathf.RoundToInt(data.staminaCost);
        }
        
        // Apply swing sound settings
        swingSounds = data.swingSounds;
        comboSwingSounds = data.comboSwingSounds;
        
        // Note: Weapon positioning is handled by the pickup system using saved WeaponData positions
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
    
    protected override void TriggerAttackAnimation()
    {
        // Play swing sound synchronized with animation
        PlaySwingSound();
        
        // Call base animation trigger
        base.TriggerAttackAnimation();
    }
    
    void PlaySwingSound()
    {
        if (audioSource == null) return;
        
        // Choose appropriate swing sound based on combo state
        AudioClip[] soundsToUse = null;
        
        if (canCombo && attackCount > 0 && comboSwingSounds != null && comboSwingSounds.Length > 0)
        {
            // Use combo swing sounds for follow-up attacks
            soundsToUse = comboSwingSounds;
        }
        else if (swingSounds != null && swingSounds.Length > 0)
        {
            // Use regular swing sounds
            soundsToUse = swingSounds;
        }
        
        if (soundsToUse != null && soundsToUse.Length > 0)
        {
            AudioClip swingClip = soundsToUse[Random.Range(0, soundsToUse.Length)];
            if (swingClip != null)
            {
                audioSource.PlayOneShot(swingClip);
            }
        }
    }
    
    protected override void PerformAttackRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Melee weapons can use different attack patterns based on weapon type
        if (weaponData != null && weaponData.weaponName.ToLower().Contains("sword"))
        {
            PerformSwordAttack(cam);
        }
        else if (weaponData != null && weaponData.weaponName.ToLower().Contains("axe"))
        {
            PerformAxeAttack(cam);
        }
        else
        {
            // Default melee attack
            PerformDefaultMeleeAttack(cam);
        }
    }
    
    void PerformSwordAttack(Camera cam)
    {
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
    
    void PerformAxeAttack(Camera cam)
    {
        // Axe has more focused, powerful attack
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            OnHit(hit);
        }
    }
    
    void PerformDefaultMeleeAttack(Camera cam)
    {
        // Standard single raycast attack
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            OnHit(hit);
        }
    }
    
    protected override void DealDamage(Actor target)
    {
        // Calculate final damage with potential bonuses
        int finalDamage = attackDamage;
        
        // Combo bonus for swords
        if (canCombo && attackCount > 1 && weaponData != null && weaponData.weaponName.ToLower().Contains("sword"))
        {
            finalDamage = Mathf.RoundToInt(attackDamage * 1.2f); // 20% damage bonus for combo
        }
        
        target.TakeDamage(finalDamage);
    }
}