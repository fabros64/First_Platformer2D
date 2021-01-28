using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireBallArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy;
        if (collision.gameObject.TryGetComponent<Enemy>(out enemy))
        {
            GetComponentInParent<PlayerCombat>().enemyInArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy;
        if (collision.gameObject.TryGetComponent<Enemy>(out enemy))
        {
            GetComponentInParent<PlayerCombat>().enemyInArea = false;
        }
    }
}
