using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSystem : MonoBehaviour
{
    [SerializeField]
    private DropItem[] dropObjects;
    
    public void Drop()
    {
        if(dropObjects != null && dropObjects.Length > 0)
        {
            int whichItem = Random.Range(0, dropObjects.Length - 1);

            int amountOfPossibilities = (int)(100 / dropObjects[whichItem].dropProbabilityPercentage);
            int[] percentagePossibilities = new int[amountOfPossibilities];
            for (int i = 0; i < amountOfPossibilities; i++)
            {
                percentagePossibilities[i] = i;
            }
            int percentageChanceValue = Random.Range(percentagePossibilities[0], percentagePossibilities[percentagePossibilities.Length - 1]);

            if (percentageChanceValue == (int)amountOfPossibilities / 2)
            {
                DropItem dropItem = Instantiate(dropObjects[whichItem], GetComponent<Enemy>().transform);
            }    
        }
    }
}
