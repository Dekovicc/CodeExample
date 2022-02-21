using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unlike.GameDev;

public class WallRun : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;

    [Header("Detection")]
    [SerializeField] private float wallDistance = .5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;
    [SerializeField] private float forwardDetection = 3f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;
    [SerializeField] private float minVelocity;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    [Header("Debuging")]
    [SerializeField] private bool wallLeft = false;
    [SerializeField] private bool wallRight = false;

    [SerializeField] private float velocity;
    [SerializeField] private float angleToWall;
    [SerializeField] private float multi;
    [SerializeField] private float speedMulti;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    RaycastHit wallAngle;

    public float tilt { get; private set; }
    public bool wallRunning { get; private set; }

    private Rigidbody rb;
    private InputManager iM;

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        iM = GetComponent<InputManager>();
    }

    void CheckWall()
    {
        //Check if wall is next to us
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance);
    }
    void CheckWallAngle()
    {
        //get current angle to the wall
        Physics.Raycast(transform.position, orientation.forward, out wallAngle, forwardDetection);
        angleToWall = Vector3.Angle(wallAngle.normal, -orientation.forward);
    }

    private void Update()
    {
        if (!wallRunning)
        {
            //wall run penalty
            if (velocity < minVelocity)
                speedMulti = 0;

            if (velocity >= minVelocity)
                speedMulti = 1;
        }

        velocity = rb.velocity.magnitude;

        CheckWallAngle();
        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
            }
            else if (wallRight)
            {
                StartWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
    }

    void StartWallRun()
    {
        wallRunning = true;      
        
        //stop rb from using gravity
        rb.useGravity = false;

        //reset velocity and apply force
        rb.velocity = Essentials.ResetRBvelY(rb.velocity);
        rb.AddForce(Vector3.down * wallRunGravity * multi * speedMulti, ForceMode.Force);

        //Lerp camera fow to the target one
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        //rotate camera to side
        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);


        if (iM.jump == 1)
        {
            //if player wants to jump, jump away from the wall
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * speedMulti * 100, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * speedMulti * 100, ForceMode.Force);
            }
        }
        if (velocity < minVelocity)
            StopWallRun();
    }

    void StopWallRun()
    {
        wallRunning = false;
        //use gravity
        rb.useGravity = true;

        //reset camera fov and tilt to default
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }
}