using UnityEngine;

public class MagicWeapon : BaseWeapon
{
    [Header("Magic Weapon Settings")]
    public WeaponData weaponData;
    
    [Header("Projectile Spawn Configuration")]
    public Transform projectileSpawnPoint;
    [Tooltip("REQUIRED: Point where magic projectiles are spawned. Drag a Transform here to set the projectile spawn location.")]
    
    // Private fields - values come from WeaponData
    private float projectileSpeed = 15f;
    private GameObject projectilePrefab;
    private float explosionRadius = 5f;
    private int explosionDamage = 25;
    private LayerMask explosionLayer;
    private GameObject explosionEffect;
    private float explosionEffectDuration = 5f;
    private AudioClip castSound;
    private AudioClip explosionSound;
    private float projectileLifetime = 3f;
    
    private Camera playerCamera;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Apply weapon data if available
        if (weaponData != null)
        {
            ApplyWeaponData(weaponData);
        }
    }
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
            
        // Validate that projectile spawn point is assigned
        if (projectileSpawnPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] Projectile Spawn Point is not assigned! This weapon will not function properly.", this);
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
        
        // Apply magic-specific settings
        projectilePrefab = data.projectilePrefab;
        projectileSpeed = data.staffProjectileSpeed;
        explosionRadius = data.explosionRadius;
        explosionDamage = data.explosionDamage;
        explosionLayer = data.explosionLayer;
        explosionEffect = data.explosionEffect;
        explosionEffectDuration = data.explosionEffectDuration;
        castSound = data.castSound;
        explosionSound = data.explosionSound;
        projectileLifetime = data.projectileLifetime;
        
        // Apply spawn point from WeaponData
        if (!string.IsNullOrEmpty(data.projectileSpawnPointName))
        {
            Transform spawnPoint = transform.Find(data.projectileSpawnPointName);
            if (spawnPoint != null)
            {
                projectileSpawnPoint = spawnPoint;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Could not find projectile spawn point '{data.projectileSpawnPointName}' in weapon hierarchy.");
            }
        }
        
        // Set magic weapon defaults
        attackDistance = Mathf.Max(attackDistance, 30f);
        attackSpeed = Mathf.Max(attackSpeed, 1.0f);
        
        // Note: Weapon positioning is handled by the pickup system using saved WeaponData positions
    }
    
    public override bool CanAttack()
    {
        return base.CanAttack() && projectilePrefab != null;
    }
    
    public override void Attack(Camera camera)
    {
        if (!CanAttack()) return;
        
        playerCamera = camera;
        
        readyToAttack = false;
        attacking = true;
        OnAttackStateChange?.Invoke(true);
        
        // Play cast sound
        if (audioSource != null && castSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(castSound);
        }
        
        // Trigger cast animation
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            OnAnimationChange?.Invoke(attackAnimations[0]);
        }
        
        // Fire projectile immediately
        FireProjectile();
        
        // Schedule attack reset
        Invoke(nameof(ResetAttack), attackSpeed);
    }
    
    void FireProjectile()
    {
        if (projectilePrefab == null || playerCamera == null) return;
        
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        Vector3 shootDirection = playerCamera.transform.forward;
        
        // Move spawn position slightly forward to avoid collision with player
        spawnPosition += shootDirection * 0.3f;
        
        // Instantiate projectile facing the shooting direction
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        
        // Setup projectile component
        if (projectile.TryGetComponent<StaffProjectile>(out StaffProjectile projectileComponent))
        {
            projectileComponent.SetMagicWeapon(this);
            projectileComponent.SetExplosionData(explosionRadius, explosionDamage, explosionLayer);
            projectileComponent.SetEffects(explosionEffect, explosionEffectDuration, explosionSound);
            projectileComponent.SetLifetime(projectileLifetime); // Set projectile lifetime from WeaponData
        }
        
        // Setup projectile physics
        if (projectile.TryGetComponent<Rigidbody>(out Rigidbody projectileRb))
        {
            // Ensure rigidbody is not kinematic for physics-based movement
            projectileRb.isKinematic = false;
            projectileRb.useGravity = false;
            projectileRb.freezeRotation = true;
            
            // Apply velocity on next frame to prevent spawn collision issues
            StartCoroutine(ApplyProjectileVelocity(projectileRb, shootDirection * projectileSpeed));
        }
    }
    
    // Coroutine to apply projectile velocity on next frame
    private System.Collections.IEnumerator ApplyProjectileVelocity(Rigidbody projectileRb, Vector3 velocity)
    {
        yield return new WaitForFixedUpdate();
        if (projectileRb != null && !projectileRb.isKinematic)
        {
            projectileRb.linearVelocity = velocity;
        }
    }
    
    // Called by projectile when it explodes
    public void OnProjectileExplode(Vector3 explosionPosition)
    {
        // Draw debug visualization of explosion
        DrawExplosionGizmo(explosionPosition);
        
        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRadius, explosionLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            // Check if target can be damaged based on tags
            if (!CanDamageTarget(hitCollider.gameObject))
                continue;
            
            // Calculate distance for damage falloff (optional)
            float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius); // Linear falloff
            damageMultiplier = Mathf.Clamp01(damageMultiplier);
            
            int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
            
            // Deal damage to actors (legacy system)
            if (hitCollider.TryGetComponent<Actor>(out Actor actor))
            {
                actor.TakeDamage(finalDamage);
            }
            // Deal damage to Health component (new system)
            else if (hitCollider.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(finalDamage);
            }
        }
        
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
            Destroy(effect, explosionEffectDuration);
        }
        
        // Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(explosionSound);
        }
    }
    
    // Debug method to visualize explosion at runtime (called by projectile)
    public void DrawExplosionGizmo(Vector3 explosionPosition)
    {
        if (Application.isPlaying)
        {
            // Draw a wire sphere to visualize explosion radius
            StartCoroutine(DrawDebugSphere(explosionPosition, explosionRadius, Color.red, 2f));
        }
    }
    
    // Coroutine to draw debug sphere over time
    private System.Collections.IEnumerator DrawDebugSphere(Vector3 center, float radius, Color color, float duration)
    {
        float timer = 0f;
        int segments = 32;
        
        while (timer < duration)
        {
            // Draw horizontal circle
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
                
                Debug.DrawLine(point1, point2, color, Time.deltaTime);
            }
            
            // Draw vertical circle (XY plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);
                
                Debug.DrawLine(point1, point2, color, Time.deltaTime);
            }
            
            // Draw vertical circle (YZ plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                
                Vector3 point1 = center + new Vector3(0, Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(0, Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius);
                
                Debug.DrawLine(point1, point2, color, Time.deltaTime);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    // Override base weapon methods since magic weapons use projectiles
    protected override void PerformAttackRaycast()
    {
        // Magic weapons don't use raycast attacks - uses projectiles instead
    }
    
    protected override void OnHit(RaycastHit hit)
    {
        // Magic weapon hits are handled by projectile explosions
    }
    
    protected override void DealDamage(Actor target)
    {
        // Damage is dealt by projectile explosions
    }
    
    // Editor validation
    void OnValidate()
    {
        // Provide warning if projectile spawn point is not assigned
        if (projectileSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Projectile Spawn Point is not assigned! Drag a Transform to the Projectile Spawn Point field.", this);
        }
    }
    
    // Debug visualization for projectile spawn point and explosion radius
    void OnDrawGizmosSelected()
    {
        if (projectileSpawnPoint != null)
        {
            // Draw projectile spawn point
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
            Gizmos.DrawRay(projectileSpawnPoint.position, projectileSpawnPoint.forward * 0.5f);
            
            // Draw spawn direction indicator
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(projectileSpawnPoint.position, projectileSpawnPoint.position + projectileSpawnPoint.forward * 2f);
            
            // Draw explosion radius preview
            Gizmos.color = Color.red;
            Vector3 previewPos = projectileSpawnPoint.position + projectileSpawnPoint.forward * 5f; // Show explosion at sample distance
            Gizmos.DrawWireSphere(previewPos, explosionRadius);
            
            // Draw trajectory line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(projectileSpawnPoint.position, previewPos);
        }
        else
        {
            // Show fallback position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
            
            // Show explosion radius at fallback position
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Vector3 previewPos = transform.position + transform.forward * 5f;
            Gizmos.DrawWireSphere(previewPos, explosionRadius);
        }
    }
}