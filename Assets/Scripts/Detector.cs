using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Enemy enemy;   
    public Transform attackPoint;

    int triggerCounter;
    public int numberOfPlayerColliders;

    bool following;
    

    public float WalkingSpeed = 1f;
    Vector3 WalkingDirection;

    Rigidbody2D enemyRigidBody;

    float attackDistance;

    void Start()
    {
        triggerCounter = 0;
        numberOfPlayerColliders = 2;
        following = false;        
    }

    private void FixedUpdate()
    {
        if (following)
            enemy.FollowPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerCounter++;

            if (triggerCounter % numberOfPlayerColliders == 0 && triggerCounter > 0)
            {
                enemy.animator.SetTrigger("NoticePlayer");               
                StartCoroutine(FollowWithDelay());               
            }
        }
    }

    IEnumerator FollowWithDelay()
    {
        yield return new WaitForSeconds(1f);
        enemy.animator.SetBool("FollowingPlayer", true);
        following = true;
    }

    IEnumerator StopFollowWithDelay()
    {
        yield return new WaitForSeconds(1f);
        enemy.animator.SetBool("FollowingPlayer", false);
        following = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerCounter--;            
            StartCoroutine(StopFollowWithDelay());
        }
    }

    
}
