using UnityEngine;

public class Bow : BaseWeapon
{
    [Header("Bow Specific")]
    [Range(10f, 100f)] public float projectileSpeed = 30f;
    [Range(0f, 45f)] public float maxDrawAngle = 30f;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public bool useGravity = true;
    [Range(0f, 5f)] public float gravityMultiplier = 1f;
    
    [Header("Draw Mechanics")]
    public bool canChargeDraw = true;
    [Range(0.5f, 3f)] public float maxDrawTime = 2f;
    [Range(0.1f, 2f)] public float minDrawTime = 0.3f;
    public AnimationCurve drawPowerCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);
    
    [Header("Bow Audio")]
    public AudioClip drawSound;
    public AudioClip releaseSound;
    
    [Header("Bow Effects")]
    public GameObject drawEffect;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float trajectoryTimeStep = 0.1f;
    
    [Header("UI Feedback")]
    public bool showDrawPowerInConsole = false;
    
    private bool isDrawing = false;
    private float drawStartTime;
    private float currentDrawPower = 0f;
    private Camera playerCamera;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set default values for bow
        if (weaponName == "Base Weapon") weaponName = "Bow";
        if (attackAnimations.Length == 0) 
        {
            attackAnimations = new string[] { "Bow Draw", "Bow Release" };
        }
        
        // Bows typically have longer range but different mechanics
        attackDistance = Mathf.Max(attackDistance, 20f);
        attackSpeed = Mathf.Max(attackSpeed, 1.5f); // Time between shots
        
        // Initialize trajectory line if not assigned
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
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
    }
    
    void Update()
    {
        if (isDrawing)
        {
            UpdateDraw();
            UpdateTrajectoryPreview();
            
            // Check for mouse button release to fire arrow
            CheckForRelease();
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
        
        // This method is called when attack input is pressed
        // For bow, we start drawing immediately
        if (!isDrawing)
        {
            StartDraw();
        }
    }
    
    void CheckForRelease()
    {
        // Check if left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseBow();
        }
        // Also check if we've been drawing for too long without input
        else if (Time.time - drawStartTime > maxDrawTime + 1f)
        {
            ReleaseBow();
        }
    }
    
    void StartDraw()
    {
        if (!canChargeDraw)
        {
            // Instant shot for non-charge bow
            FireArrow(1f);
            return;
        }
        
        isDrawing = true;
        drawStartTime = Time.time;
        readyToAttack = false;
        attacking = true;
        OnAttackStateChange?.Invoke(true);
        
        // Play draw sound
        if (audioSource != null && drawSound != null)
        {
            audioSource.PlayOneShot(drawSound);
        }
        
        // Trigger draw animation
        if (attackAnimations.Length > 0)
        {
            OnAnimationChange?.Invoke(attackAnimations[0]); // Draw animation
        }
        
        // Enable trajectory preview
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }
        
        // Enable draw effect
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
        
        // Optional debug output
        if (showDrawPowerInConsole)
        {
            Debug.Log($"Draw Power: {currentDrawPower:F2} | Draw Time: {drawTime:F2}s");
        }
        
        // Auto-release if held too long
        if (drawTime >= maxDrawTime)
        {
            ReleaseBow();
        }
    }
    
    void ReleaseBow()
    {
        if (!isDrawing) return;
        
        float drawTime = Time.time - drawStartTime;
        float finalPower = drawTime >= minDrawTime ? currentDrawPower : 0.3f; // Minimum power for quick shots
        
        FireArrow(finalPower);
        
        isDrawing = false;
        currentDrawPower = 0f;
        
        // Disable trajectory preview
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
        
        // Disable draw effect
        if (drawEffect != null)
        {
            drawEffect.SetActive(false);
        }
        
        // Play release sound
        if (audioSource != null && releaseSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(releaseSound);
        }
        
        // Trigger release animation
        if (attackAnimations.Length > 1)
        {
            OnAnimationChange?.Invoke(attackAnimations[1]); // Release animation
        }
        
        // Schedule attack reset
        Invoke(nameof(ResetAttack), attackSpeed);
    }
    
    void FireArrow(float drawPower)
    {
        if (arrowPrefab == null || playerCamera == null) return;
        
        Vector3 spawnPosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        Vector3 shootDirection = playerCamera.transform.forward;
        
        // Calculate final speed based on draw power
        float finalSpeed = projectileSpeed * drawPower;
        
        // Calculate damage based on draw power
        int finalDamage = Mathf.RoundToInt(attackDamage * drawPower);
        
        // Spawn arrow facing the player's aim direction
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        
        // Setup arrow component
        if (arrow.TryGetComponent<Arrow>(out Arrow arrowComponent))
        {
            arrowComponent.SetDamage(finalDamage);
            arrowComponent.SetShooter(gameObject);
            // Note: Draw power will be handled by arrow mass and physics
        }
        
        // Setup simple physics - just forward velocity
        if (arrow.TryGetComponent<Rigidbody>(out Rigidbody arrowRb))
        {
            // Arrow flies straight in the aim direction
            arrowRb.linearVelocity = shootDirection * finalSpeed;
            arrowRb.useGravity = useGravity;
            if (useGravity)
            {
                arrowRb.linearDamping = 0.05f; // Light air resistance
            }
            
            // No rotation - arrow stays facing forward
            arrowRb.freezeRotation = true;
        }
    }
    
    void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || !isDrawing || playerCamera == null) return;
        
        Vector3 startPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        Vector3 velocity = playerCamera.transform.forward * (projectileSpeed * currentDrawPower);
        
        // Change trajectory color based on draw power
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
            
            // Stop trajectory if it hits something
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
        // Bow doesn't use raycast attacks - uses projectiles instead
        // This method is overridden to prevent the base raycast behavior
    }
    
    protected override void OnHit(RaycastHit hit)
    {
        // Bow hits are handled by the arrow projectile
        // This method is overridden to prevent base behavior
    }
    
    protected override void DealDamage(Actor target)
    {
        // Damage is dealt by the arrow projectile
        // This method is overridden to prevent base behavior
    }
    
    // Public methods for external control
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
}
