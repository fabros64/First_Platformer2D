using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : MonoBehaviour
{
    public Animator animator;

    [SerializeField]
    float FlyingSpeed = 40f;


    [SerializeField]
    private Transform GroundCheck;

    [SerializeField]
    private LayerMask WhatIsGround;

    [Range(0, .3f)]
    [SerializeField]
    private float MovementSmoothing = .05f;

    private Vector3 Velocity = Vector3.zero;

    Rigidbody2D Rigidbody;

    Vector3 FlyingDirection;
    public int RotationValue { get; private set; } = 0;

    Quaternion ActualRotation;


    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        ActualRotation = Quaternion.Euler(Vector3.up);
    }
    void Start()
    {       
    }

    void Update()
    {
        UpdateInputMovement();
    }

    private void FixedUpdate()
    {
        FlyingDirection.x *= Time.fixedDeltaTime * 5;
        FlyingDirection.y *= Time.fixedDeltaTime * 5;
        animator.SetFloat("Speed", Mathf.Abs(FlyingDirection.magnitude));

        UpdateMovement();
    }

    void UpdateInputMovement()
    {
        FlyingDirection = Vector3.zero;
        FlyingDirection += Vector3.right * Input.GetAxisRaw("Horizontal");
        FlyingDirection += Vector3.up * Input.GetAxisRaw("Vertical");
        FlyingDirection = FlyingDirection.normalized * FlyingSpeed;
        

        if (FlyingDirection.x < 0)
        {
            RotationValue = 180;
        }
        else if (FlyingDirection.x > 0)
        {
            RotationValue = 0;
        }

        ActualRotation = Quaternion.Euler(Vector3.up * RotationValue);
    }

    void UpdateMovement()
    {
        Vector3 targetVelocity = new Vector2(FlyingDirection.x, FlyingDirection.y);
        Rigidbody.velocity = Vector3.SmoothDamp(Rigidbody.velocity, targetVelocity, ref Velocity, MovementSmoothing);
        transform.rotation = ActualRotation;
    }

}
