using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeapon : BaseWeapon
{
    [Header("Ranged Weapon Settings")]
    public WeaponData weaponData;
    
    [Header("Arrow Spawn Configuration")]
    public Transform arrowSpawnPoint;
    [Tooltip("REQUIRED: Point where arrows are spawned. Drag a Transform here to set the arrow spawn location.")]
    
    [Header("Optional Visual Overrides")]
    [SerializeField] private GameObject drawEffect;
    [SerializeField] private bool showDrawPowerInConsole = false;
    
    // Private fields - values come from WeaponData
    private float projectileSpeed = 30f;
    private float maxDrawAngle = 30f;
    private GameObject arrowPrefab;
    private bool useGravity = true;
    private float gravityMultiplier = 1f;
    private bool canChargeDraw = true;
    private float maxDrawTime = 2f;
    private float minDrawTime = 0.3f;
    private AnimationCurve drawPowerCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);
    private AudioClip drawSound;
    private AudioClip releaseSound;
    private LineRenderer trajectoryLine;
    private int trajectoryPoints = 30;
    private float trajectoryTimeStep = 0.1f;
    
    private bool isDrawing = false;
    private float drawStartTime;
    private float currentDrawPower = 0f;
    private Camera playerCamera;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Apply weapon data if available
        if (weaponData != null)
        {
            ApplyWeaponData(weaponData);
        }
        
        // Initialize trajectory line if not assigned
        SetupTrajectoryLine();
    }
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
            
        // Validate that arrow spawn point is assigned
        if (arrowSpawnPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] Arrow Spawn Point is not assigned! This weapon will not function properly.", this);
        }
    }
    
    void Update()
    {
        if (isDrawing)
        {
            UpdateDraw();
            UpdateTrajectoryPreview();
            CheckForRelease();
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
        
        // Apply ranged-specific settings
        arrowPrefab = data.arrowPrefab;
        projectileSpeed = data.projectileSpeed;
        maxDrawAngle = data.maxDrawAngle;
        maxDrawTime = data.maxDrawTime;
        minDrawTime = data.minDrawTime;
        useGravity = data.useGravity;
        gravityMultiplier = data.gravityMultiplier;
        canChargeDraw = data.canChargeDraw;
        drawSound = data.drawSound;
        releaseSound = data.releaseSound;
        
        // Apply spawn point from WeaponData
        if (!string.IsNullOrEmpty(data.arrowSpawnPointName))
        {
            Transform spawnPoint = transform.Find(data.arrowSpawnPointName);
            if (spawnPoint != null)
            {
                arrowSpawnPoint = spawnPoint;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Could not find arrow spawn point '{data.arrowSpawnPointName}' in weapon hierarchy.");
            }
        }
        
        // Set weapon-specific defaults
        attackDistance = Mathf.Max(attackDistance, 20f);
        attackSpeed = Mathf.Max(attackSpeed, 1.5f);
        
        // Note: Weapon positioning is handled by the pickup system using saved WeaponData positions
    }
    
    void SetupTrajectoryLine()
    {
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
            if (trajectoryLine == null)
            {
                GameObject lineObj = new GameObject("TrajectoryLine");
                lineObj.transform.SetParent(transform);
                trajectoryLine = lineObj.AddComponent<LineRenderer>();
                trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
                trajectoryLine.startColor = Color.yellow;
                trajectoryLine.endColor = Color.yellow;
                trajectoryLine.startWidth = 0.02f;
                trajectoryLine.endWidth = 0.02f;
                trajectoryLine.positionCount = trajectoryPoints;
                trajectoryLine.enabled = false;
            }
        }
    }
    
    public override bool CanAttack()
    {
        return base.CanAttack() && arrowPrefab != null;
    }
    
    public override void Attack(Camera camera)
    {
        if (!CanAttack()) return;
        
        playerCamera = camera;
        
        if (!isDrawing)
        {
            StartDraw();
        }
    }
    
    void CheckForRelease()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ReleaseBow();
        }
        
        if (Time.time - drawStartTime > maxDrawTime + 1f)
        {
            ReleaseBow();
        }
    }
    
    void StartDraw()
    {
        if (!canChargeDraw)
        {
            FireArrow(1f);
            return;
        }
        
        isDrawing = true;
        drawStartTime = Time.time;
        readyToAttack = false;
        attacking = true;
        OnAttackStateChange?.Invoke(true);
        
        if (audioSource != null && drawSound != null)
        {
            audioSource.PlayOneShot(drawSound);
        }
        
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            OnAnimationChange?.Invoke(attackAnimations[0]);
        }
        
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }
        
        if (drawEffect != null)
        {
            drawEffect.SetActive(true);
        }
    }
    
    void UpdateDraw()
    {
        float drawTime = Time.time - drawStartTime;
        float normalizedDrawTime = Mathf.Clamp01(drawTime / maxDrawTime);
        currentDrawPower = drawPowerCurve.Evaluate(normalizedDrawTime);
        
        if (showDrawPowerInConsole)
        {
            Debug.Log($"Draw Power: {currentDrawPower:F2} | Draw Time: {drawTime:F2}s");
        }
        
        if (drawTime >= maxDrawTime)
        {
            ReleaseBow();
        }
    }
    
    void ReleaseBow()
    {
        if (!isDrawing) return;
        
        float drawTime = Time.time - drawStartTime;
        float finalPower = drawTime >= minDrawTime ? currentDrawPower : 0.3f;
        
        FireArrow(finalPower);
        
        isDrawing = false;
        currentDrawPower = 0f;
        
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
        
        if (drawEffect != null)
        {
            drawEffect.SetActive(false);
        }
        
        if (audioSource != null && releaseSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(releaseSound);
        }
        
        if (attackAnimations != null && attackAnimations.Length > 1)
        {
            OnAnimationChange?.Invoke(attackAnimations[1]);
        }
        
        Invoke(nameof(ResetAttack), attackSpeed);
    }
    
    void FireArrow(float drawPower)
    {
        if (arrowPrefab == null || playerCamera == null) return;
        
        Vector3 spawnPosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        Vector3 shootDirection = playerCamera.transform.forward;
        
        spawnPosition += shootDirection * 0.5f;
        
        float finalSpeed = projectileSpeed * drawPower;
        int finalDamage = Mathf.RoundToInt(attackDamage * drawPower);
        
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        
        if (arrow.TryGetComponent<Arrow>(out Arrow arrowComponent))
        {
            arrowComponent.SetDamage(finalDamage);
            arrowComponent.SetShooter(gameObject);
            arrowComponent.SetDamageableTags(damageableTags);
        }
        
        if (arrow.TryGetComponent<Rigidbody>(out Rigidbody arrowRb))
        {
            arrowRb.linearVelocity = Vector3.zero;
            arrowRb.useGravity = useGravity;
            if (useGravity)
            {
                arrowRb.linearDamping = 0.05f;
            }
            arrowRb.freezeRotation = true;
            StartCoroutine(ApplyArrowVelocity(arrowRb, shootDirection * finalSpeed));
        }
    }
    
    void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || !isDrawing || playerCamera == null) return;
        
        Vector3 startPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        Vector3 velocity = playerCamera.transform.forward * (projectileSpeed * currentDrawPower);
        
        Color trajectoryColor = Color.Lerp(Color.red, Color.green, currentDrawPower);
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;
        
        trajectoryLine.positionCount = trajectoryPoints;
        
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 point = startPos + velocity * time;
            
            if (useGravity)
            {
                point.y += 0.5f * Physics.gravity.y * gravityMultiplier * time * time;
            }
            
            trajectoryLine.SetPosition(i, point);
            
            if (Physics.Raycast(startPos, (point - startPos).normalized, out RaycastHit hit, Vector3.Distance(startPos, point), attackLayer))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                break;
            }
        }
    }
    
    protected override void PerformAttackRaycast()
    {
        // Ranged weapons don't use raycast attacks
    }
    
    protected override void OnHit(RaycastHit hit)
    {
        // Hits are handled by projectiles
    }
    
    protected override void DealDamage(Actor target)
    {
        // Damage is dealt by projectiles
    }
    
    public void ForceRelease()
    {
        if (isDrawing)
        {
            ReleaseBow();
        }
    }
    
    public float GetCurrentDrawPower()
    {
        return currentDrawPower;
    }
    
    public bool IsDrawing()
    {
        return isDrawing;
    }
    
    private System.Collections.IEnumerator ApplyArrowVelocity(Rigidbody arrowRb, Vector3 velocity)
    {
        yield return new WaitForFixedUpdate();
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = velocity;
        }
    }
    
    // Editor validation
    void OnValidate()
    {
        // Provide warning if arrow spawn point is not assigned
        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Arrow Spawn Point is not assigned! Drag a Transform to the Arrow Spawn Point field.", this);
        }
    }
    
    // Debug visualization for arrow spawn point
    void OnDrawGizmosSelected()
    {
        if (arrowSpawnPoint != null)
        {
            // Draw arrow spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(arrowSpawnPoint.position, 0.1f);
            Gizmos.DrawRay(arrowSpawnPoint.position, arrowSpawnPoint.forward * 0.5f);
            
            // Draw spawn direction indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(arrowSpawnPoint.position, arrowSpawnPoint.position + arrowSpawnPoint.forward * 1f);
        }
        else
        {
            // Show fallback position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }
        
        // Show trajectory preview when drawing
        if (isDrawing && trajectoryLine != null && trajectoryLine.enabled)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < trajectoryLine.positionCount - 1; i++)
            {
                Gizmos.DrawLine(trajectoryLine.GetPosition(i), trajectoryLine.GetPosition(i + 1));
            }
        }
    }
}