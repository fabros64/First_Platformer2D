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
    private bool isCoroutineRecoilExecuting = false;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    public int maxHealth = 100;
    int currentHealth;

    bool isDead = false;

    public bool IsBeignFollowed { get; set; } = false;
    public List<GameObject> EnemiesFollowed { get; set; } = new List<GameObject>();

    private void Start()
    {
        currentHealth = maxHealth;
        basicAttackForce = attackForce;
    }

    void Update()
    {
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
        if (IsBeignFollowed && EnemiesFollowed.Count > 0)
            attackForce = basicAttackForce;
        if (!IsBeignFollowed || EnemiesFollowed.Count == 0)
            attackForce = 3;
    }

    void Attack1()
    {
        animator.SetTrigger("Attack1");
        animator.SetBool("IsAttacking", true);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            break;
        }
    }

    public void TakeDamage(int damage, float recoilDirection)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            animator.SetTrigger("Hurt");
            StartCoroutine(RecoilWithDelay(0.2f, recoilDirection));
        }

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Die());           
        }
    }
    IEnumerator RecoilWithDelay(float time, float recoilDirection)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(time);
        player.GetComponent<Rigidbody2D>().AddForce(new UnityEngine.Vector2(recoilDirection * (isDead ? 0.5f : 30), 5), ForceMode2D.Impulse);
        isCoroutineRecoilExecuting = false;
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
