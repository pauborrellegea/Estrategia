using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAController : Player
{
    private void Start()
    {
        player = false;
    }

    public override void Attack(Unit attacker, int x, int z)
    {
        if (gameController.turnOfPlayer() != player) return; //no deberian ser necesarias, en teoria


        gridController.GenerateIAAttackInfluence(); //solo si ataca
    }

    public override void CreateUnit(UnitType type)
    {
        if (gameController.turnOfPlayer() != player) return;

        int cost = gameController.spawnableUnits[(int)type].coste;
        if (coins >= cost)
        {
            if (CanSpawn())
            {
                Unit newUnit = Instantiate(gameController.spawnableUnits[(int)type], new Vector3(spawnX, 0f, spawnZ), transform.rotation, transform) as Unit;
                newUnit.SetPlayer(player);

                gridController.AddUnit(newUnit, spawnX, spawnZ);
                SubstractCoins(cost);
            }
        }
    }

    public override void MoveUnit(Unit unit, int newX, int newZ)
    {
        if (gameController.turnOfPlayer() != player) return;


    }

    public override void resetEndTurn()
    {
        
    }

    public bool HasCoinsFor(UnitType type)
    {
        return gameController.spawnableUnits[(int)type].coste <= coins;
    }

    public bool HasCoinsForAttack()
    {
        return coins >= attackCost;
    }

    public bool isTurn()
    {
        return gameController.turnOfPlayer() == player;
    }

    public bool CanSpawn()
    {
        return gridController.CanSpawnUnit(spawnX, spawnZ);
    }
}
