using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Player player;
    public Player ia;

    public Unit[] spawnableUnits;


    private bool playerTurn; //de quien es el turno

    GridController gridController;

    private void Awake()
    {
        gridController = GetComponent<GridController>();
        playerTurn = true; //aleatorio?
    }

    private void Start()
    {
        player.AddCoins(30);
        ia.AddCoins(30);
    }

    public bool turnOfPlayer()
    {
        return playerTurn;
    }

    //turnos
}
