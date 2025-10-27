using UnityEngine;

public class Staff : BaseWeapon
{
    [Header("Staff Specific")]
    [Range(1f, 50f)] public float projectileSpeed = 15f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    
    [Header("AoE Settings")]
    [Range(1f, 20f)] public float explosionRadius = 5f;
    [Range(1, 100)] public int explosionDamage = 25;
    public LayerMask explosionLayer;
    
    [Header("Staff Effects")]
    public GameObject explosionEffect;
    public float explosionEffectDuration = 5f;
    public AudioClip castSound;
    public AudioClip explosionSound;
    
    private Camera playerCamera;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set default values for staff
        if (weaponName == "Base Weapon") weaponName = "Staff";
        if (attackAnimations.Length == 0) 
        {
            attackAnimations = new string[] { "Staff Cast" };
        }
        
        // Staffs typically have different mechanics than melee weapons
        attackDistance = Mathf.Max(attackDistance, 30f); // Longer range for projectiles
        attackSpeed = Mathf.Max(attackSpeed, 1.0f); // Casting time
    }
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
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
        if (attackAnimations.Length > 0)
        {
            OnAnimationChange?.Invoke(attackAnimations[0]);
        }
        
        // Fire projectile immediately (no delay like bow)
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
            projectileComponent.SetStaff(this);
            projectileComponent.SetExplosionData(explosionRadius, explosionDamage, explosionLayer);
            projectileComponent.SetEffects(explosionEffect, explosionEffectDuration, explosionSound);
            projectileComponent.SetLifetime(1.5f); // Set projectile lifetime to 1.5 seconds
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
            // Calculate distance for damage falloff (optional)
            float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius); // Linear falloff
            damageMultiplier = Mathf.Clamp01(damageMultiplier);
            
            // Deal damage to actors
            if (hitCollider.TryGetComponent<Actor>(out Actor actor))
            {
                int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                actor.TakeDamage(finalDamage);
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
    
    // Override base weapon methods since staff uses projectiles
    protected override void PerformAttackRaycast()
    {
        // Staff doesn't use raycast attacks - uses projectiles instead
    }
    
    protected override void OnHit(RaycastHit hit)
    {
        // Staff hits are handled by projectile explosions
    }
    
    protected override void DealDamage(Actor target)
    {
        // Damage is dealt by projectile explosions
    }
    
    // Debug method to visualize explosion radius
    void OnDrawGizmosSelected()
    {
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, explosionRadius);
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
}