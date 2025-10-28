using UnityEngine;

public class StaffProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 10f;
    public LayerMask collisionLayers = -1;
    
    private MagicWeapon parentMagicWeapon;
    private Rigidbody rb;
    private bool hasExploded = false;
    
    // Explosion data
    private float explosionRadius;
    private int explosionDamage;
    private LayerMask explosionLayer;
    
    // Effects
    private GameObject explosionEffect;
    private float explosionEffectDuration;
    private AudioClip explosionSound;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Auto-destroy after lifetime if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }
    
    void Start()
    {
        // Basic projectile setup
        if (rb != null)
        {
            // Ensure it's not kinematic at start for proper physics movement
            rb.isKinematic = false;
            rb.useGravity = false; // Magic projectiles don't fall
            rb.freezeRotation = true; // Keep projectile facing forward
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }
    
    void HandleCollision(Collider hitCollider)
    {
        if (hasExploded) return;
        
        // Check if we should collide with this object
        if (((1 << hitCollider.gameObject.layer) & collisionLayers) == 0) return;
        
        // Don't explode on the caster
        GameObject casterObject = null;
        if (parentMagicWeapon != null) casterObject = parentMagicWeapon.gameObject;
        if (parentMagicWeapon != null) casterObject = parentMagicWeapon.gameObject;
        
        if (casterObject != null && hitCollider.gameObject == casterObject) return;
        
        hasExploded = true;
        
        // Stop movement
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        
        // Trigger explosion
        if (parentMagicWeapon != null)
        {
            parentMagicWeapon.OnProjectileExplode(transform.position);
        }
        else if (parentMagicWeapon != null)
        {
            parentMagicWeapon.OnProjectileExplode(transform.position);
        }
        
        // Destroy projectile
        Destroy(gameObject, 0.1f);
    }
    
    public void SetMagicWeapon(MagicWeapon magicWeapon)
    {
        parentMagicWeapon = magicWeapon;
    }
    
    public void SetExplosionData(float radius, int damage, LayerMask layer)
    {
        explosionRadius = radius;
        explosionDamage = damage;
        explosionLayer = layer;
    }
    
    public void SetEffects(GameObject effect, float effectDuration, AudioClip sound)
    {
        explosionEffect = effect;
        explosionEffectDuration = effectDuration;
        explosionSound = sound;
    }
    
    public void SetLifetime(float newLifetime)
    {
        lifeTime = newLifetime;
        CancelInvoke();
        Destroy(gameObject, lifeTime);
    }
    
    // Debug visualization for projectile
    void OnDrawGizmos()
    {
        // Draw projectile trajectory (small sphere)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw explosion radius preview
        if (explosionRadius > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}