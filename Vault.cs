using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unlike.GameDev;

public class Vault : MonoBehaviour
{
    [Header("Asignables")]
    [SerializeField] private Transform orientation;
    [Space]
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform legs;

    [Header("Searching")]
    [SerializeField] private float checkDistance;

    [Header("Vaulting")]
    [SerializeField] private float vaultForce;
    [SerializeField] private float vaultBoost;
    [SerializeField] private float maxVaultVelocity;

    [Header("Debug")]
    [SerializeField] private bool vaulting;
    [SerializeField] private bool headLevel;
    [SerializeField] private bool bodyLevel;
    [SerializeField] private bool legsLevel;

    public float tilt { get; private set; }

    private WallRun wallRun;
    private InputManager iM;
    private Rigidbody rb;
    private Movement movement;

    bool CheckLevel(Transform position)
    {
        return Physics.Raycast(position.position, orientation.forward, checkDistance);
    }

    private void Start()
    {
        //cache components
        rb = GetComponent<Rigidbody>();
        iM = GetComponent<InputManager>();
        wallRun = GetComponent<WallRun>();
        movement = GetComponent<Movement>();
    }
    private void Update()
    {
        CheckLevelLoop();
    }

    private void FixedUpdate()
    {
        ClampVelocity();

        //check if obsticle isnt larger than head, if player isnt currently vaulting or wallrunning orr standing still
        if (!headLevel && (bodyLevel || legsLevel) && !vaulting && iM.movement != Vector2.zero && !wallRun.wallRunning) 
        {
            Debug.Log("Vault");
            VaultOver();
        }

        if (!headLevel && !bodyLevel && !legsLevel)
        {
            vaulting = false;
        }
    }

    private void ClampVelocity()
    {
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Mathf.Infinity, maxVaultVelocity), rb.velocity.z);
    }

    private void VaultOver()
    {
        //Vault and check states
        vaulting = true;
        CheckLevelLoop();

        //raycast to see if object has rigidbody
        RaycastHit raycastHit = Physics.Raycast(legs.position, orientation.forward, out raycastHit, checkDistance);

        if (raycastHit.collider)
        {
            //Return if it has
            if (raycastHit.collider.GetComponent<Rigidbody>() != null)
                return;
        }


        //if falling and legs trigger, move up (fake recover)
        if (!headLevel && !bodyLevel && legsLevel && rb.velocity.y > 0)
        {
            rb.AddForce(Vector3.up * vaultForce, ForceMode.Impulse);
        }
        
        //Reset velocity and apply vaulting force
        rb.velocity = Essentials.ResetRBvelY(rb.velocity);
        rb.AddForce(Vector3.up * vaultForce, ForceMode.Impulse);

        //Add boost
        if (!headLevel && !bodyLevel && !legsLevel)
        {
            rb.AddForce(orientation.forward * vaultBoost, ForceMode.Acceleration);
        }
    }
    void CheckLevelLoop()
    {
        headLevel = CheckLevel(head);
        bodyLevel = CheckLevel(body);
        legsLevel = CheckLevel(legs);
    }
}
