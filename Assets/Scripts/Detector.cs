using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Enemy enemy;
    public Transform player;

    int triggerCounter;
    public int numberOfPlayerColliders;

    bool following;
    int flipDirection;

    void Start()
    {
        triggerCounter = 0;
        numberOfPlayerColliders = 2;
        following = false;
        flipDirection = 0;
    }

    private void FixedUpdate()
    {
        if (following)
            FollowPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerCounter++;
        }

        if (triggerCounter % numberOfPlayerColliders == 0 && triggerCounter > 0)
        {
            enemy.animator.SetTrigger("NoticePlayer");
            enemy.animator.SetBool("FollowingPlayer", true);
            following = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        enemy.animator.SetBool("FollowingPlayer", false);
    }

    void SetProperDirection()
    {
        if ((enemy.transform.position.x - player.position.x) > 0)
            flipDirection = 0;
        else 
            flipDirection = 180;

        enemy.transform.rotation = Quaternion.Euler(Vector3.up * flipDirection);
    }

    void FollowPlayer()
    {
        SetProperDirection();


    }
}
