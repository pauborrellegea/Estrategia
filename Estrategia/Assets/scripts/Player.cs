using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    [HideInInspector]public bool player;
    /*[HideInInspector]*/public int coins = 0;

    //EL JUGADOR DEBE ESTAR EN LA POSICION DE SU SPAWN POINT
    [HideInInspector]public int spawnX, spawnZ;

    [HideInInspector]public GameController gameController;
    [HideInInspector]public GridController gridController;

    private void Awake()
    {
        //falta ponerlo
        GameObject juego = GameObject.Find("Juego");
        gameController = juego.GetComponent<GameController>();
        gridController = juego.GetComponent<GridController>();

        spawnX = (int)transform.position.x;
        spawnZ = (int)transform.position.z;
    }

    abstract public void MoveUnit(Unit unit, int newX, int newZ);

    abstract public void Attack(Unit attacker, int x, int z);

    abstract public void CreateUnit(UnitType type);

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void SubstractCoins(int amount)
    {
        coins -= amount;
    }
}
