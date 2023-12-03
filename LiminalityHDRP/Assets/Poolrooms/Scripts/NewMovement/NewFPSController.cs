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
    //add interactionEnabled field here later
    [SerializeField] private bool footstepsEnabled = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumptKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f; 


    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80.0f;

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
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] grassSounds = default;
    [SerializeField] private AudioClip[] dirtSounds = default;
    [SerializeField] private AudioClip[] tileSounds = default;
    [SerializeField] private AudioClip[] waterSounds = default;
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

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;



    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponentInChildren<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
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

            //add if statement for interaction

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
                        footstepAudioSource.PlayOneShot(grassSounds[Random.Range(0, grassSounds.Length - 1)]);
                        break;
                    case "Dirt":
                        footstepAudioSource.PlayOneShot(dirtSounds[Random.Range(0, dirtSounds.Length - 1)]);
                        break;
                    case "Tile":
                        footstepAudioSource.PlayOneShot(tileSounds[Random.Range(0, tileSounds.Length - 1)]);
                        break;
                    case "Water":
                        footstepAudioSource.PlayOneShot(waterSounds[Random.Range(0, waterSounds.Length - 1)]);
                        break;
                    default:
                        break;
                }
            }
            footstepTimer = GetCurrentOffset;
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
}
