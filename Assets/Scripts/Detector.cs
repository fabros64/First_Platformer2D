using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Enemy enemy;   
    public Transform attackPoint;
    public GameObject alert;

    int triggerCounter;
    public int numberOfPlayerColliders;

    bool following;    

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
                alert.GetComponent<SpriteRenderer>().enabled = true;
                StartCoroutine(FollowWithDelay());               
            }
        }
    }

    IEnumerator FollowWithDelay()
    {
        yield return new WaitForSeconds(1f);
        alert.GetComponent<SpriteRenderer>().enabled = false;
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
