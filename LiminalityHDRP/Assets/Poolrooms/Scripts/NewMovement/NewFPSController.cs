using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NewFPSController : MonoBehaviour
{
    public bool canMove { get; private set; } = true;
    private bool isSprinting => sprintEnabled && Input.GetKey(sprintKey) && Input.GetKey(KeyCode.W) && !isCrouching;
    private bool shouldJump => Input.GetKeyDown(jumptKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) || Input.GetKeyUp(crouchKey) && !ongoingCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool sprintEnabled = true;
    [SerializeField] private bool jumpEnabled = true;
    [SerializeField] private bool crouchEnabled = true;
    [SerializeField] private bool headBobEnabled = true;
    [SerializeField] private bool slopeSlidingEnabled = true;
    [SerializeField] private bool zoomEnabled = true;
    [SerializeField] private bool interactionEnabled = true;
    [SerializeField] private bool footstepsEnabled = true;
    [SerializeField] private bool staminaEnabled = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumptKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    [Header("Movement Parameters")]
    [SerializeField] public float walkSpeed = 4.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f; 


    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80.0f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float regenCooldown = 3;
    [SerializeField] private float healthIncreaseIncrement = 1;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    private float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaIncreaseIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> OnStaminaChange;

    [Header("Look Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3 (0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool ongoingCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkbobSpeed = 14f;
    [SerializeField] private float walkbobAmount = 0.5f;
    [SerializeField] private float sprintbobSpeed = 18f;
    [SerializeField] private float sprintbobAmount = 0.11f;
    [SerializeField] private float crouchbobSpeed = 8f;
    [SerializeField] private float crouchbobAmount = 0.025f;
    private float defaultYPos = 0;
    private float Timer;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 40f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.3f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] grassSounds = default;
    [SerializeField] private AudioClip[] dirtSounds = default;
    [SerializeField] private AudioClip[] tileSounds = default;
    [SerializeField] private AudioClip[] waterSounds = default;
    [SerializeField] private AudioClip[] waterRunSounds = default;
    [SerializeField] private AudioClip[] grassRunSounds = default;
    [SerializeField] private AudioClip[] tileRunSounds = default;
    [SerializeField] private AudioClip[] dirtRunSounds = default;

    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed = sprintStepMultiplier : baseStepSpeed;


    // SLIDING PARAMETERS
    private Vector3 hitPointNormal;

    private bool isSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }
    [Header("Interaction Parameters")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
    }
    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }


    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponentInChildren<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        //Specifies when to update movement
        if (canMove) 
        {
            HandleMovementInput();
            HandleMouseLook();

            if(jumpEnabled)
                HandleJump();
            if(crouchEnabled)
                HandleCrouch();
            if (headBobEnabled)
                HandleHeadBob();
            if (zoomEnabled)
                HandleZoom();
            if (footstepsEnabled)
                HandleFootsteps();
            if (interactionEnabled)
            {
                HandleInteractionCheck();
                HandleInteractionInput();

            }
            if (staminaEnabled)
                HandleStamina();


            print("Health: " + currentHealth);
            print("Stamina: " + currentStamina);

            ApplyFinalMovement();
        }
    }
    private void HandleMovementInput()
    {
        currentInput = new Vector2(isSprinting ? sprintSpeed : walkSpeed = Input.GetAxis("Vertical"), walkSpeed = Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }
    private void HandleJump()
    {
        if (shouldJump)
            moveDirection.y = jumpForce;
    }
    private void HandleCrouch()
    {
        if (shouldCrouch)
            StartCoroutine(CrouchStand());
    }
    private void HandleHeadBob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            Timer += Time.deltaTime + (isCrouching ? crouchbobSpeed : isSprinting ? sprintbobSpeed : walkbobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(Timer) * (isCrouching ? crouchbobAmount : isSprinting ? sprintbobAmount : walkbobAmount),
                playerCamera.transform.localPosition.z);
        }

    }
    private void HandleStamina()
    {
        if (isSprinting && currentInput != Vector2.zero)
        {
            if (regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }
            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            if (currentStamina < 0)
                currentStamina = 0;

            OnStaminaChange?.Invoke(currentStamina);

            if (currentStamina <= 0)
                sprintEnabled = false;
        }
        if (!isSprinting && currentStamina < maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void HandleZoom()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(true));

        }
        if (Input.GetKeyUp(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(false));

        }
    }
    private void ApplyFinalMovement()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (slopeSlidingEnabled && isSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.z) * slopeSpeed;


            characterController.Move(moveDirection * Time.deltaTime);
    }
    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch(hit.collider.tag)
                {
                    case "Grass":
                        if (isSprinting)
                            footstepAudioSource.PlayOneShot(grassRunSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        else
                        {
                            footstepTimer = baseStepSpeed;
                            footstepAudioSource.PlayOneShot(grassSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        }
                        break;
                    case "Dirt":
                        if (isSprinting)
                            footstepAudioSource.PlayOneShot(dirtRunSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        else
                        {
                            footstepTimer = baseStepSpeed;
                            footstepAudioSource.PlayOneShot(dirtSounds[UnityEngine.Random.Range(0, dirtSounds.Length - 1)]);
                        }
                        break;
                    case "Tile":
                        if (isSprinting)
                            footstepAudioSource.PlayOneShot(tileRunSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        else
                        {
                            footstepTimer = baseStepSpeed;
                            footstepAudioSource.PlayOneShot(tileSounds[UnityEngine.Random.Range(0, tileSounds.Length - 1)]);
                        }
                        break;
                    case "Water":
                        if (isSprinting)
                            footstepAudioSource.PlayOneShot(waterRunSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        else
                        {
                            footstepTimer = baseStepSpeed;
                            footstepAudioSource.PlayOneShot(waterSounds[UnityEngine.Random.Range(0, waterSounds.Length - 1)]);
                        }              
                        break;
                    default:
                        break;
                }
            }
            baseStepSpeed = 0.5f;
            footstepTimer = baseStepSpeed;
        }
    }

    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0)
            KillPlayer();
        else if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }
    private void KillPlayer()
    {
        currentHealth = 0;

        if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        print("DEAD");

        //Add further implementation later
        //Make a "fade to black" animation play here and restart the scene
    }
    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 8 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable)
                    currentInteractable.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }

    }
    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;
            

        ongoingCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        isCrouching = !isCrouching;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        ongoingCrouchAnimation = false;
    }
    private IEnumerator ToggleZoom(bool enterZoom)
    {
        float targetFOV = enterZoom ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }
    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenCooldown);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while(currentHealth < maxHealth)
        {
            currentHealth += healthIncreaseIncrement;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        regeneratingHealth = null;
    }
    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(currentStamina < maxStamina)
        {
            if (currentStamina > 0)
                sprintEnabled = true;
            currentStamina += staminaIncreaseIncrement;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            yield return timeToWait;
        }
        regeneratingStamina = null;
    }
}
