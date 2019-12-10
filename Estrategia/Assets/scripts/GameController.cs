using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Player player;
    public Player ia;

    public Unit[] spawnableUnits;

    private float timePerTurn = 20f;
    private float ticks;

    public int coinsPerTurn = 20;


    private bool playerTurn; //de quien es el turno (true=player, false=ia)

    GridController gridController;

    private void Awake()
    {
        ticks = 0f;
        gridController = GetComponent<GridController>();
        playerTurn = true; //aleatorio?
    }

    private void Start()
    {
        player.AddCoins(30);
        ia.AddCoins(30);
        player.setSpawn(gridController.rows - 1, 0);
        ia.setSpawn(0, gridController.cols - 1);
    }

    public bool turnOfPlayer()
    {
        return playerTurn;
    }

    private void FixedUpdate()
    {
        ticks += Time.fixedDeltaTime;

        if (ticks >= timePerTurn)
        {
            playerTurn = !playerTurn;
            ticks -= timePerTurn;
        }
    }
    
    public void EndTurn()
    {
        playerTurn = !playerTurn;
        ticks = 0f;
        if (playerTurn)
        {
            player.AddCoins(coinsPerTurn);
        }
        else
        {
            ia.AddCoins(coinsPerTurn);
        }
    }
}
