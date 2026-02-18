using UnityEngine;

using System;
using System.Collections;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [Range(0, 100)] public float mouseSensitivity = 50f;
        [Range(0f, 200f)] private float snappiness = 20f;

        [Range(0f, 20f)] public float walkSpeed = 3f;
        [Range(0f, 30f)] public float sprintSpeed = 5f;

        [Range(0f, 50f)] public float gravity = 9.81f;

        public float normalFov = 60f;
        public float sprintFov = 65f;
        public float fovChangeSpeed = 1f;

        public float walkingBobbingSpeed = 10f;
        public float bobbingAmount = 0.05f;
        private float sprintBobMultiplier = 1.2f;
        private float recoilReturnSpeed = 8f;

        public bool canSprint = true;

        //Ground check
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
        private bool isSprinting;

        private float originalCameraParentHeight;
        private Camera cam;

        private float bobTimer;
        private Vector3 recoil = Vector3.zero;

        //Enable/Disable controls
        private bool isLook = true, isMove = true;
        

        private float currentCameraHeight;
        private float currentBobOffset;
        private float currentFov;
        private float fovVelocity;

        // New Input System actions
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;

        public Animator animator;

        private Quaternion targetRotation;
        private bool isRotatingToTarget = false;
        public float lookAtSpeed = 5f;

        private bool isLookRestricted = false;
        private float restrictedYawCenter = 0f; 
        public float maxYawDeviation = 45f; 
        public float maxPitchDeviation = 35f;

    private void Awake()
        {
            // Initialize Input Actions
            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
           

            lookAction = new InputAction("Look", binding: "<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick");

            sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");

            moveAction.Enable();
            lookAction.Enable();
            sprintAction.Enable();

            characterController = GetComponent<CharacterController>();
            cam = playerCamera.GetComponent<Camera>();
            originalCameraParentHeight = cameraParent.localPosition.y;

            Cursor.lockState = CursorLockMode.Locked;
            currentCameraHeight = originalCameraParentHeight;
            currentBobOffset = 0f;
            currentFov = normalFov;

            rotX = transform.rotation.eulerAngles.y;
            rotY = playerCamera.localRotation.eulerAngles.x;
            xVelocity = rotX;
            yVelocity = rotY;
        }

        private void OnDestroy()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            sprintAction?.Disable();
        }

        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, groundCheckQueryTriggerInteraction);
        
        if (isGrounded && moveDirection.y < 0f)
            {
                moveDirection.y = -2f;
            }

            if (isLook)
            {
            if (isRotatingToTarget)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);


                float targetPitch = 0f; // Look straight ahead
                rotY = Mathf.Lerp(rotY, targetPitch, lookAtSpeed * Time.deltaTime);
                yVelocity = Mathf.Lerp(yVelocity, targetPitch, lookAtSpeed * Time.deltaTime);

                playerCamera.transform.localRotation = Quaternion.Euler(yVelocity, 0f, 0f);

                // Update internal rotation values to match
                rotX = transform.rotation.eulerAngles.y;
                xVelocity = rotX;


                bool horizontalDone = Quaternion.Angle(transform.rotation, targetRotation) < 0.5f;
                bool verticalDone = Mathf.Abs(rotY) < 0.5f;
                // Check if close enough to target
                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
                {
                    isRotatingToTarget = false;
                    transform.rotation = targetRotation;

                    rotY = 0f;
                    yVelocity = 0f;
                    playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                    if (isLookRestricted)
                    {
                        restrictedYawCenter = rotX;
                    }
                }
            }
            else
            {
                // Normal mouse look
                Vector2 lookInput = lookAction.ReadValue<Vector2>();
                float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
                float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

                rotX += mouseX;
                rotY -= mouseY;

                if (isLookRestricted)
                {
                    // Clamp yaw (horizontal) around the NPC direction
                    float minYaw = restrictedYawCenter - maxYawDeviation;
                    float maxYaw = restrictedYawCenter + maxYawDeviation;
                    rotX = Mathf.Clamp(rotX, minYaw, maxYaw);

                    // Clamp pitch (vertical)
                    rotY = Mathf.Clamp(rotY, -maxPitchDeviation, maxPitchDeviation);
                }
                else
                {
                    // Normal pitch clamp when not restricted
                    rotY = Mathf.Clamp(rotY, -90f, 90f);
                }

                xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
                yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

                playerCamera.transform.localRotation = Quaternion.Euler(yVelocity, 0f, 0f);
                transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
            }
        }

            HandleHeadBob();

            float targetFov = isSprinting ? sprintFov : normalFov;
            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
            cam.fieldOfView = currentFov;

            HandleMovement();
        }

        private void HandleHeadBob()
        {
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            bool isMovingEnough = horizontalVelocity.magnitude > 0.1f;

            float targetBobOffset = isMovingEnough ? Mathf.Sin(bobTimer) * bobbingAmount : 0f;
            currentBobOffset = Mathf.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * walkingBobbingSpeed);

            if (!isGrounded)
            {
                bobTimer = 0f;
                float targetCameraHeight = originalCameraParentHeight;
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
                float targetCameraHeight = originalCameraParentHeight;
                currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
                cameraParent.localPosition = new Vector3(
                    cameraParent.localPosition.x,
                    currentCameraHeight + currentBobOffset,
                    cameraParent.localPosition.z);
                recoil.z = moveInput.x * 0f;
            }
            else
            {
                bobTimer = 0f;
                float targetCameraHeight = originalCameraParentHeight;
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
            Vector2 move = moveAction.ReadValue<Vector2>();
            moveInput.x = move.x;
            moveInput.y = move.y;
            isSprinting = canSprint && sprintAction.IsPressed() && moveInput.y > 0.1f;

            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            if (!isMove) currentSpeed = 0f;

            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
            moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

            if (!isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            moveDirection = new Vector3(moveVector.x, moveDirection.y, moveVector.z);
            characterController.Move(moveDirection * Time.deltaTime);

            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            float speed = horizontalVelocity.magnitude;

            
            bool isMoving = speed > 0.1f;

            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isRunning", isMoving && isSprinting);

        
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

        public void SetLookDirection(Quaternion rotation, float speed = 5f)
        {
            targetRotation = rotation;
            isRotatingToTarget = true;
            lookAtSpeed = speed;

    }

        public void SetLookRestriction(bool restricted, float centerYaw = 0f)
        {
            isLookRestricted = restricted;
            restrictedYawCenter = centerYaw;
        }
}

