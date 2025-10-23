    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public partial class FirstPersonController : MonoBehaviour
    {
        // Input System
        PlayerInput playerInput;
        PlayerInput.MainActions input;
        
        [Range(0, 100)] public float mouseSensitivity = 50f;
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
        
        // Animation States
        public const string IDLE = "Idle";
        public const string WALK = "Walk";
        public const string ATTACK1 = "Attack 1";
        public const string ATTACK2 = "Attack 2";
        private string currentAnimationState;
        
        // Attack System
        [Header("Attacking")]
        public float attackDistance = 3f;
        public float attackDelay = 0.4f;
        public float attackSpeed = 1f;
        public int attackDamage = 1;
        public LayerMask attackLayer;
        public GameObject hitEffect;
        public float hitEffectDuration = 10f;
        public AudioClip swordSwing;
        public AudioClip hitSound;
        
        private bool attacking = false;
        private bool readyToAttack = true;
        private int attackCount;

        public float CurrentCameraHeight => isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cam = playerCamera.GetComponent<Camera>();
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponent<AudioSource>();
            
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
                float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
                float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

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

            // Handle attack input (continuous check for holding)
            if (input.Attack.IsPressed())
            {
                Attack();
            }

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
            isSprinting = canSprint && input.Sprint.IsPressed() && moveInput.y > 0.1f && isGrounded && !isCrouching && !isSliding;

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
                moveDirection.y = jumpSpeed;
            }
        }

        void AssignInputs()
        {
            input.Jump.performed += ctx => Jump();
            input.Attack.started += ctx => Attack();
        }

        // Animation System
        public void ChangeAnimationState(string newState)
        {
            if (currentAnimationState == newState) return;
            currentAnimationState = newState;
            if (animator != null)
                animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
        }

        void SetAnimations()
        {
            if (animator == null) return;
            
            if (!attacking)
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

        // Attack System
        public void Attack()
        {
            if (!readyToAttack || attacking) return;

            readyToAttack = false;
            attacking = true;

            Invoke(nameof(ResetAttack), attackSpeed);
            Invoke(nameof(AttackRaycast), attackDelay);

            if (audioSource != null && swordSwing != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(swordSwing);
            }

            if (attackCount == 0)
            {
                ChangeAnimationState(ATTACK1);
                attackCount++;
            }
            else
            {
                ChangeAnimationState(ATTACK2);
                attackCount = 0;
            }
        }

        void ResetAttack()
        {
            attacking = false;
            readyToAttack = true;
        }

        void AttackRaycast()
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, attackDistance, attackLayer))
            {
                HitTarget(hit.point);

                if (hit.transform.TryGetComponent<Actor>(out Actor T))
                {
                    T.TakeDamage(attackDamage);
                }
            }
        }

        void HitTarget(Vector3 pos)
        {
            if (audioSource != null && hitSound != null)
            {
                audioSource.pitch = 1;
                audioSource.PlayOneShot(hitSound);
            }

            if (hitEffect != null)
            {
                GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
                Destroy(GO, hitEffectDuration);
            }
        }
    }