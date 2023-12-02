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

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumptKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;


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
    private void ApplyFinalMovement()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

            characterController.Move(moveDirection * Time.deltaTime);
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
}
