using UnityEngine;

public abstract class PickUpBase : MonoBehaviour
{
    [Header("Pickup Settings")]
    public LayerMask playerLayer = 1; // Default to "Default" layer
    [Range(0.1f, 5f)] public float rotationSpeed = 1f;
    [Range(0.1f, 2f)] public float bobSpeed = 1f;
    [Range(0.1f, 1f)] public float bobHeight = 0.2f;
    public bool enableRotation = true;
    public bool enableBobbing = true;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    
    [Header("Effects")]
    public GameObject pickupEffect;
    public float effectDuration = 3f;
    
    protected Vector3 startPosition;
    protected float bobTimer = 0f;
    
    protected virtual void Start()
    {
        startPosition = transform.position;
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
    }
    
    protected virtual void Update()
    {
        HandleVisualEffects();
    }
    
    protected virtual void HandleVisualEffects()
    {
        if (enableRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * 90f * Time.deltaTime);
        }
        
        if (enableBobbing)
        {
            bobTimer += bobSpeed * Time.deltaTime;
            float newY = startPosition.y + Mathf.Sin(bobTimer) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        // Check if the collider is on the player layer
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            // Try to get the FirstPersonController
            FirstPersonController player = other.GetComponent<FirstPersonController>();
            if (player == null)
                player = other.GetComponentInParent<FirstPersonController>();
            
            if (player != null)
            {
                OnPickedUp(player);
            }
        }
    }
    
    protected virtual void OnPickedUp(FirstPersonController player)
    {
        // Play pickup sound
        PlayPickupSound();
        
        // Spawn pickup effect
        SpawnPickupEffect();
        
        // Call the abstract method for specific pickup behavior
        HandlePickup(player);
        
        // Destroy the pickup object
        Destroy(gameObject, 0.1f); // Small delay to allow sound/effects
    }
    
    protected virtual void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    protected virtual void SpawnPickupEffect()
    {
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }
    
    // Abstract method that derived classes must implement
    protected abstract void HandlePickup(FirstPersonController player);
    
    // Debug visualization
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
