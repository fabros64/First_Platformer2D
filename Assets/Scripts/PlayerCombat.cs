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
    bool attacking;
    int recoilDirection;
    private bool isCoroutineRecoilExecuting = false;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    public int maxHealth = 100;
    int currentHealth;

    Rigidbody2D currentEnemy;

    private void Start()
    {
        currentHealth = maxHealth;
        attacking = false;
        recoilDirection = player.RotationValue == 0 ? 1 : (-1);
    }

    void Update()
    {
        recoilDirection = player.RotationValue == 0 ? 1 : (-1);
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Attack1();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }       
    }

    private void FixedUpdate()
    {
        if(attacking)
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
        currentEnemy?.AddForce(new UnityEngine.Vector2(attackForce * recoilDirection * 10, attackForce / 2), ForceMode2D.Impulse);
        attacking = false;
        isCoroutineRecoilExecuting = false;
    }

    void Attack1()
    {
        animator.SetTrigger("Attack1");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            currentEnemy = enemy.GetComponent<Rigidbody2D>();
            attacking = true;
            break;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
            
        }
    }

    void Die()
    {
        animator.SetTrigger("Dead");
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Player>().enabled = false;
        this.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
