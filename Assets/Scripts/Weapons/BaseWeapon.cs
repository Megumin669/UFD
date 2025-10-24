using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName = "Base Weapon";
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public float hitEffectDuration = 10f;
    public AudioClip[] attackSounds;
    public AudioClip hitSound;
    
    [Header("Animation")]
    public string[] attackAnimations = { "Attack 1", "Attack 2" };
    
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
    }
    
    public virtual bool CanAttack()
    {
        return readyToAttack && !attacking;
    }
    
    public virtual void Attack(Camera playerCamera)
    {
        if (!CanAttack()) return;
        
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
        
        // Deal damage
        if (hit.transform.TryGetComponent<Actor>(out Actor actor))
        {
            DealDamage(actor);
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
        target.TakeDamage(attackDamage);
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
}