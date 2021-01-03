using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public ParticleSystem dust;
    public Animator animator;

    [SerializeField]
    float WalkingSpeed = 40f;

    [SerializeField]
    float JumpForce = 400f;

    [SerializeField] 
    private Transform GroundCheck;                     

    [SerializeField] 
    private LayerMask WhatIsGround;							

    [Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f;	// How much to smooth out the movement

    const float GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool Grounded;

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;


    Rigidbody2D Rigidbody;
    Vector3 WalkingDirection;
    int RotationValue = 0;
    Quaternion ActualRotation;
    private Vector3 Velocity = Vector3.zero;

    private float jumpTimeCounter;
    [SerializeField] float jumpTime;
    private bool isJumping;

    bool jump = false;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        ActualRotation = Quaternion.Euler(Vector3.up);

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
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
        bool wasGrounded = Grounded;
        Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                Grounded = true;

                if (!wasGrounded)
                    CreateDust();

                animator.SetBool("IsFalling", false);
                animator.SetBool("IsJumping", false);
            }
        }

        WalkingDirection.x *= Time.fixedDeltaTime*5;
        animator.SetFloat("Speed", Mathf.Abs(WalkingDirection.x));

        UpdateMovement();
    }

    void UpdateInputMovement()
    {
        WalkingDirection = Vector3.zero;

        WalkingDirection += Vector3.right * Input.GetAxisRaw("Horizontal");
        WalkingDirection = WalkingDirection.normalized * WalkingSpeed;

        if (Input.GetButtonDown("Jump") && Grounded)
        {
            jump = true;
            isJumping = true;
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + jumpTimeCounter*10);
                jumpTimeCounter -= Time.deltaTime;
                isJumping = true;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            jump = false;
            isJumping = false;
        }

        if ( WalkingDirection.x < 0)
        {
            if (RotationValue == 0 && Grounded)
                CreateDust();
            RotationValue = 180;
        }
        else if ( WalkingDirection.x > 0)
        {
            if (RotationValue == 180 && Grounded)
                CreateDust();
            RotationValue = 0;
        }

        ActualRotation = Quaternion.Euler(Vector3.up * RotationValue);
    }

    void UpdateMovement()
    {
        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(WalkingDirection.x, Rigidbody.velocity.y);
        // And then smoothing it out and applying it to the character
        Rigidbody.velocity = Vector3.SmoothDamp(Rigidbody.velocity, targetVelocity, ref Velocity, MovementSmoothing);
        //Rigidbody.velocity = targetVelocity;
        transform.rotation = ActualRotation;

        if (jump && Grounded)
        {
            // Add a vertical force to the player.
            //Grounded = false;
            isJumping = true;
            jumpTimeCounter = jumpTime;
            //Rigidbody.AddForce(new Vector2(0f, JumpForce));
        }

        if(isJumping)
        {
            animator.SetBool("IsJumping", true);
        }

        if (!Grounded && Rigidbody.velocity.y < 0 && !isJumping)
        {
            animator.SetBool("IsFalling", true);
        }

        jump = false;
    }

    void CreateDust()
    {
        dust.Play();
    }
    
}
