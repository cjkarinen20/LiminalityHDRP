using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public GameObject mainCamera;


    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;
    public float waterDrag;
    public float airDrag;

    [Header("Sprinting")]
    public float sprintSpeed;
    public float stamina;
    public float cooldown;
    float maxStamina;
    public Slider staminaSlider;
    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    RaycastHit hit;
    public Transform groundCheck;
    public LayerMask ground;
    public LayerMask water;
    bool grounded;
    public bool inWater;
    public bool crouching;

    [Header("Footsteps")]
    public AudioSource AudioSource;
    public FootstepSystem footstepSystem;
    public AudioClip[] concreteFootSteps;
    public AudioClip[] grassFootSteps;
    public AudioClip[] waterFootSteps;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Transform orientation;
    Vector3 moveDirection;
    private Vector3 playerPos;
    Rigidbody rb;

    float horInput;
    float vertInput;

    [SerializeField] private float velocityXZ, velocityY;

    public MovementState state;
    public enum MovementState //used to determine jumping, sprinting, air strafing and crouching
    {
        walking,
        sprinting,
        crouching,
        air
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        maxStamina = stamina;
        staminaSlider.maxValue= maxStamina;

        grounded = true;

        readyToJump = true;

        ResetJump(); //applies cooldown and resets readyToJump to true

        startYScale = transform.localScale.y; //starting height of the player
    }
    private void Update()
    {
        //ground check
        Debug.Log(Physics.Raycast(groundCheck.position, Vector3.down * 0.1f, ground));
        grounded = Physics.Raycast(groundCheck.position, Vector3.down * 0.1f, ground);
        Debug.Log(grounded);
        Debug.Log(hit.collider.CompareTag("Ground"));
        Debug.Log(hit.collider.CompareTag("Water"));

        if (hit.collider.CompareTag("Ground"))
        {
            grounded = true;
            PlayFootstepSoundL(concreteFootSteps[1]);
        }
        if (hit.collider.CompareTag("Water"))
        {
            inWater = true;
            PlayFootstepSoundL(waterFootSteps[1]);
        }

        //handle drag
        if (grounded)
            rb.drag = groundDrag;
        else if (inWater)
            rb.drag = waterDrag;
        else
            rb.drag = 0;


        Debug.DrawRay(groundCheck.position, Vector3.down * 0.2f,Color.red);
        myInput();
        SpeedControl();
        StateHandler(); //changes the movement state


    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void PlayFootstepSoundL(AudioClip source)
    {
        AudioSource.pitch = Random.Range(0.8f, 1f);
        AudioSource.PlayOneShot(source);
    }
    private void myInput()
    {
        horInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
  
        //when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            
             readyToJump = false;

             Jump();

             Invoke(nameof(ResetJump), jumpCoolDown);
        }

        //start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            readyToJump = false;
        }
        //stop crouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            readyToJump = true;
        }
    }
    private void StateHandler()
    {
        //mode - crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            Debug.Log("Crouching");
            desiredMoveSpeed = crouchSpeed;
        }
        //mode - sprinting
        else if (grounded && Input.GetKeyDown(sprintKey))
        {

            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
            decreaseStamina();
        }

        //mode - walking
        else if (grounded)
        {
            state = MovementState.walking;
            Debug.Log("Walking");
            desiredMoveSpeed = walkSpeed;
            readyToJump = true;
            increaseStamina();
        }
    
        //mode - air
        else 
        {
            state = MovementState.air;
            Debug.Log("Air");
            readyToJump = false;
        }
        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    } 
    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * vertInput + orientation.right * horInput;


        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        //on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        //in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }
    private void decreaseStamina()
    {
        if (stamina != 0)
            stamina -= cooldown * Time.deltaTime;
    }
    private void increaseStamina()
    {
        stamina += cooldown * Time.deltaTime;
    }
    private void SpeedControl()
    {
        //limiting speed on slope
        if(OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        //limiting speed on ground or in air
        else
        {
            Vector3 flatVal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //limit velocity if needed
            if (flatVal.magnitude > moveSpeed)
            {
                Vector3 limitedVal = flatVal.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVal.x, rb.velocity.y, limitedVal.z);
            }
        }
    }
    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, startYScale * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false; //if raycast doesn't hit anything
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

}
