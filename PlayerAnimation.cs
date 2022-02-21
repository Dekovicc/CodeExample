using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Transform _playerVisual;
    [SerializeField] private float animTime;
    [SerializeField] private AnimationCurve angleCurve;
    [SerializeField] private float angleCurveTimeModifier;
    [SerializeField] private LayerMask ground;


    private InputManager inputManager;
    private Animator animator;
    private Player player;

    private Vector3 targetRotation;
    private Vector3 groundRotation;
    private float angleCurveTime;

    private Quaternion groundRotQ;

    private bool onetime = false;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        animator.SetBool("Jump", player.jumping);

        GetGroundNormal();
        ControllAnimations();
    }

    private void GetGroundNormal()
    {        
        //get ground normal
        RaycastHit2D rayhit = Physics2D.Raycast(transform.position, Vector2.down, 3, ground);

        if(rayhit.collider != null)
        {
            groundRotation = rayhit.normal.y > 0.8f ? rayhit.normal : Vector3.zero;
        }

        if(rayhit.collider == null)
        {
            groundRotation = Vector3.zero;
        }

        //convert rotation to quaternion
        groundRotQ = Quaternion.FromToRotation(transform.up, groundRotation);
    }


    private void ControllAnimations()
    {

        if (inputManager._movement != 0) {
            angleCurveTime = Mathf.Lerp(angleCurveTime, 1, Time.deltaTime * angleCurveTimeModifier);
            onetime = false;
        }

        //Reset
        if (inputManager._movement == 0 && !onetime)
        {
            //get time for angle curve
            angleCurveTime = Mathf.Lerp(angleCurveTime, 1.5f, Time.deltaTime * angleCurveTimeModifier);

            //reset curve value
            if (angleCurve.Evaluate(angleCurveTime) >= 1.3f)
            {
                angleCurveTime = 0f;
                onetime = true;
            }

        }

        //apply values to vector 3
        targetRotation.z = angleCurve.Evaluate(angleCurveTime);
        targetRotation *= -inputManager._movement;
        
        //Apply rotation
        _playerVisual.rotation = Quaternion.Lerp(_playerVisual.rotation, Quaternion.Euler(targetRotation) * groundRotQ, animTime * Time.deltaTime);
    }
}