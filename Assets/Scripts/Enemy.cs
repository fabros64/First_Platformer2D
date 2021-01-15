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
    public GameObject floatingPoints;
    public ParticleSystem hurtEffectPS;

    CameraShake camShake;
    float camShakeAmount = 0.01f;

    public int maxHealth = 100;
    int _currentHealth;
    public int currentHealth
        {
            get { return _currentHealth;}
            set { _currentHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

    DamageSystem damageSystem;
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

    bool isDead = false;

    public int recoilForce = 100;
    int basicRecoilForce;

    public Color bloodColor = Color.white;
    public bool IsFollowing { get; set; } = false;

    [Header("Optional: ")]
    [SerializeField]
    private StatusIndicator statusIndicator;

    void Start()
    {
        currentHealth = maxHealth;
        if(statusIndicator != null)
        {
            statusIndicator.SetHealth(currentHealth, maxHealth);
        }
        nextAttackTime = Time.time + Random.Range(10f / attackRate, 25f / attackRate);
        enemyRigidBody = GetComponent<Rigidbody2D>();
        attackDistance = Vector2.Distance(transform.position, attackPoint.transform.position)
            + attackRange;
        flipDirection = 0;
        jumpDirection = -1;
        WalkingDirection = new Vector3(0, enemyRigidBody.velocity.y);
        basicRecoilForce = recoilForce;

        camShake = Game.game.GetComponent<CameraShake>();
        damageSystem = GetComponent<DamageSystem>();
    }

    void FixedUpdate()
    {
        if (IsFollowing)
            recoilForce = basicRecoilForce;
        if (!IsFollowing)
            recoilForce = 30;

        jumpDirection = flipDirection == 0 ? (-1) : 1;
        

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(Jump(0.5f));
            StartCoroutine(Attack());            
            nextAttackTime = Time.time + Random.Range(10f / attackRate, 25f / attackRate);
        }
    }

    IEnumerator Jump(float time)
    {
        Collider2D[] hitObstacles = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, obstacleLayers);
        if(hitObstacles.Length > 0)
        {
            foreach(Collider2D obstacle in hitObstacles)
            {
                if (obstacle.gameObject == gameObject && hitObstacles.Length == 1)
                    yield break;
            }

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
                    hurtEffectPS.startColor = player.GetComponent<PlayerCombat>().bloodColor;
                    StartCoroutine(DamageEffect(0.15f));
                    player.GetComponent<PlayerCombat>().TakeDamage(damageSystem.Damage(), jumpDirection);
                    break;
                }
            }
        }        
    }

    IEnumerator DamageEffect(float time)
    {
        camShake.Shake(camShakeAmount, time, 0.1f);
        UnityEngine.Vector3 firstPosition = attackPoint.position;
        yield return new WaitForSeconds(time);
        var hurtPS = Instantiate(hurtEffectPS);
        hurtPS.transform.position = firstPosition;
        hurtPS.Play();
        yield return new WaitForSeconds(time);
        hurtPS.transform.position = attackPoint.position;
    }

    public void TakeDamage(int damage, float time)
    {
        if (currentHealth > 0)
        {
            animator.SetTrigger("Hurt");
            currentHealth -= damage;
            StartCoroutine(RecoilWithDelay(damage, time));
        }

        if (statusIndicator != null)
        {
            statusIndicator.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator RecoilWithDelay(int damage, float time)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(time);        
        GameObject points = Instantiate(floatingPoints, new Vector3(transform.position.x, transform.position.y + GetComponent<CapsuleCollider2D>().size.y/2), Quaternion.identity, gameObject.transform);
        points.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();

        enemyRigidBody.AddForce(new Vector2(jumpDirection * (-1) * (isDead || !IsFollowing ? 30f : recoilForce * 10), recoilForce/3), ForceMode2D.Impulse);       
        isCoroutineRecoilExecuting = false;
    }

    IEnumerator Die()
    {        
        animator.SetBool("IsDead", true);
        isDead = true;
        
        gameObject.GetComponentInChildren<Detector>().enabled = false;
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = false;
        IsFollowing = false;

        this.enabled = false;

        yield return new WaitForSeconds(0.3f);
        GetComponent<CapsuleCollider2D>().enabled = false;

        //GetComponent<Rigidbody2D>().simulated = false;       

        // yield return new WaitForSeconds(5f);
        // gameObject.SetActive(false);
    }

    void SetProperDirection()
    {
        if ((transform.position.x - player.position.x) > attackDistance)
        {
            flipDirection = 0;
            WalkingDirection = Vector3.right * (-1);

            if(enemyRigidBody.velocity.magnitude != 0)
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

            if (enemyRigidBody.velocity.magnitude != 0)
                animator.SetBool("FollowingPlayer", true);
        }

        if (enemyRigidBody.velocity.magnitude == 0)
            animator.SetBool("FollowingPlayer", false);

        WalkingDirection = WalkingDirection.normalized * WalkingSpeed;
        WalkingDirection.x *= Time.fixedDeltaTime * 5;
        transform.rotation = Quaternion.Euler(Vector3.up * flipDirection);
    }

    public void FollowPlayer()
    {

            SetProperDirection();
            enemyRigidBody.velocity = new Vector3(enemyRigidBody.velocity.x / 15 + WalkingDirection.x, enemyRigidBody.velocity.y);
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
