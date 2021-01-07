using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;
    public LayerMask playerLayer;
    public LayerMask obstacleLayers;
    public Transform player;

    public int maxHealth = 100;
    int currentHealth;

    public int attackDamage = 10;
    public float attackRange = 0.1f;

    public float attackRate = 10f;
    float nextAttackTime;

    public float jumpForce = 100f;
    float jumpDirection;
    bool isCoroutineJumpExecuting = false;

    Rigidbody2D enemyRigidBody;

    float attackDistance;

    public float WalkingSpeed = 1f;
    int flipDirection;
    Vector3 WalkingDirection;

    bool isCoroutineRecoilExecuting = false;

    void Start()
    {
        currentHealth = maxHealth;
        nextAttackTime = Time.time + Random.Range(10f / attackRate, 25f / attackRate);
        enemyRigidBody = GetComponent<Rigidbody2D>();
        attackDistance = Vector2.Distance(transform.position, attackPoint.transform.position)
            + attackRange;
        flipDirection = 0;
        jumpDirection = -1;
        WalkingDirection = new Vector3(0, enemyRigidBody.velocity.y);
    }

    void FixedUpdate()
    {
        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
            jumpDirection = flipDirection == 0 ? (-1) : 1;
            StartCoroutine(Jump(1));
            nextAttackTime = Time.time + Random.Range(10f / attackRate, 25f / attackRate);
        }
    }

    IEnumerator Jump(float time)
    {
        Collider2D[] hitObstacles = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, obstacleLayers);
        if(hitObstacles.Length > 0)
        {
            if (isCoroutineJumpExecuting)
                yield break;
            isCoroutineJumpExecuting = true;
            yield return new WaitForSeconds(time);
            enemyRigidBody.AddForce(new Vector2(jumpForce/4f * jumpDirection, jumpForce), ForceMode2D.Impulse);
            isCoroutineJumpExecuting = false;
        }          
    }

    IEnumerator Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        if (hitEnemies.Length > 0)
        {
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(0.2f);
            hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
            if (hitEnemies.Length > 0)
            {
                foreach (Collider2D player in hitEnemies)
                {
                    player.GetComponent<PlayerCombat>().TakeDamage(attackDamage);
                    StartCoroutine(RecoilWithDelay(0.2f));
                    break;
                }
            }
        }        
    }

    IEnumerator RecoilWithDelay(float time)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(time);
        player.GetComponent<Rigidbody2D>().AddForce(new Vector2(jumpDirection * 30, 5), ForceMode2D.Impulse);
        isCoroutineRecoilExecuting = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        animator.SetTrigger("Hurt");

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetBool("IsDead", true);

        GetComponent<Rigidbody2D>().simulated = false;
        this.enabled = false;        
    }

    void SetProperDirection()
    {
        if ((transform.position.x - player.position.x) > attackDistance)
        {
            flipDirection = 0;
            WalkingDirection = Vector3.right * (-1);
            animator.SetBool("FollowingPlayer", true);
        }
        else if (Mathf.Abs(transform.position.x - player.position.x) < attackDistance)
        {
            WalkingDirection = new Vector3(0, enemyRigidBody.velocity.y);
            animator.SetBool("FollowingPlayer", false);
            if ((transform.position.x - player.position.x) > 0)
                flipDirection = 0;
            else flipDirection = 180;
        }
        else
        {
            flipDirection = 180;
            WalkingDirection = Vector3.right;
            animator.SetBool("FollowingPlayer", true);
        }

        WalkingDirection = WalkingDirection.normalized * WalkingSpeed;
        WalkingDirection.x *= Time.fixedDeltaTime * 5;
        transform.rotation = Quaternion.Euler(Vector3.up * flipDirection);
    }

    public void FollowPlayer()
    {
        SetProperDirection();
            enemyRigidBody.velocity = new Vector3(enemyRigidBody.velocity.x/15 + WalkingDirection.x, enemyRigidBody.velocity.y);
    }

    public void FreeMovement()
    {
        // powolne losowe poruszanie się 
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
