using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Player player;

    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange = 0.1f;
    public int attackDamage = 40;

    public int attackForce = 10;
    int basicAttackForce;
    bool attacking;
    int recoilDirection;
    private bool isCoroutineRecoilExecuting = false;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    public int maxHealth = 100;
    int currentHealth;

    Rigidbody2D currentEnemy;

    UnityEngine.Vector2 recoilForce;

    public bool IsBeignFollowed { get; set; } = false;

    private void Start()
    {
        currentHealth = maxHealth;
        attacking = false;
        recoilDirection = player.RotationValue == 0 ? 1 : (-1);
        recoilForce = new UnityEngine.Vector2(attackForce * recoilDirection * 10, attackForce / 2);
        basicAttackForce = attackForce;
    }

    void Update()
    {
        if (IsBeignFollowed)
            attackForce = basicAttackForce;
        else if (!IsBeignFollowed)
            attackForce = 3;

        recoilDirection = player.RotationValue == 0 ? 1 : (-1);

        if (Time.time >= nextAttackTime)
        {
            animator.SetBool("IsAttacking", false);
            if (Input.GetKeyDown(KeyCode.I))
            {
                Attack1();
                nextAttackTime = Time.time + 1f / attackRate;
            }          
        }       
    }

    private void FixedUpdate()
    {
        recoilForce = new UnityEngine.Vector2(attackForce * recoilDirection * 10, attackForce / 3);
        if (attacking)
            {
                StartCoroutine(RecoilWithDelay(0.2f));
            }
    }

    IEnumerator RecoilWithDelay(float time)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(time);
        currentEnemy?.AddForce(recoilForce, ForceMode2D.Impulse);
        attacking = false;
        isCoroutineRecoilExecuting = false;
    }

    void Attack1()
    {
        animator.SetTrigger("Attack1");
        animator.SetBool("IsAttacking", true);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            currentEnemy = enemy.GetComponent<Rigidbody2D>();
            attacking = true;
            break;
        }
    }

    public void TakeDamage(int damage, ref bool isDead)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            animator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Die());           
        }
    }

    IEnumerator Die()
    {
        animator.SetTrigger("Dead");
        GetComponent<Player>().enabled = false;
        this.enabled = false;
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody2D>().simulated = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
