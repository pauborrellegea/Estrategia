using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Player player;
    public Player ia;

    public Unit[] spawnableUnits;

    private float timePerTurn = 2f;
    private float ticks;

    private int turnsElapsed;

    private int startCoins = 30;
    public int coinsPerTurn; //cada turno la recompensa aumenta


    private bool playerTurn; //de quien es el turno (true=player, false=ia)

    GridController gridController;

    private void Awake()
    {
        ticks = 0f;
        gridController = GetComponent<GridController>();
        playerTurn = true; //aleatorio?

        coinsPerTurn = 20;

        turnsElapsed = 0;
    }

    private void Start()
    {
        player.AddCoins(startCoins);
        ia.AddCoins(startCoins);

        player.setOtherBase(0, gridController.cols - 1);
        ia.setOtherBase(gridController.rows - 1, 0);

        player.setSpawn(gridController.rows - 2, 1);
        ia.setSpawn(1, gridController.cols - 2);
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
            EndTurn();
        }
    }
    
    public void EndTurn()
    {
        turnsElapsed += 1;
        if (turnsElapsed >= 2)
        {
            coinsPerTurn += 2;
            turnsElapsed -= 2;
        }

        playerTurn = !playerTurn;
        ticks = 0f;
        if (playerTurn)
        {
            player.AddCoins(coinsPerTurn);
            ia.resetEndTurn();
        }
        else
        {
            ia.AddCoins(coinsPerTurn);
            player.resetEndTurn();
        }
    }

    public void AttackBase(int amount, bool p)
    {
        if (p)
        {
            ia.baseAttacked(amount);
        }
        else
        {
            player.baseAttacked(amount);
        }
    }
}
