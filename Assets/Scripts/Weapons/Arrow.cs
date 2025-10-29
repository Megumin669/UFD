using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public int damage = 1;
    public float lifeTime = 10f;
    public bool stickToSurfaces = true;
    public LayerMask targetLayers = -1;
    
    [Header("Damage Target Configuration")]
    [Tooltip("Objects with these tags will receive damage from this arrow. Leave empty to damage all objects with Health or Actor components.")]
    public string[] damageableTags = { "Player" };
    
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
        
        // Basic arrow setup - physics will be configured by the bow
        if (rb != null)
        {
            // Don't set physics properties here - let the bow handle it
            // This prevents conflicts and camera shake issues
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
        
        // Check if we should damage this target - check tags first, then layer
        if (CanDamageTarget(hitCollider.gameObject) && ((1 << hitCollider.gameObject.layer) & targetLayers) != 0)
        {
            // Try to deal damage to Health component (current system)
            if (hitCollider.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
            }
            // Try to deal damage to Actor component (legacy fallback - will be removed)
            else if (hitCollider.TryGetComponent<Actor>(out Actor actor))
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
    
    public void SetDamageableTags(string[] tags)
    {
        damageableTags = tags;
    }
    
    // Check if target can be damaged based on tags
    private bool CanDamageTarget(GameObject target)
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
}