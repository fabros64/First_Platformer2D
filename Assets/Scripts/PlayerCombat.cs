﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator statusIndicator;
    public GameObject floatingPoints;
    public Animator animator;
    public Player player;

    public ParticleSystem hurtPS;
    public Color bloodColor;

    CameraShake camShake;
    float camShakeAmount = 0.02f;

    public Transform attackPoint;
    public GameObject fireBall;
    public LayerMask fireBallHitLayers;
    public LayerMask enemyLayers;

    public float attackRange = 0.1f;
    public DamageSystem damageSystem;

    private bool isCoroutineRecoilExecuting = false;

    public float attackRate = 2f;
    float nextAttackTime = 0f;
    public float spellRate = 1f;
    float nextSpellTime = 0f;

    public float fireBallSpeed = 10f;

    public int maxHealth = 100;
    int _currentHealth;
    public int currentHealth
    {
        get { return _currentHealth; }
        set 
        {
            int valueBefore = _currentHealth;
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(valueBefore > _currentHealth ? valueBefore-_currentHealth:_currentHealth-valueBefore
                , valueBefore > _currentHealth ? "":"+");

            if (currentHealth <= 0 && !isDead)
            {
                OnKilled?.Invoke();
            }
        }
    }

    public event Action<int, string> OnHealthChanged;
    public event Action OnKilled;

    bool takingDamage = false;
    bool isDead = false;


    private void Start()
    {
        currentHealth = maxHealth;
        camShake = Game.game.GetComponent<CameraShake>();
        damageSystem = GetComponent<DamageSystem>();

        OnKilled += () =>
        {
            StartCoroutine(Die());
        };

        statusIndicator?.SetHealth(currentHealth, maxHealth);

        OnHealthChanged += (value, additionalStr) =>
        {
            statusIndicator?.SetHealth(currentHealth, maxHealth);
            FloatingPoints(value, additionalStr);
        };
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            animator.SetBool("IsAttacking", false);
            if (Input.GetKeyDown(KeyCode.I) && !takingDamage)
            {
                Attack1();
                nextAttackTime = Time.time + 1f / attackRate;
            }          
        }      
        
        if(Time.time >= nextSpellTime)
        {
            animator.SetBool("IsCastingSpell", false);
            if (Input.GetKeyDown(KeyCode.O) && !takingDamage)
            {
                StartCoroutine(CastSpell(0.2f));
                nextSpellTime = Time.time + attackRate / spellRate;
            }
        }
    }

    private void FixedUpdate()
    {

    }

    IEnumerator CastSpell(float time)
    {
        animator.SetTrigger("CastSpell");
        animator.SetBool("IsCastingSpell", true);
        
        int direction = gameObject.transform.rotation.y == 0 ? 1 : (-1);

        float x_RelativeToPlayer = 0.49f * direction;
        float y_RelativeToPlayer = (-0.065f) * direction;

        yield return new WaitForSeconds(time);
        var fireball = Instantiate(fireBall);
        fireball.transform.rotation = gameObject.transform.rotation;
        fireball.transform.position = new UnityEngine.Vector3(gameObject.transform.position.x + x_RelativeToPlayer, gameObject.transform.position.y + y_RelativeToPlayer, 0);
        fireball.GetComponent<Rigidbody2D>().velocity = UnityEngine.Vector2.right * direction * fireBallSpeed;
    }


    void Attack1()
    {
        animator.SetTrigger("Attack1");
        animator.SetBool("IsAttacking", true);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            hurtPS.startColor = enemy.GetComponent<Enemy>().bloodColor;
            StartCoroutine(DamageEffect(0.15f));
            enemy.GetComponent<Enemy>().TakeDamage(damageSystem.Damage(), 0.2f);
            break;
        }
    }



    IEnumerator DamageEffect(float time)
    {
        camShake.Shake(camShakeAmount, time, 0.1f);       
        UnityEngine.Vector3 firstPosition = hurtPS.gameObject.transform.position;
        yield return new WaitForSeconds(time);
        hurtPS.transform.position = firstPosition;
        hurtPS.Play();
        yield return new WaitForSeconds(time);
        hurtPS.transform.position = attackPoint.position;
    }

    public void TakeDamage(int damage, float recoilDirection)
    {
        if (currentHealth > 0)
        {
            takingDamage = true;
            StartCoroutine(RecoilWithDelay(0.2f, recoilDirection, damage));
        }
    }

    IEnumerator RecoilWithDelay(float time, float recoilDirection, int damage)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(time);
        animator.SetTrigger("Hurt");
        currentHealth -= damage;
        player.GetComponent<Rigidbody2D>().AddForce(new UnityEngine.Vector2(recoilDirection * (isDead ? 0.5f : 30), 5), ForceMode2D.Impulse);
        takingDamage = false;
        isCoroutineRecoilExecuting = false;
    }

    private void FloatingPoints(int value, string additionalText = "")
    {
        GameObject points = Instantiate(floatingPoints, new UnityEngine.Vector3(transform.position.x, transform.position.y + GetComponent<BoxCollider2D>().size.y / 2), UnityEngine.Quaternion.identity, gameObject.transform);
        points.transform.GetChild(0).GetComponent<TextMesh>().text = additionalText + value.ToString();
        points.GetComponentInChildren<TextMesh>().color = Color.green;
    }

    IEnumerator Die()
    {
        isDead = true;
        GetComponent<Player>().enabled = false;
        this.enabled = false;
        yield return new WaitForSeconds(0.4f);
        animator.SetTrigger("Dead");
        yield return new WaitForSeconds(0.4f);
        GetComponent<Rigidbody2D>().simulated = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
