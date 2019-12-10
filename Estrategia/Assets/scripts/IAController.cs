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


    }

    public override void CreateUnit(UnitType type)
    {
        if (gameController.turnOfPlayer() != player) return;


    }

    public override void MoveUnit(Unit unit, int newX, int newZ)
    {
        if (gameController.turnOfPlayer() != player) return;


    }
}
