using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int AverageHealValue = 20;
    int minHealValue;
    int maxHealValue;

    void Start()
    {
        minHealValue = AverageHealValue - 10;
        maxHealValue = AverageHealValue + 10;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerCombat playerCombat;
        bool isPlayer = collision.gameObject.TryGetComponent<PlayerCombat>(out playerCombat);

        if(isPlayer)
        {
            int healValue = Random.Range(minHealValue, maxHealValue);
            if (playerCombat.currentHealth < playerCombat.maxHealth)
            {
                playerCombat.currentHealth += healValue;
                Destroy(gameObject);
            }
        }
    }

}
