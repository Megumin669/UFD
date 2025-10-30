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
    private float arcHeightMultiplier;
    
    // Runtime state
    private Vector3 direction;
    private Vector3 velocity; // For ballistic projectiles
    private float travelDistance = 0f;
    private bool hasHit = false;
    private List<Transform> hitTargets = new List<Transform>();
    private float startTime;
    private Vector3 spawnPosition; // Track where projectile was spawned
    
    // Collision filtering
    private const float MIN_TRAVEL_DISTANCE = 2.5f; // Minimum distance before checking collisions (prevents hitting own turret)
    private LayerMask collisionMask; // What layers to check for collisions
    
    // Components
    private TrailRenderer trailRenderer;
    private LineRenderer lineRenderer;
    private Light projectileLight;
    private Rigidbody rb;
    
    // Properties
    public bool IsActive => !hasHit && Time.time - startTime < lifetime;
    
    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        projectileLight = GetComponent<Light>();
        rb = GetComponent<Rigidbody>();
        
        startTime = Time.time;
        spawnPosition = transform.position;
        
        // Set up collision mask - ignore Ignore Raycast layer (usually layer 2) and projectiles
        // This prevents hitting the turret that fired it
        collisionMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
    }
    
    /// <summary>
    /// Initialize projectile with turret data
    /// </summary>
    public void Initialize(Transform targetTransform, int projectileDamage, float projectileSpeed, 
                          float projectileLifetime, ProjectileBehavior projectileBehavior, 
                          bool projectilePiercing, int maxPierce, float explRadius, int explDamage,
                          GameObject hitFX, GameObject explFX, float arcHeight = 1f, Vector3 predictedPosition = default)
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
        arcHeightMultiplier = arcHeight;
        
        // Disable Rigidbody physics for our custom projectile movement
        if (rb != null)
        {
            rb.isKinematic = true; // We control movement manually
            rb.useGravity = false; // We apply gravity manually for ballistic
        }
        
        // For ballistic projectiles with predicted position, use that instead of target
        Vector3 targetPosition = (behavior == ProjectileBehavior.Ballistic && predictedPosition != default) 
            ? predictedPosition 
            : (target != null ? target.position : transform.position + transform.forward * 10f);
        
        // Calculate initial direction
        if (target != null || (behavior == ProjectileBehavior.Ballistic && predictedPosition != default))
        {
            direction = (targetPosition - transform.position).normalized;
            
            // For ballistic projectiles, calculate proper arc trajectory
            if (behavior == ProjectileBehavior.Ballistic)
            {
                CalculateBallisticTrajectory(targetPosition);
            }
        }
        else
        {
            direction = transform.forward;
            velocity = direction * speed;
        }
        
        // Handle instant projectiles (laser/hitscan)
        if (behavior == ProjectileBehavior.Instant)
        {
            HandleInstantHit();
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
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
                return; // Ballistic handles its own position update
                
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
        // Apply gravity to velocity
        velocity += Physics.gravity * Time.deltaTime;
        
        // Move using velocity
        Vector3 moveVector = velocity * Time.deltaTime;
        transform.position += moveVector;
        travelDistance += moveVector.magnitude; // Track travel distance for ballistic projectiles
        
        // Update rotation to face movement direction (makes projectile arc visually)
        if (velocity.magnitude > 0.01f)
        {
            direction = velocity.normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void CalculateBallisticTrajectory(Vector3 targetPosition)
    {
        // Calculate the trajectory for a ballistic arc
        Vector3 toTarget = targetPosition - transform.position;
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float verticalDistance = toTarget.y;
        
        // Use moderate arc angle for nice mortar visual (45-60 degrees)
        float gravity = Mathf.Abs(Physics.gravity.y);
        float baseAngle = 45f;
        float angle = Mathf.Clamp(baseAngle + (arcHeightMultiplier - 1f) * 15f, 35f, 65f);
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate required velocity to reach target (no speed boost - using prediction instead)
        float velocityMagnitude = Mathf.Sqrt(horizontalDistance * gravity / Mathf.Sin(2 * angleRad));
        
        // Height boost for arc
        float heightBoost = horizontalDistance * 0.15f * arcHeightMultiplier;
        
        // If calculation fails, use arc trajectory
        if (float.IsNaN(velocityMagnitude) || float.IsInfinity(velocityMagnitude))
        {
            Vector3 horizontalDir = new Vector3(toTarget.x, 0, toTarget.z).normalized;
            float upwardVelocity = speed * Mathf.Sin(angleRad) + heightBoost * 0.5f;
            float forwardVelocity = speed * Mathf.Cos(angleRad);
            velocity = horizontalDir * forwardVelocity + Vector3.up * upwardVelocity;
            return;
        }
        
        // Calculate velocity components
        Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z).normalized;
        float horizontalVelocity = velocityMagnitude * Mathf.Cos(angleRad);
        float verticalVelocity = velocityMagnitude * Mathf.Sin(angleRad) + heightBoost;
        
        velocity = horizontalDirection * horizontalVelocity + Vector3.up * verticalVelocity;
    }
    
    void CheckCollisions()
    {
        // CRITICAL: Don't check collisions until projectile has traveled minimum distance
        // This prevents hitting the turret that fired it
        if (travelDistance < MIN_TRAVEL_DISTANCE)
        {
            return;
        }
        
        // Use much larger collision detection for ballistic projectiles to ensure hits
        float checkRadius = behavior == ProjectileBehavior.Ballistic ? 1.0f : 0.3f; // Much larger for ballistic
        RaycastHit hit;
        
        // Use velocity direction for ballistic, direction for others
        Vector3 checkDirection = behavior == ProjectileBehavior.Ballistic ? velocity.normalized : direction;
        float checkDistance = behavior == ProjectileBehavior.Ballistic ? velocity.magnitude * Time.deltaTime : speed * Time.deltaTime;
        
        // For ballistic, also add extra forward distance to catch fast-moving projectiles
        if (behavior == ProjectileBehavior.Ballistic)
        {
            checkDistance *= 2f; // Look further ahead
        }
        
        // Primary check - SphereCast with collision mask
        if (Physics.SphereCast(transform.position, checkRadius, checkDirection, out hit, checkDistance, collisionMask))
        {
            HandleHit(hit);
            return;
        }
        
        // Additional OverlapSphere check for ballistic when close to ground
        if (behavior == ProjectileBehavior.Ballistic && transform.position.y < 5f) // Near ground level
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, collisionMask);
            if (hits.Length > 0)
            {
                foreach (var col in hits)
                {
                    if (col.transform == transform) continue;
                    
                    // Found something - create a raycast hit
                    RaycastHit fakeHit;
                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    if (Physics.Raycast(transform.position, dir, out fakeHit, checkRadius * 2f, collisionMask))
                    {
                        HandleHit(fakeHit);
                        return;
                    }
                }
            }
        }
    }
    
    void HandleInstantHit()
    {
        // Instant hit - raycast to target immediately
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = direction;
        float maxRange = 100f; // Max range for instant projectiles
        
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRange, collisionMask))
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
        
        // For Explosive and Ballistic projectiles, explode on ANY contact (terrain, enemies, walls)
        if (behavior == ProjectileBehavior.Explosive || behavior == ProjectileBehavior.Ballistic)
        {
            // Deal direct damage if it's a valid target
            if (CanDamageTarget(hitTransform) && !HasTargetBeenHit(hitTransform))
            {
                DealDamage(hitTransform, hit.point);
                hitTargets.Add(hitTransform);
            }
            
            // Create explosion at hit point regardless of what was hit
            if (explosionRadius > 0)
            {
                CreateExplosion(hit.point);
            }
            
            // Create hit effect
            CreateHitEffect(hit.point, hit.normal);
            
            hasHit = true;
            Destroy(gameObject, 0.1f);
            
            if (showDebugInfo)
            {
                Debug.Log($"[TurretProjectile] {behavior} hit {hitTransform.name}, creating explosion");
            }
            return;
        }
        
        // For other projectile types, check if we can damage this target
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
            // Create explosion if applicable (for non-explosive projectiles with explosion radius)
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
        
        // Find all targets in explosion radius (use collision mask to avoid hitting turrets)
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, collisionMask);
        
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