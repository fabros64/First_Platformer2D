﻿using System.Collections;
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
    float WaterSpeed = 5f;
    bool isInWater = false;

    [SerializeField]
    float JumpForce = 400f;

    [SerializeField] 
    private Transform GroundCheck;                     

    [SerializeField] 
    private LayerMask WhatIsGround;							

    [Range(0, .3f)] 
    [SerializeField] 
    private float MovementSmoothing = .05f;

    const float GroundedRadius = .2f;
    private bool Grounded;

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;

    Rigidbody2D Rigidbody;

    Vector3 WalkingDirection;
    public int RotationValue { get; private set; } = 0;

    Quaternion ActualRotation;

    private Vector3 Velocity = Vector3.zero;

    private float jumpTimeCounter;
    [SerializeField] float jumpTime;
    private bool isJumping;

    public Transform jumpPoint;

    bool jump = false;

    bool isCoroutineRecoilExecuting = false;

    bool isCoroutineJumpExecuting = false;

    [SerializeField]
    LayerMask obstacleLayers;

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

        if(isInWater)
            StartCoroutine(PoisonDamage());
    }

    private void FixedUpdate()
    {
        bool wasGrounded = Grounded;
        Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                Grounded = true;

                if (!wasGrounded)
                {
                        CreateDust();
                        StartCoroutine(JumpSqueeze(1.15f, 0.8f, 0.05f));
                }

                animator.SetBool("IsFalling", false);
                animator.SetBool("IsJumping", false);
            }
        }

        WalkingDirection.x *= Time.fixedDeltaTime*5;
        animator.SetFloat("Speed", Mathf.Abs(WalkingDirection.x));

        UpdateMovement();
        Jump();
    }

    void Jump()
    {
        Collider2D[] hitObstacles = Physics2D.OverlapCircleAll(jumpPoint.position, 0.2f, obstacleLayers);
        if (hitObstacles.Length > 0 && Grounded)
        {
            isJumping = true;
            jump = true;
            //foreach (Collider2D obstacle in hitObstacles)
            //{
            //    if (obstacle.gameObject == gameObject && hitObstacles.Length == 1)
            //        break;
            //}

            //Rigidbody.AddForce(new Vector2(jumpForce / 4f * jumpDirection, jumpForce), ForceMode2D.Impulse);

        }
        if (isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x-3, JumpForce + jumpTimeCounter * 20);
                jumpTimeCounter -= Time.deltaTime;
                isJumping = true;
                StartCoroutine(JumpSqueeze(0.7f, 1.2f, 0.1f));
            }
            else
            {
                isJumping = false;
            }
        }

        if(hitObstacles.Length == 0)
        {
            isJumping = false;
            jump = false;
        }
    }

    void UpdateInputMovement()
    {
        //WalkingDirection = Vector3.zero;
        //WalkingDirection += Vector3.right * Input.GetAxisRaw("Horizontal");
        WalkingDirection = Vector3.right;
        WalkingDirection = WalkingDirection.normalized * (isInWater? WaterSpeed : WalkingSpeed);

        //if (Input.GetButtonDown("Jump") && Grounded)
        //{
        //    jump = true;
        //    isJumping = true;
        //}

        //if (Input.GetButton("Jump") && isJumping)
        //{
        //    if (jumpTimeCounter > 0)
        //    {
        //        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, JumpForce + jumpTimeCounter*10);
        //        jumpTimeCounter -= Time.deltaTime;
        //        isJumping = true;
        //        StartCoroutine(JumpSqueeze(0.7f, 1.2f, 0.1f));
        //    }
        //    else
        //    {
        //        isJumping = false;
        //    }
        //}

        //if (Input.GetButtonUp("Jump"))
        //{
        //    jump = false;
        //    isJumping = false;
        //}

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

    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }

    }

    void UpdateMovement()
    {
        Vector3 targetVelocity = new Vector2(WalkingDirection.x, Rigidbody.velocity.y);
        Rigidbody.velocity = Vector3.SmoothDamp(Rigidbody.velocity, targetVelocity, ref Velocity, MovementSmoothing);
        transform.rotation = ActualRotation;

        if (jump && Grounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
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
    
    public void InWater()
    {
        isInWater = true;
    }

    IEnumerator PoisonDamage()
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(1f);
        GetComponent<PlayerCombat>().currentHealth -= 1;

        isCoroutineRecoilExecuting = false;
    }

    public void OutWater()
    {
        isInWater = false;
    }
}
