    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public partial class FirstPersonController : MonoBehaviour
    {
        // Input System
        PlayerInput playerInput;
        PlayerInput.MainActions input;
        
        [Header("Mouse Settings")]
        [Range(0, 100)] public float mouseHorizontalSensitivity = 50f;
        [Range(0, 100)] public float mouseVerticalSensitivity = 50f;
        [Range(0f, 200f)] private float snappiness = 100f;
        [Range(0f, 20f)] public float walkSpeed = 3f;
        [Range(0f, 30f)] public float sprintSpeed = 5f;
        [Range(0f, 10f)] public float crouchSpeed = 1.5f;
        public float crouchHeight = 1f;
        public float crouchCameraHeight = 1f;
        public float slideSpeed = 8f;
        public float slideDuration = 0.7f;
        public float slideFovBoost = 5f;
        public float slideTiltAngle = 5f;
        [Range(0f, 15f)] public float jumpSpeed = 3f;
        [Range(0f, 50f)] public float gravity = 9.81f;
        public bool coyoteTimeEnabled = true;
        [Range(0.01f, 0.3f)] public float coyoteTimeDuration = 0.2f;
        public float normalFov = 60f;
        public float sprintFov = 70f;
        public float fovChangeSpeed = 5f;
        public float walkingBobbingSpeed = 10f;
        public float bobbingAmount = 0.05f;
        private float sprintBobMultiplier = 1.5f;
        private float recoilReturnSpeed = 8f;
        public bool canSlide = true;
        public bool canJump = true;
        public bool canSprint = true;
        public bool canCrouch = true;
        public QueryTriggerInteraction ceilingCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
        public QueryTriggerInteraction groundCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
        public Transform groundCheck;
        public float groundDistance = 0.2f;
        public LayerMask groundMask;
        public Transform playerCamera;
        public Transform cameraParent;
        private float rotX, rotY;
        private float xVelocity, yVelocity;
        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private bool isGrounded;
        private Vector2 moveInput;
        public bool isSprinting;
        public bool isCrouching;
        public bool isSliding;
        private float slideTimer;
        private float postSlideCrouchTimer;
        private Vector3 slideDirection;
        private float originalHeight;
        private float originalCameraParentHeight;
        private float coyoteTimer;
        private Camera cam;
        private AudioSource slideAudioSource;
        private float bobTimer;
        private float defaultPosY;
        private Vector3 recoil = Vector3.zero;
        private bool isLook = true, isMove = true;
        private float currentCameraHeight;
        private float currentBobOffset;
        private float currentFov;
        private float fovVelocity;
        private float currentSlideSpeed;
        private float slideSpeedVelocity;
        private float currentTiltAngle;
        private float tiltVelocity;

        // Animation System
        private Animator animator;
        private AudioSource audioSource;
        
        // Health System
        private Health healthComponent;
        
        // Stamina System
        private Stamina staminaComponent;
        
        // Animation States
        public const string IDLE = "Idle";
        public const string WALK = "Walk";
        public const string ATTACK1 = "Attack 1";
        public const string ATTACK2 = "Attack 2";
        private string currentAnimationState;
        
        // Weapon System
        [Header("Weapon System")]
        public BaseWeapon currentWeapon;
        [SerializeField] public BaseWeapon[] availableWeapons;
        [SerializeField] public int selectedWeaponIndex = 0;
        
        [Header("Weapon Selection (Editor)")]
        [SerializeField] private string[] weaponNames = new string[0];
        [SerializeField] private bool autoFindWeapons = true;
        
        private bool attacking = false;

        public float CurrentCameraHeight => isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

        private void OnValidate()
        {
            if (autoFindWeapons)
                UpdateWeaponList();
        }
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cam = playerCamera.GetComponent<Camera>();
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponent<AudioSource>();
            healthComponent = GetComponent<Health>();
            staminaComponent = GetComponent<Stamina>();
            
            originalHeight = characterController.height;
            originalCameraParentHeight = cameraParent.localPosition.y;
            defaultPosY = cameraParent.localPosition.y;
            slideAudioSource = gameObject.AddComponent<AudioSource>();
            slideAudioSource.playOnAwake = false;
            slideAudioSource.loop = false;
            
            // Initialize Input System
            playerInput = new PlayerInput();
            input = playerInput.Main;
            AssignInputs();
            
            // Initialize Weapon System
            InitializeWeaponSystem();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            currentCameraHeight = originalCameraParentHeight;
            currentBobOffset = 0f;
            currentFov = normalFov;
            currentSlideSpeed = 0f;
            currentTiltAngle = 0f;

            rotX = transform.rotation.eulerAngles.y;
            rotY = playerCamera.localRotation.eulerAngles.x;
            xVelocity = rotX;
            yVelocity = rotY;
        }

        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, groundCheckQueryTriggerInteraction);
            if (isGrounded && moveDirection.y < 0)
            {
                moveDirection.y = -2f;
                coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
            }
            else if (coyoteTimeEnabled)
            {
                coyoteTimer -= Time.deltaTime;
            }

            if (isLook)
            {
                Vector2 lookInput = input.Look.ReadValue<Vector2>();
                float mouseX = lookInput.x * mouseHorizontalSensitivity * Time.deltaTime;
                float mouseY = lookInput.y * mouseVerticalSensitivity * Time.deltaTime;

                rotX += mouseX;
                rotY -= mouseY;
                rotY = Mathf.Clamp(rotY, -90f, 90f);

                xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
                yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

                float targetTiltAngle = isSliding ? slideTiltAngle : 0f;
                currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTiltAngle, ref tiltVelocity, 0.2f);
                playerCamera.transform.localRotation = Quaternion.Euler(yVelocity - currentTiltAngle, 0f, 0f);
                transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
            }

            HandleHeadBob();

            // Handle weapon-specific input logic
            HandleWeaponInput();

            bool wantsToCrouch = canCrouch && input.Crouch.IsPressed() && !isSliding;
            Vector3 point1 = transform.position + characterController.center - Vector3.up * (characterController.height * 0.5f);
            Vector3 point2 = point1 + Vector3.up * characterController.height * 0.6f;
            float capsuleRadius = characterController.radius * 0.95f;
            float castDistance = isSliding ? originalHeight + 0.2f : originalHeight - crouchHeight + 0.2f;
            bool hasCeiling = Physics.CapsuleCast(point1, point2, capsuleRadius, Vector3.up, castDistance, groundMask, ceilingCheckQueryTriggerInteraction);
            if (isSliding)
            {
                postSlideCrouchTimer = 0.3f;
            }
            if (postSlideCrouchTimer > 0)
            {
                postSlideCrouchTimer -= Time.deltaTime;
                isCrouching = canCrouch;
            }
            else
            {
                isCrouching = canCrouch && (wantsToCrouch || (hasCeiling && !isSliding));
            }

            if (canSlide && isSprinting && input.Crouch.WasPressedThisFrame() && isGrounded)
            {
                isSliding = true;
                slideTimer = slideDuration;
                slideDirection = moveInput.magnitude > 0.1f ? (transform.right * moveInput.x + transform.forward * moveInput.y).normalized : transform.forward;
                currentSlideSpeed = sprintSpeed;
            }

            float slideProgress = slideTimer / slideDuration;
            if (isSliding)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f || !isGrounded)
                {
                    isSliding = false;
                }
                float targetSlideSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideProgress);
                currentSlideSpeed = Mathf.SmoothDamp(currentSlideSpeed, targetSlideSpeed, ref slideSpeedVelocity, 0.2f);
                characterController.Move(slideDirection * currentSlideSpeed * Time.deltaTime);
            }

            float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * 10f);
            characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);

            float targetFov = isSprinting ? sprintFov : (isSliding ? sprintFov + (slideFovBoost * Mathf.Lerp(0f, 1f, 1f - slideProgress)) : normalFov);
            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
            cam.fieldOfView = currentFov;

            HandleMovement();
            SetAnimations();
        }

        private void HandleHeadBob()
        {
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            bool isMovingEnough = horizontalVelocity.magnitude > 0.1f;

            float targetBobOffset = isMovingEnough ? Mathf.Sin(bobTimer) * bobbingAmount : 0f;
            currentBobOffset = Mathf.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * walkingBobbingSpeed);

            if (!isGrounded || isSliding || isCrouching)
            {
                bobTimer = 0f;
                float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
                currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
                cameraParent.localPosition = new Vector3(
                    cameraParent.localPosition.x,
                    currentCameraHeight + currentBobOffset,
                    cameraParent.localPosition.z);
                recoil = Vector3.zero;
                cameraParent.localRotation = Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil), recoilReturnSpeed * Time.deltaTime);
                return;
            }

            if (isMovingEnough)
            {
                float bobSpeed = walkingBobbingSpeed * (isSprinting ? sprintBobMultiplier : 1f);
                bobTimer += Time.deltaTime * bobSpeed;
                float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
                currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
                cameraParent.localPosition = new Vector3(
                    cameraParent.localPosition.x,
                    currentCameraHeight + currentBobOffset,
                    cameraParent.localPosition.z);
                recoil.z = moveInput.x * -2f;
            }
            else
            {
                bobTimer = 0f;
                float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
                currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
                cameraParent.localPosition = new Vector3(
                    cameraParent.localPosition.x,
                    currentCameraHeight + currentBobOffset,
                    cameraParent.localPosition.z);
                recoil = Vector3.zero;
            }

            cameraParent.localRotation = Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil), recoilReturnSpeed * Time.deltaTime);
        }

        private void HandleMovement()
        {
            Vector2 movementInput = input.Movement.ReadValue<Vector2>();
            moveInput.x = movementInput.x;
            moveInput.y = movementInput.y;
            // Check stamina availability for sprinting
            bool hasStaminaForSprint = staminaComponent == null || staminaComponent.CanPerformAction;
            isSprinting = canSprint && input.Sprint.IsPressed() && moveInput.y > 0.1f && isGrounded && !isCrouching && !isSliding && hasStaminaForSprint;
            
            // Consume stamina while sprinting
            if (isSprinting && staminaComponent != null)
            {
                var staminaStats = staminaComponent.GetStaminaStats();
                if (!staminaComponent.ConsumeStaminaOverTime(staminaStats.sprintCostPerSecond))
                {
                    // Stop sprinting if stamina is exhausted
                    isSprinting = false;
                }
            }

            float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
            if (!isMove) currentSpeed = 0f;

            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
            moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

            if (isGrounded || coyoteTimer > 0f)
            {
                // Jump will be handled by AssignInputs method through input actions
                if (moveDirection.y < 0)
                {
                    moveDirection.y = -2f;
                }
            }
            else
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            if (!isSliding)
            {
                moveDirection = new Vector3(moveVector.x, moveDirection.y, moveVector.z);
                characterController.Move(moveDirection * Time.deltaTime);
            }
        }

        public void SetControl(bool newState)
        {
            SetLookControl(newState);
            SetMoveControl(newState);
        }

        public void SetLookControl(bool newState)
        {
            isLook = newState;
        }

        public void SetMoveControl(bool newState)
        {
            isMove = newState;
        }

        public void SetCursorVisibility(bool newVisibility)
        {
            Cursor.lockState = newVisibility ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = newVisibility;
        }

        // Input System Methods
        void OnEnable()
        {
            input.Enable();
        }

        void OnDisable()
        {
            input.Disable();
        }

        void Jump()
        {
            if ((isGrounded || coyoteTimer > 0f) && canJump && !isSliding)
            {
                // Check stamina and consume it for jumping
                if (staminaComponent != null)
                {
                    var staminaStats = staminaComponent.GetStaminaStats();
                    if (!staminaComponent.ConsumeStamina(staminaStats.jumpCost))
                    {
                        return; // Not enough stamina to jump
                    }
                }
                
                moveDirection.y = jumpSpeed;
            }
        }

        void AssignInputs()
        {
            input.Jump.performed += ctx => Jump();
            // Remove the Attack.started event as we'll handle it in HandleWeaponInput()
            // input.Attack.started += ctx => Attack();
        }
        
        void HandleWeaponInput()
        {
            if (currentWeapon == null) return;
            
            // Special handling for ranged weapons (needs continuous input for drawing)
            if (currentWeapon is RangedWeapon rangedWeapon)
            {
                // Start drawing when attack button is first pressed
                if (input.Attack.WasPressedThisFrame())
                {
                    Attack();
                }
                // Pass input state to ranged weapon for release detection
                if (input.Attack.WasReleasedThisFrame() && rangedWeapon.IsDrawing())
                {
                    rangedWeapon.ForceRelease();
                }
            }
            else
            {
                // For all other weapons, only respond to single clicks
                if (input.Attack.WasPressedThisFrame())
                {
                    Attack();
                }
            }
        }
        
        // Public method to check input state for weapons
        public bool IsAttackButtonPressed()
        {
            return input.Attack.IsPressed();
        }
        
        public bool WasAttackButtonReleasedThisFrame()
        {
            return input.Attack.WasReleasedThisFrame();
        }

        // Animation System
        public void ChangeAnimationState(string newState)
        {
            if (currentAnimationState == newState) return;
            if (animator == null) return;
            
            // Check if the animation state exists before trying to play it
            if (HasAnimationState(newState))
            {
                currentAnimationState = newState;
                animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
            }
        }
        
        private bool HasAnimationState(string stateName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            
            // Check if state exists in any layer
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.HasState(i, Animator.StringToHash(stateName)))
                {
                    return true;
                }
            }
            return false;
        }

        void SetAnimations()
        {
            if (animator == null) return;
            
            // Check if we're using a ranged weapon and drawing
            bool isRangedDrawing = currentWeapon != null && 
                                 (currentWeapon is RangedWeapon rangedWeapon && rangedWeapon.IsDrawing());
            
            // Don't override animations when attacking with non-ranged weapons
            // But allow movement animations when drawing a ranged weapon
            if (!attacking || isRangedDrawing)
            {
                Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
                if (horizontalVelocity.magnitude < 0.1f)
                {
                    ChangeAnimationState(IDLE);
                }
                else
                {
                    ChangeAnimationState(WALK);
                }
            }
        }

        // Weapon System Methods
        void InitializeWeaponSystem()
        {
            // Find all available weapons in WeaponSlot (or fallback to all children)
            GameObject weaponSlot = FindWeaponSlot();
            if (weaponSlot != null)
            {
                availableWeapons = weaponSlot.GetComponentsInChildren<BaseWeapon>();
                Debug.Log($"Found {availableWeapons.Length} weapons in WeaponSlot");
            }
            else
            {
                // Fallback: Find all available weapons in player children
                availableWeapons = GetComponentsInChildren<BaseWeapon>();
                Debug.LogWarning("WeaponSlot not found, searching in all player children");
            }
            
            // Set current weapon
            if (availableWeapons.Length > 0 && selectedWeaponIndex < availableWeapons.Length)
            {
                SetCurrentWeapon(selectedWeaponIndex);
            }
        }
        
        public void SetCurrentWeapon(int weaponIndex)
        {
            if (availableWeapons == null || weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
                return;
                
            // Disable previous weapon
            if (currentWeapon != null)
            {
                currentWeapon.OnAnimationChange -= ChangeAnimationState;
                currentWeapon.OnAttackStateChange -= SetAttackingState;
                currentWeapon.gameObject.SetActive(false);
            }
            
            // Enable new weapon
            selectedWeaponIndex = weaponIndex;
            currentWeapon = availableWeapons[weaponIndex];
            currentWeapon.gameObject.SetActive(true);
            
            // Subscribe to weapon events
            currentWeapon.OnAnimationChange += ChangeAnimationState;
            currentWeapon.OnAttackStateChange += SetAttackingState;
        }
        
        public void Attack()
        {
            if (currentWeapon != null)
            {
                currentWeapon.Attack(playerCamera.GetComponent<Camera>());
            }
        }
        
        void SetAttackingState(bool isAttacking)
        {
            attacking = isAttacking;
        }
        
        // Weapon selection methods for editor/runtime use
        public void NextWeapon()
        {
            if (availableWeapons.Length > 0)
            {
                int nextIndex = (selectedWeaponIndex + 1) % availableWeapons.Length;
                SetCurrentWeapon(nextIndex);
            }
        }
        
        public void PreviousWeapon()
        {
            if (availableWeapons.Length > 0)
            {
                int prevIndex = selectedWeaponIndex - 1;
                if (prevIndex < 0) prevIndex = availableWeapons.Length - 1;
                SetCurrentWeapon(prevIndex);
            }
        }
        
        public string GetCurrentWeaponName()
        {
            return currentWeapon != null ? currentWeapon.WeaponName : "No Weapon";
        }
        
        // Method for weapon pickups to replace current weapon
        public void ReplaceCurrentWeapon(BaseWeapon newWeapon)
        {
            if (newWeapon == null) return;
            
            // Get current weapon index to maintain position in array
            int currentIndex = selectedWeaponIndex;
            
            // Disable and unsubscribe from current weapon
            if (currentWeapon != null)
            {
                currentWeapon.OnAnimationChange -= ChangeAnimationState;
                currentWeapon.OnAttackStateChange -= SetAttackingState;
                currentWeapon.gameObject.SetActive(false);
            }
            
            // Update the weapon array
            if (availableWeapons != null && currentIndex < availableWeapons.Length)
            {
                // Destroy old weapon if it exists (use Destroy instead of DestroyImmediate)
                if (availableWeapons[currentIndex] != null)
                {
                    Destroy(availableWeapons[currentIndex].gameObject);
                }
                
                availableWeapons[currentIndex] = newWeapon;
            }
            else
            {
                // Expand array if needed
                System.Array.Resize(ref availableWeapons, currentIndex + 1);
                availableWeapons[currentIndex] = newWeapon;
            }
            
            // Set the new weapon as current
            currentWeapon = newWeapon;
            currentWeapon.gameObject.SetActive(true);
            
            // Subscribe to new weapon events
            currentWeapon.OnAnimationChange += ChangeAnimationState;
            currentWeapon.OnAttackStateChange += SetAttackingState;
            
            // Update weapon names array for editor display
            UpdateWeaponList();
            
            Debug.Log($"Weapon replaced with: {newWeapon.WeaponName}");
        }
        
        // Helper method to find WeaponSlot in player hierarchy
        public GameObject FindWeaponSlot()
        {
            return FindWeaponSlotRecursive(transform);
        }
        
        private GameObject FindWeaponSlotRecursive(Transform parent)
        {
            // Check if this object has the WeaponSlot tag
            if (parent.CompareTag("WeaponSlot"))
            {
                return parent.gameObject;
            }
            
            // Search through all children recursively
            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject found = FindWeaponSlotRecursive(parent.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        // Add weapon to inventory (for future multi-weapon system)
        public void AddWeapon(BaseWeapon newWeapon)
        {
            if (newWeapon == null) return;
            
            // For now, this just replaces the current weapon
            // In the future, this could add to a weapon inventory
            ReplaceCurrentWeapon(newWeapon);
        }
        
        public void UpdateWeaponList()
        {
            if (Application.isPlaying) return;
            
            // Find weapons in WeaponSlot first, fallback to all children
            GameObject weaponSlot = FindWeaponSlot();
            if (weaponSlot != null)
            {
                availableWeapons = weaponSlot.GetComponentsInChildren<BaseWeapon>(true);
            }
            else
            {
                availableWeapons = GetComponentsInChildren<BaseWeapon>(true);
            }
            
            weaponNames = new string[availableWeapons.Length];
            
            for (int i = 0; i < availableWeapons.Length; i++)
            {
                weaponNames[i] = availableWeapons[i] != null ? availableWeapons[i].WeaponName : "Unknown Weapon";
            }
            
            // Clamp selected index
            if (selectedWeaponIndex >= availableWeapons.Length)
                selectedWeaponIndex = Mathf.Max(0, availableWeapons.Length - 1);
        }
        
        // Health System Integration
        public void TakeDamage(int damageAmount)
        {
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damageAmount);
            }
        }
        
        public void Heal(int healAmount)
        {
            if (healthComponent != null)
            {
                healthComponent.Heal(healAmount);
            }
        }
        
        public int GetCurrentHealth()
        {
            return healthComponent != null ? healthComponent.CurrentHealth : 0;
        }
        
        public int GetMaxHealth()
        {
            return healthComponent != null ? healthComponent.MaxHealth : 100;
        }
        
        public bool IsDead()
        {
            return healthComponent != null ? healthComponent.IsDead : false;
        }
        
        public float GetHealthPercentage()
        {
            return healthComponent != null ? healthComponent.HealthPercentage : 0f;
        }
        
        // Stamina System Integration
        public void ConsumeStamina(int staminaAmount)
        {
            if (staminaComponent != null)
            {
                staminaComponent.ConsumeStamina(staminaAmount);
            }
        }
        
        public void RestoreStamina(int staminaAmount)
        {
            if (staminaComponent != null)
            {
                staminaComponent.RestoreStamina(staminaAmount);
            }
        }
        
        public int GetCurrentStamina()
        {
            return staminaComponent != null ? staminaComponent.CurrentStamina : 0;
        }
        
        public int GetMaxStamina()
        {
            return staminaComponent != null ? staminaComponent.MaxStamina : 100;
        }
        
        public bool IsExhausted()
        {
            return staminaComponent != null ? staminaComponent.IsExhausted : false;
        }
        
        public float GetStaminaPercentage()
        {
            return staminaComponent != null ? staminaComponent.StaminaPercentage : 0f;
        }
        
        public bool CanPerformAction()
        {
            return staminaComponent != null ? staminaComponent.CanPerformAction : true;
        }
    }