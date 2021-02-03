using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleCombat : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator statusIndicator;
    public GameObject floatingPoints;
    public Animator animator;
    public Eagle eagle;

    public ParticleSystem hurtEffectPS;
    public Color bloodColor;

    CameraShake camShake;
    float camShakeAmount = 0.02f;

    public Transform attackPoint;

    public LayerMask enemyLayers;

    public float attackRange = 0.1f;
    public DamageSystem damageSystem;

    private bool isCoroutineRecoilExecuting = false;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    public int maxHealth = 100;
    int _currentHealth;
    public int currentHealth
    {
        get { return _currentHealth; }
        set
        {
            int valueBefore = _currentHealth;
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(valueBefore > _currentHealth ? valueBefore - _currentHealth : _currentHealth - valueBefore
                , valueBefore > _currentHealth ? "" : "+");

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

    void Start()
    {
        currentHealth = maxHealth;
        camShake = Game.game.GetComponent<CameraShake>();
        damageSystem = GetComponent<DamageSystem>();

        OnKilled += () =>
        {
            //StartCoroutine(Die());
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
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        //animator.SetBool("IsAttacking", true);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D player in hitEnemies)
        {
            hurtEffectPS.startColor = player.GetComponent<PlayerCombat>().bloodColor;
            StartCoroutine(DamageEffect(0.42f, player.GetComponent<PlayerCombat>()));
            break;
        }
    }

    IEnumerator DamageEffect(float time, PlayerCombat playerCombat)
    {
        camShake.Shake(camShakeAmount, time, 0.2f);
        UnityEngine.Vector3 firstPosition = attackPoint.position;
        yield return new WaitForSeconds(time);
        playerCombat.TakeDamage(damageSystem.Damage(), GetComponent<Eagle>().RotationValue == 0 ? 1 : -1);
        var hurtPS = Instantiate(hurtEffectPS);
        hurtPS.transform.position = firstPosition;
        hurtPS.Play();
        yield return new WaitForSeconds(time);
        hurtPS.transform.position = attackPoint.position;
    }

    public void TakeDamage(int damage, float recoilDirection)
    {
        if (currentHealth > 0)
        {
            StartCoroutine(RecoilWithDelay(damage, recoilDirection));
        }

        //if (statusIndicator != null)
        //{
        //    statusIndicator.SetHealth(currentHealth, maxHealth);
        //}

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator RecoilWithDelay(int damage, float recoilDirection)
    {
        if (isCoroutineRecoilExecuting)
            yield break;

        isCoroutineRecoilExecuting = true;
        yield return new WaitForSeconds(0.2f);
        currentHealth -= damage;

        GetComponent<Rigidbody2D>().AddForce(new UnityEngine.Vector2(recoilDirection * (isDead ? 0.5f : 30), 5), ForceMode2D.Impulse);
        isCoroutineRecoilExecuting = false;
    }

    IEnumerator Die()
    {
        isDead = true;      
        this.enabled = false;

        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger("Dead");
        gameObject.GetComponent<Eagle>().enabled = false;
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = false;

    }

    private void FloatingPoints(int value, string additionalText = "")
    {
        GameObject points = Instantiate(floatingPoints, new UnityEngine.Vector3(transform.position.x, transform.position.y + GetComponent<BoxCollider2D>().size.y / 2), UnityEngine.Quaternion.identity, gameObject.transform);
        points.transform.GetChild(0).GetComponent<TextMesh>().text = additionalText + value.ToString();
        points.GetComponentInChildren<TextMesh>().color = Color.red;
    }

}
