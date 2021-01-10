using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Enemy enemy;   
    public Transform attackPoint;
    public GameObject alert;
    public GameObject player;

    PlayerCombat playerCombat;

    int triggerCounter;
    public int numberOfPlayerColliders;

    bool following;

    void Start()
    {
        triggerCounter = 0;
        numberOfPlayerColliders = 2;
        following = false;
        playerCombat = player.GetComponent<PlayerCombat>();
    }

    private void FixedUpdate()
    {
        enemy.IsFollowing = following;

        if (following)
        {
            enemy.FollowPlayer();
        }      
        else if(!following)
        {
            enemy.animator.SetBool("FollowingPlayer", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerCounter++;

            if (triggerCounter % numberOfPlayerColliders == 0 && triggerCounter > 0)
            {
                playerCombat.EnemiesFollowed.Add(gameObject);
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
        following = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerCounter--;

                playerCombat.EnemiesFollowed.Remove(gameObject);
                StartCoroutine(StopFollowWithDelay());
        }
    } 
}
