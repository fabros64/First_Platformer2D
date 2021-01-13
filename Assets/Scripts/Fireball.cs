using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    Animator animator;

    int directionShift;

    void Start()
    {
        animator = GetComponent<Animator>();
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
            enemy.TakeDamage(50, 0f);

        directionShift = collision.gameObject.transform.position.x - gameObject.transform.position.x > 0 ? 1 : (-1);
        
        StartCoroutine(Explosion(0.28f));
        GetComponent<EdgeCollider2D>().enabled = false;
    }

    IEnumerator Explosion(float time)
    {
        animator.SetTrigger("Hit");
        gameObject.GetComponent<Rigidbody2D>().velocity = UnityEngine.Vector2.zero;
        gameObject.transform.position = new UnityEngine.Vector3(gameObject.transform.position.x + (0.4f * directionShift), gameObject.transform.position.y);
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}
