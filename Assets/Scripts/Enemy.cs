﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;
    public LayerMask playerLayer;

    public int maxHealth = 100;
    int currentHealth;

    public int attackDamage = 10;
    public float attackRange = 0.1f;

    public float attackRate = 10f;
    float nextAttackTime;

    int triggerCounter;
    public int numberOfPlayerColliders;

    void Start()
    {
        currentHealth = maxHealth;
        nextAttackTime = Time.time + Random.Range(10f / attackRate, 40f / attackRate);
        triggerCounter = 0;
        numberOfPlayerColliders = 2;
    }

    void Update()
    {
        if(Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
            nextAttackTime = Time.time + Random.Range(10f / attackRate, 20f / attackRate);
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
                    break;
                }
            }
        }        
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggerCounter++;
        if (triggerCounter % numberOfPlayerColliders == 0)
        {
            animator.SetTrigger("NoticePlayer");

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {

    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
