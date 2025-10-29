using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    // All weapon settings now come from WeaponData - no duplicates needed
    
    // Runtime values loaded from WeaponData (final calculated values)
    protected string weaponName = "Base Weapon";
    protected float attackDistance = 3f;
    protected float attackDelay = 0.4f;
    protected float attackSpeed = 1f;
    protected int attackDamage = 1;
    protected LayerMask attackLayer;
    protected GameObject hitEffect;
    protected float hitEffectDuration = 10f;
    protected AudioClip[] attackSounds;
    protected AudioClip hitSound;
    protected string[] attackAnimations = { "Attack 1", "Attack 2" };
    
    // Damage targeting system
    protected string[] damageableTags = { "Player" };
    
    // Stamina system integration
    protected Stamina staminaComponent;
    protected int staminaCost = 10; // Default stamina cost for attacks
    
    // Upgrade system values
    protected float criticalChance = 0f;
    protected float criticalMultiplier = 2f;
    protected float comboDamageMultiplier = 1f;
    protected float statusEffectChance = 0f;
    protected float lifestealPercentage = 0f;
    protected bool canPenetrate = false;
    protected int penetrationCount = 1;
    
    protected AudioSource audioSource;
    protected bool attacking = false;
    protected bool readyToAttack = true;
    protected int attackCount = 0;
    
    // Events
    public System.Action<string> OnAnimationChange;
    public System.Action<bool> OnAttackStateChange;
    
    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Try to find stamina component in parent hierarchy (player)
        staminaComponent = GetComponentInParent<Stamina>();
        if (staminaComponent == null)
        {
            // Look for stamina component in same GameObject
            staminaComponent = GetComponent<Stamina>();
        }
    }
    
    public virtual bool CanAttack()
    {
        // Check basic attack readiness
        if (!readyToAttack || attacking)
            return false;
        
        // Check stamina availability if stamina system is present
        if (staminaComponent != null && staminaCost > 0)
        {
            return staminaComponent.HasSufficientStamina(staminaCost);
        }
        
        return true;
    }
    
    public virtual void Attack(Camera playerCamera)
    {
        if (!CanAttack()) return;
        
        // Consume stamina if stamina system is present
        if (staminaComponent != null && staminaCost > 0)
        {
            if (!staminaComponent.ConsumeStamina(staminaCost))
            {
                // Failed to consume stamina - attack cancelled
                return;
            }
        }
        
        readyToAttack = false;
        attacking = true;
        OnAttackStateChange?.Invoke(true);
        
        // Play attack sound
        PlayAttackSound();
        
        // Trigger animation
        TriggerAttackAnimation();
        
        // Schedule attack raycast and reset
        Invoke(nameof(PerformAttackRaycast), attackDelay);
        Invoke(nameof(ResetAttack), attackSpeed);
    }
    
    protected virtual void PlayAttackSound()
    {
        if (audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            AudioClip randomSound = attackSounds[Random.Range(0, attackSounds.Length)];
            if (randomSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(randomSound);
            }
        }
    }
    
    protected virtual void TriggerAttackAnimation()
    {
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            string animationToPlay = attackAnimations[attackCount % attackAnimations.Length];
            OnAnimationChange?.Invoke(animationToPlay);
            attackCount++;
        }
    }
    
    protected virtual void PerformAttackRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            OnHit(hit);
        }
    }
    
    protected virtual void OnHit(RaycastHit hit)
    {
        // Spawn hit effect
        SpawnHitEffect(hit.point);
        
        // Play hit sound
        PlayHitSound();
        
        // Deal damage - check tags first, then use Health or Actor component
        if (CanDamageTarget(hit.transform.gameObject))
        {
            if (hit.transform.TryGetComponent<Health>(out Health health))
            {
                DealDamageToHealth(health);
            }
            else if (hit.transform.TryGetComponent<Actor>(out Actor actor))
            {
                DealDamage(actor); // Legacy fallback - will be removed
            }
        }
    }
    
    protected virtual void SpawnHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
            Destroy(effect, hitEffectDuration);
        }
    }
    
    protected virtual void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    protected virtual void DealDamage(Actor target)
    {
        int finalDamage = CalculateFinalDamage();
        target.TakeDamage(finalDamage);
        
        // Handle lifesteal
        if (lifestealPercentage > 0f)
        {
            HandleLifesteal(finalDamage);
        }
    }
    
    // Deal damage to Health component
    protected virtual void DealDamageToHealth(Health healthComponent)
    {
        int finalDamage = CalculateFinalDamage();
        healthComponent.TakeDamage(finalDamage);
        
        // Handle lifesteal
        if (lifestealPercentage > 0f)
        {
            HandleLifesteal(finalDamage);
        }
    }
    
    // Check if target can be damaged based on tags
    protected virtual bool CanDamageTarget(GameObject target)
    {
        // If no damage tags specified, allow damage to any object with Actor or Health component
        if (damageableTags == null || damageableTags.Length == 0)
        {
            return target.TryGetComponent<Actor>(out _) || target.TryGetComponent<Health>(out _);
        }
        
        // Check if target has any of the specified damage tags (with safe tag checking)
        foreach (string damageTag in damageableTags)
        {
            if (!string.IsNullOrEmpty(damageTag) && HasTag(target, damageTag))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Safe tag checking that won't crash if tag doesn't exist
    private bool HasTag(GameObject target, string tagName)
    {
        try
        {
            return target.CompareTag(tagName);
        }
        catch (UnityException)
        {
            // Tag doesn't exist in Unity's tag list - check manually
            return target.tag.Equals(tagName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
    
    // Calculate final damage with all modifiers
    protected virtual int CalculateFinalDamage()
    {
        int baseDamage = attackDamage;
        
        // Apply combo damage multiplier
        if (attackCount > 1)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * comboDamageMultiplier);
        }
        
        // Apply critical hit
        if (Random.Range(0f, 1f) < criticalChance)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
        }
        
        return baseDamage;
    }
    
    // Handle lifesteal effect
    protected virtual void HandleLifesteal(int damageDealt)
    {
        if (lifestealPercentage > 0f)
        {
            int healAmount = Mathf.RoundToInt(damageDealt * lifestealPercentage);
            // TODO: Implement healing system for player
            Debug.Log($"Lifesteal: Healed {healAmount} HP");
        }
    }
    
    // Apply WeaponData values including upgrades (to be called by child classes)
    protected virtual void ApplyUpgradeValues(WeaponData data)
    {
        if (data == null) return;
        
        // Calculate final values with upgrade bonuses
        attackDamage = data.attackDamage + data.bonusDamage;
        attackDistance = data.attackDistance;
        attackSpeed = Mathf.Max(0.1f, data.baseAttackSpeed);
        attackDelay = Mathf.Max(0.05f, data.baseAttackDelay);
        
        // Apply upgrade modifiers
        criticalChance = data.criticalChance;
        criticalMultiplier = data.criticalMultiplier;
        comboDamageMultiplier = data.comboDamageMultiplier;
        statusEffectChance = data.statusEffectChance;
        lifestealPercentage = data.lifestealPercentage;
        canPenetrate = data.canPenetrate;
        penetrationCount = data.penetrationCount;
        
        // Apply damage targeting settings
        damageableTags = data.damageableTags;
        
        // Apply stamina cost (use weapon-specific cost from WeaponData)
        staminaCost = Mathf.RoundToInt(data.staminaCost);
    }
    
    protected virtual void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        OnAttackStateChange?.Invoke(false);
    }
    
    // Public getters for controller
    public bool IsAttacking => attacking;
    public string WeaponName => weaponName;
    public string GetWeaponName() => weaponName;
}