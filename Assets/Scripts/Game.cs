using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game game;

    private void Awake()
    {
        if(game == null)
            game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    void Start()
    {
        
    }

    public int DamageSystem(int minDamage, int maxDamage, int criticalDamage, float criticalChancePercentage)
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

    public void KeyRaised()
    {
        Debug.Log("You failed");
    }

    public void PlayerDead()
    {
        Debug.Log("You win");
    }
}
