using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;
    [Space]
    [SerializeField] private Vector3 boxHalfExtents;

    [Header("Movement")]
    [SerializeField] private float maxVelocity;
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float acceleration;
    [Space]
    [SerializeField, Range(0f, 0.3f)] private float threshold;
    [SerializeField] private float walkDrag;
    [SerializeField] private float stopDrag;

    [Header("Jumping")]
    [SerializeField] private float jumpTimer;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool jumpWanted;
    [SerializeField, Range(0f,2f)] private float movementJumpMultiplier;
    [Space]
    [SerializeField] private float cayoteTime;
    [Space]
    [SerializeField] private float normalGravity;
    [SerializeField] private float pullbackGravity;

    [Header("Camera")]
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private float targetDistance;
    [SerializeField] private float moveTargetSmoothness;

    [Header("Feel")]
    [SerializeField] private MMFeedbacks Jump_MMF;
    [SerializeField] private ParticleSystem walkParticles;
    [SerializeField] private AudioSource footsteps;


    public bool playerJumped {get; private set; }
    public bool jumping { get; private set; }


    //Refrences
    private InputManager inputManager;
    private Rigidbody2D rb;
    private float cayoteTimeTimer;
    private bool GroundCheck()
    {        
        return Physics2D.BoxCast(groundCheck.position, boxHalfExtents, 0, Vector2.zero, 0, groundLayer);
    }

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleDataStates();



        Jump();
        UpdateCameraFollow();


    }

    private void HandleDataStates()
    {
        //Jump
        if (inputManager._jump)
        {
            jumpWanted = true;
        }
        if (isGrounded)
        {
            playerJumped = false;
            cayoteTimeTimer = cayoteTime;
        }

        //Cayote time
        cayoteTimeTimer -= Time.deltaTime;

        if (cayoteTimeTimer < 0f)
        {
            jumpWanted = false;
        }

        jumping = cayoteTimeTimer > 0 && jumpWanted ? true : false;

        isGrounded = GroundCheck();

        //play and stop walking particles
        if (isGrounded && !walkParticles.isPlaying)
            walkParticles.Play();
        else if ((!isGrounded || inputManager._movement == 0) && walkParticles.isPlaying)
        {
            walkParticles.Stop();
        }

        //play footsteps spund if player is moving and stop it if he isnt
        if ((rb.velocity.magnitude > 1 && isGrounded) && !footsteps.isPlaying && inputManager._movement != 0)
            footsteps.Play();
        else if (rb.velocity.magnitude <= 1 || !isGrounded || inputManager._movement == 0)
            footsteps.Stop();

        if (jumpWanted)
            StartCoroutine(ResetWantedJump());
    }

    private void FixedUpdate()
    {
        Move();
        Drag();
    }
    private void Move()
    {
        if (rb.velocity.magnitude > defaultSpeed)
            return;
        else
        {
            //Get input
            Vector2 _movenemetVector = Vector2.right * inputManager._movement * defaultSpeed;

            //apply movement to current vector
            RaycastHit2D rayhit = Physics2D.Raycast(transform.position, Vector2.down, 4f, groundLayer);
            Vector2 groundNormal = Vector3.ProjectOnPlane(_movenemetVector, rayhit.normal);

            //lerp to target speed and clamp velocity
            rb.velocity = Vector3.Lerp(rb.velocity, groundNormal, acceleration * Time.deltaTime);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        }
    }
    private void Drag()
    {
        //if player is walking use stopDrag
        if ((inputManager._movement <= threshold && inputManager._movement >= -threshold) && isGrounded)
        {
            rb.drag = stopDrag;
        }
        else
        {
            rb.drag = walkDrag;
        }
    }

    private void Jump()
    {
        //Better gravity
        rb.gravityScale = rb.velocity.y <= -0.01f ? pullbackGravity : normalGravity;


        //if cayote time isnt 0 and jump is wanted jump
        if (cayoteTimeTimer > 0f && jumpWanted)
        {
            cayoteTimeTimer = 0f;

            //calculations
            Vector2 _jumpVector = Vector2.up * jumpForce;
            //penalty
            if (rb.velocity.x > threshold && rb.velocity.x < threshold)
            {
                _jumpVector *= movementJumpMultiplier;
            }

            //force 
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * _jumpVector, ForceMode2D.Impulse);

            //Trigger feedbacks and reset values
            Jump_MMF.PlayFeedbacks();
            jumpWanted = false;
        }
    }

    private void UpdateCameraFollow()
    {
        //Dynamic camera follow 
        Vector2 _targetPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 _cameraTarget = (_targetPosition + rb.velocity) * targetDistance;

        cameraFollow.position = Vector3.Lerp(cameraFollow.position, _cameraTarget, moveTargetSmoothness * Time.deltaTime);
    }

    public void ResetRbVelocity()
    {
        rb.velocity = Vector2.zero;
    }
    public void ResetWantJump()
    {
        jumpWanted = false;
    }

    IEnumerator ResetWantedJump()
    {
        yield return new WaitForSeconds(jumpTimer);
    }

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawCube(groundCheck.position, boxHalfExtents);
    }
}