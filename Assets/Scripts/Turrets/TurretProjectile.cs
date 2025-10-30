using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Turret projectile component that handles physics-free movement like laser beams
/// Supports various projectile behaviors: instant, linear, guided, piercing, explosive
/// </summary>
public class TurretProjectile : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    // Configuration (set by turret)
    private Transform target;
    private int damage;
    private float speed;
    private float lifetime;
    private ProjectileBehavior behavior;
    private bool piercing;
    private int maxPierceTargets;
    private float explosionRadius;
    private int explosionDamage;
    private GameObject hitEffect;
    private GameObject explosionEffect;
    
    // Runtime state
    private Vector3 direction;
    private float travelDistance = 0f;
    private bool hasHit = false;
    private List<Transform> hitTargets = new List<Transform>();
    private float startTime;
    
    // Components
    private TrailRenderer trailRenderer;
    private LineRenderer lineRenderer;
    private Light projectileLight;
    
    // Properties
    public bool IsActive => !hasHit && Time.time - startTime < lifetime;
    
    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        projectileLight = GetComponent<Light>();
        
        startTime = Time.time;
    }
    
    /// <summary>
    /// Initialize projectile with turret data
    /// </summary>
    public void Initialize(Transform targetTransform, int projectileDamage, float projectileSpeed, 
                          float projectileLifetime, ProjectileBehavior projectileBehavior, 
                          bool projectilePiercing, int maxPierce, float explRadius, int explDamage,
                          GameObject hitFX, GameObject explFX)
    {
        target = targetTransform;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifetime = projectileLifetime;
        behavior = projectileBehavior;
        piercing = projectilePiercing;
        maxPierceTargets = maxPierce;
        explosionRadius = explRadius;
        explosionDamage = explDamage;
        hitEffect = hitFX;
        explosionEffect = explFX;
        
        // Calculate initial direction
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            direction = transform.forward;
        }
        
        // Handle instant projectiles (laser/hitscan)
        if (behavior == ProjectileBehavior.Instant)
        {
            HandleInstantHit();
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
        
        if (showDebugInfo)
        {
            Debug.Log($"[TurretProjectile] Initialized: Damage={damage}, Speed={speed}, Behavior={behavior}");
        }
    }
    
    void Update()
    {
        if (!IsActive || behavior == ProjectileBehavior.Instant) return;
        
        UpdateMovement();
        CheckCollisions();
    }
    
    void UpdateMovement()
    {
        Vector3 moveVector = Vector3.zero;
        
        switch (behavior)
        {
            case ProjectileBehavior.Linear:
                moveVector = direction * speed * Time.deltaTime;
                break;
                
            case ProjectileBehavior.Guided:
                UpdateGuidedMovement();
                moveVector = direction * speed * Time.deltaTime;
                break;
                
            case ProjectileBehavior.Ballistic:
                UpdateBallisticMovement();
                moveVector = direction * speed * Time.deltaTime;
                break;
                
            case ProjectileBehavior.Piercing:
            case ProjectileBehavior.Explosive:
                // Same as linear for movement
                moveVector = direction * speed * Time.deltaTime;
                break;
        }
        
        transform.position += moveVector;
        travelDistance += moveVector.magnitude;
        
        // Update rotation to face movement direction
        if (moveVector.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveVector.normalized);
        }
    }
    
    void UpdateGuidedMovement()
    {
        if (target != null && !HasTargetBeenHit(target))
        {
            // Gradually adjust direction towards target
            Vector3 targetDirection = (target.position - transform.position).normalized;
            direction = Vector3.Slerp(direction, targetDirection, 5f * Time.deltaTime).normalized;
        }
        // If target is null or already hit, continue in current direction
    }
    
    void UpdateBallisticMovement()
    {
        // Add gravity effect for ballistic projectiles
        direction += Vector3.down * 9.81f * Time.deltaTime;
        direction = direction.normalized;
    }
    
    void CheckCollisions()
    {
        // Use SphereCast for better collision detection
        float checkRadius = 0.1f;
        RaycastHit hit;
        
        if (Physics.SphereCast(transform.position, checkRadius, direction, out hit, speed * Time.deltaTime))
        {
            HandleHit(hit);
        }
    }
    
    void HandleInstantHit()
    {
        // Instant hit - raycast to target immediately
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = direction;
        float maxRange = 100f; // Max range for instant projectiles
        
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRange))
        {
            // Create visual laser effect
            CreateLaserEffect(rayOrigin, hit.point);
            
            // Handle the hit
            HandleHit(hit);
        }
        else
        {
            // No hit - create laser to max range
            CreateLaserEffect(rayOrigin, rayOrigin + rayDirection * maxRange);
        }
        
        // Instant projectiles destroy immediately after effect
        Destroy(gameObject, 0.1f);
    }
    
    void CreateLaserEffect(Vector3 start, Vector3 end)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            
            // Fade out the laser
            StartCoroutine(FadeLaser());
        }
    }
    
    IEnumerator FadeLaser()
    {
        float fadeTime = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime && lineRenderer != null)
        {
            float alpha = 1f - (elapsed / fadeTime);
            Color color = lineRenderer.material.color;
            color.a = alpha;
            lineRenderer.material.color = color;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
    
    void HandleHit(RaycastHit hit)
    {
        Transform hitTransform = hit.transform;
        
        // Check if we can damage this target
        if (!CanDamageTarget(hitTransform))
        {
            return; // Continue flying if we can't damage this target
        }
        
        // Check if we've already hit this target (for piercing projectiles)
        if (HasTargetBeenHit(hitTransform))
        {
            return; // Continue flying if we've already hit this target
        }
        
        // Deal damage
        DealDamage(hitTransform, hit.point);
        
        // Add to hit targets list
        hitTargets.Add(hitTransform);
        
        // Check if projectile should stop
        if (!piercing || hitTargets.Count >= maxPierceTargets)
        {
            // Create explosion if applicable
            if (explosionRadius > 0)
            {
                CreateExplosion(hit.point);
            }
            
            // Create hit effect
            CreateHitEffect(hit.point, hit.normal);
            
            hasHit = true;
            Destroy(gameObject, 0.1f); // Small delay to allow effects to spawn
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[TurretProjectile] Hit {hitTransform.name} for {damage} damage");
        }
    }
    
    bool CanDamageTarget(Transform target)
    {
        // Check for Health component (new system)
        if (target.GetComponent<Health>() != null)
        {
            return true;
        }
        
        // Check for legacy Actor component
        if (target.GetComponent<Actor>() != null)
        {
            return true;
        }
        
        return false;
    }
    
    bool HasTargetBeenHit(Transform target)
    {
        return hitTargets.Contains(target);
    }
    
    void DealDamage(Transform target, Vector3 hitPoint)
    {
        // Try Health component first (new system)
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            return;
        }
        
        // Fallback to legacy Actor component
        Actor actor = target.GetComponent<Actor>();
        if (actor != null)
        {
            actor.TakeDamage(damage);
        }
    }
    
    void CreateExplosion(Vector3 explosionCenter)
    {
        if (explosionRadius <= 0) return;
        
        // Find all targets in explosion radius
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius);
        
        foreach (Collider col in colliders)
        {
            if (CanDamageTarget(col.transform) && !HasTargetBeenHit(col.transform))
            {
                // Calculate distance-based damage
                float distance = Vector3.Distance(explosionCenter, col.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                damageMultiplier = Mathf.Clamp01(damageMultiplier);
                
                int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                
                // Deal explosion damage
                Health health = col.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(finalDamage);
                }
                else
                {
                    Actor actor = col.GetComponent<Actor>();
                    if (actor != null)
                    {
                        actor.TakeDamage(finalDamage);
                    }
                }
                
                if (showDebugInfo)
                {
                    Debug.Log($"[TurretProjectile] Explosion hit {col.name} for {finalDamage} damage");
                }
            }
        }
        
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, explosionCenter, Quaternion.identity);
            Destroy(effect, 5f);
        }
        
        // Debug visualization
        if (showDebugInfo)
        {
            StartCoroutine(DrawDebugExplosion(explosionCenter, explosionRadius));
        }
    }
    
    void CreateHitEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hitEffect != null)
        {
            Quaternion effectRotation = Quaternion.LookRotation(hitNormal);
            GameObject effect = Instantiate(hitEffect, hitPoint, effectRotation);
            Destroy(effect, 3f);
        }
    }
    
    IEnumerator DrawDebugExplosion(Vector3 center, float radius)
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Draw a circle using debug lines
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)i / segments * 2f * Mathf.PI;
                float angle2 = (float)(i + 1) / segments * 2f * Mathf.PI;
                
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
                
                Debug.DrawLine(point1, point2, Color.red, Time.deltaTime);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugInfo && IsActive)
        {
            // Draw movement direction
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, direction * 2f);
            
            // Draw explosion radius preview
            if (explosionRadius > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }
        }
    }
}