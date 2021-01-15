using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public int minDamage = 20;
    public int maxDamage = 40;
    public int criticalDamage = 50;
    public float criticalChancePercentage = 25f;

    public int Damage()
    {
        int dmgValue = 0;
        int amountOfPossibilities = (int)(100 / criticalChancePercentage);
        int[] percentagePossibilities = new int[amountOfPossibilities];
        for (int i = 0; i < amountOfPossibilities; i++)
        {
            percentagePossibilities[i] = i;
        }

        int percentageChanceValue = Random.Range(percentagePossibilities[0], percentagePossibilities[percentagePossibilities.Length - 1]);
        if (percentageChanceValue == (int)amountOfPossibilities / 2)
            dmgValue = criticalDamage;
        else dmgValue = Random.Range(minDamage, maxDamage);

        return dmgValue;
    }
}
