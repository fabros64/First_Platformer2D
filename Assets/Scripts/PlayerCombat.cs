using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField]
    private StatusIndicator statusIndicator;

    public Animator animator;
    public Player player;

    public ParticleSystem hurtPS;

    CameraShake camShake;
    float camShakeAmount = 0.02f;

    public Transform attackPoint;
    public GameObject fireBall;
    public LayerMask fireBallHitLayers;
    public LayerMask enemyLayers;

    public float attackRange = 0.1f;
    public int attackDamage = 40;

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
        set { _currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    bool isDead = false;

    public bool IsBeignFollowed { get; set; } = false;  // ? to remove
    public List<GameObject> EnemiesFollowed { get; set; } = new List<GameObject>();  // ? to remove

    private void Start()
    {
        currentHealth = maxHealth;
        camShake = Game.game.GetComponent<CameraShake>();

        if (statusIndicator != null)
        {
            statusIndicator.SetHealth(currentHealth, maxHealth);
        }
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
        
        if(Time.time >= nextSpellTime)
        {
            animator.SetBool("IsCastingSpell", false);
            if (Input.GetKeyDown(KeyCode.O))
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
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage, 0.2f);
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
            currentHealth -= damage;
            animator.SetTrigger("Hurt");
            StartCoroutine(RecoilWithDelay(0.2f, recoilDirection));
        }

        if (statusIndicator != null)
        {
            statusIndicator.SetHealth(currentHealth, maxHealth);
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
