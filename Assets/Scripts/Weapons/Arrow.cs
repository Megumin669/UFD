using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public int damage = 1;
    public float lifeTime = 10f;
    public bool stickToSurfaces = true;
    public LayerMask targetLayers = -1;
    
    [Header("Flight Settings")]
    public float gravityScale = 1f;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private GameObject shooter;
    private bool hasHit = false;
    private Rigidbody rb;
    private AudioSource audioSource;
    private float drawPower = 1f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Setup arrow physics for simple forward flight
        if (rb != null)
        {
            rb.freezeRotation = true; // No rotation during flight
            rb.useGravity = true; // Let gravity pull it down naturally
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifeTime);
    }
    
    void Start()
    {
        // Ignore collision with shooter
        if (shooter != null && shooter.TryGetComponent<Collider>(out Collider shooterCollider))
        {
            Collider arrowCollider = GetComponent<Collider>();
            if (arrowCollider != null)
            {
                Physics.IgnoreCollision(arrowCollider, shooterCollider);
            }
        }
        
    }
    
    void FixedUpdate()
    {
        // Arrow always faces forward - no rotation changes
        // Gravity will naturally make it fall over time
        // Draw power affects how long it maintains forward momentum
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }
    
    void HandleHit(Collider hitCollider)
    {
        if (hasHit) return;
        
        // Don't hit the shooter
        if (hitCollider.gameObject == shooter) return;
        
        // Check if we should damage this target
        if (((1 << hitCollider.gameObject.layer) & targetLayers) != 0)
        {
            // Try to deal damage
            if (hitCollider.TryGetComponent<Actor>(out Actor actor))
            {
                actor.TakeDamage(damage);
            }
        }
        
        hasHit = true;
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Spawn hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Stop physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (stickToSurfaces)
        {
            // Stick to surface
            transform.SetParent(hitCollider.transform);
            
            // Disable collider to prevent further hits
            Collider arrowCollider = GetComponent<Collider>();
            if (arrowCollider != null)
            {
                arrowCollider.enabled = false;
            }
        }
        else
        {
            // Destroy arrow on impact
            Destroy(gameObject, 0.1f);
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void SetShooter(GameObject shooterObject)
    {
        shooter = shooterObject;
    }
    
    public void SetLifetime(float newLifetime)
    {
        lifeTime = newLifetime;
        CancelInvoke();
        Destroy(gameObject, lifeTime);
    }
    
    public void SetDrawPower(float power)
    {
        drawPower = power;
        // Higher draw power means stronger initial velocity and less affected by gravity
        if (rb != null)
        {
            rb.mass = 1f / Mathf.Max(power, 0.1f); // Lower mass = less gravity effect
        }
    }
}