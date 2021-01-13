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


    void Update()
    {
        
    }
}
