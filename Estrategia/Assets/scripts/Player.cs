using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public abstract class Player : MonoBehaviour
{
    [HideInInspector]public bool player;
    [HideInInspector]public int coins = 0;

    public GameObject attackPrefab;

    public int baseHP = 100;
    [HideInInspector] public int otherBaseX, otherBaseZ = -1;

    //EL JUGADOR DEBE ESTAR EN LA POSICION DE SU SPAWN POINT
    [HideInInspector]public int spawnX, spawnZ;

    [HideInInspector]public static GameController gameController;
    [HideInInspector]public static GridController gridController;

    [HideInInspector] public static int attackCost = 3;

    private void Awake()
    {
        //falta ponerlo
        GameObject juego = GameObject.Find("Juego");
        gameController = juego.GetComponent<GameController>();
        gridController = juego.GetComponent<GridController>();
    }

    public void setSpawn(int x, int z)
    {
        spawnX = x;
        spawnZ = z;
    }

    abstract public void MoveUnit(Unit unit, int newX, int newZ);

    abstract public void Attack(Unit attacker, int x, int z);

    abstract public void CreateUnit(UnitType type);

    abstract public void resetEndTurn();

    public void setOtherBase(int x, int z)
    {
        otherBaseX = x;
        otherBaseZ = z;
    }

    [Panda.Task]
    public void EndTurn()
    {
        if (gameController.turnOfPlayer() != player) return;

        gameController.EndTurn();
    }

    public virtual void AddCoins(int amount)
    {
        coins += amount;
    }

    public virtual void SubstractCoins(int amount)
    {
        coins -= amount;
    }

    public void baseAttacked(int amount)
    {
        baseHP -= amount;

        if (baseHP < 0)
        {
            gameController.LoseGame(player);
        }
    }
}
