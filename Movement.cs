/* NOTE */

/* Made for physic based FPS */
/* Unlike.GameDev is a custom namespace */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unlike.GameDev;

public class Movement : MonoBehaviour
{
    float playerHeight = 2f;

    [Header("Assignables")]    
    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] private float doubleJumpForce = 10f;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity;
    [SerializeField, Range(0f,.5f)] private float gravityMod;



    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;

    public bool isGrounded { get; private set; }
    public bool canDoubleJump { get; private set; }

    private float horizontalMovement;
    private float verticalMovement;
    private float gravityModLocal = 1;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;

    private RaycastHit slopeHit;

    //Refrences to components
    private Rigidbody rb;
    private InputManager inputs;


    private bool OnSlope()
    {

        //check if we are on a slope so we can alter movement direction
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start()
    {
        // hash required components
        rb = GetComponent<Rigidbody>();
        inputs = FindObjectOfType<InputManager>();

        // setup components
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // check if we are grounded using checksphere for bigger radius
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // call functions every frame
        MyInput();
        ControlDrag();
        ControlSpeed();

        // reset double jump if player touches the ground
        if (isGrounded) { canDoubleJump = true; }

        // calculate slope movement direction
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void MyInput()
    {
        // get input from input manager
        horizontalMovement = inputs.movement.x;
        verticalMovement = inputs.movement.y;

        // calculate independent movement direction
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void Jump(float force)
    {
        // reset rigidbody velocity y (set it to 0) and then apply upwards force
        rb.velocity = Essentials.ResetRBvelY(rb.velocity);

        float _jumpx = Essentials.CalculateJumpForce(force, gravity);

        rb.AddForce(Vector3.up * _jumpx, ForceMode.Impulse);
    }

    void ControlSpeed()
    {
        if (inputs.run == 1 && isGrounded)
        {
            // if player wants to run smoothly lerp from current speed to run speed so we get gradual speed increase
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            // else smoothly lerp back to walk speed
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        // call functions in fixed update
        MovePlayer();
        BetterGravity();

        if (inputs.jump)
        {
            if (isGrounded)
            {
                // if first jump than just jump
                Jump(jumpForce);
            }
            else
            {
                // check if we can double jump and if yes preform double jump
                if (canDoubleJump)
                {                    
                    canDoubleJump = false;
                    Jump(doubleJumpForce);
                }
            }
        }
    }
    
    private void BetterGravity()
    {
        // if player is grounded reset gravity to 1
        if (isGrounded) { gravityModLocal = 1f; }

        if (!isGrounded && rb.velocity.y < 0)
        {
            // if player is not grounded calculate extra gravity, apply it and then increase extra gravity by some value
            Vector3 _gravityVector = Vector3.up * gravity * gravityModLocal;
            rb.AddForce(_gravityVector, ForceMode.Acceleration);

            gravityModLocal += gravityMod;

            if(gravityModLocal > 50f)
            {
                gravityModLocal = 50f;
            }
        }
    }

    void MovePlayer()
    {
        // movement for player in all conditions
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }
   
    public Vector3 CurrentVelocity()
    {
        return rb.velocity;
    }

    // indev stuff
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;

        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
