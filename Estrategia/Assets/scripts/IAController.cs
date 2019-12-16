using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using Panda;

public class IAController : Player
{

    private void Start()
    {
        player = false;
    }

    public override void Attack(Unit attacker, int x, int z)
    {
        if (gameController.turnOfPlayer() != player) return; //no deberian ser necesarias, en teoria

        if (gridController.IA_CanAttack(attacker, x, z) && HasCoinsForAttack())
        {
            if (x == otherBaseX && z == otherBaseZ)
            {
                gameController.AttackBase(attacker.ataque, player);
            }
            else
            {
                Unit attackedUnit = gridController.GetUnit(x, z);
                if (attackedUnit != null)
                {
                    attackedUnit.ReceiveDamage(attacker.ataque);
                    if (attackedUnit.IsDead())
                    {
                        gridController.RemoveUnit(attackedUnit, x, z);
                    }
                }
            }

            SubstractCoins(attackCost);
            attacker.Attacked();

            GameObject att = Instantiate(attackPrefab, transform);
            att.GetComponent<AttackAnimation>().setObjectives(attacker.transform.position, new Vector3(x, 0, z));
        }


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

        //pathfinding

        int initX = (int)unit.transform.position.x;
        int initZ = (int)unit.transform.position.z;

        //this.StartCoroutineAsync(Dijkstra(newX, newZ, initX, initZ));
        List<SingleMove>  listMoves = Dijkstra(newX, newZ, initX, initZ);

        //mover hasta donde se pueda (de 1 en 1)
        int possibleMoves = Mathf.Min(unit.remainingMoves, coins, listMoves.Count);
        int x = initX;
        int z = initZ;

        for (int i = 0; i<listMoves.Count; i++)
        {
            Debug.Log(listMoves[i].x + " " + listMoves[i].z);
        }

        if (possibleMoves > 0)
        {
            x = listMoves[listMoves.Count - possibleMoves].x;
            z = listMoves[listMoves.Count - possibleMoves].z;

            gridController.MoveUnit(unit, x, z);
            SubstractCoins(possibleMoves);
            unit.substractMoves(possibleMoves);
        }
    }

    public override void resetEndTurn()
    {
        //nada que resetear
    }

    public bool HasCoinsFor(UnitType type)
    {
        return gameController.spawnableUnits[(int)type].coste <= coins;
    }

    public bool HasCoinsForAnyUnit()
    {
        return coins >= 40;
    }

    public bool HasCoinsForAttack()
    {
        return coins >= attackCost;
    }

    [Panda.Task]
    public bool isTurn()
    {
        return gameController.turnOfPlayer() == player;
    }
    
    public bool CanSpawn()
    {
        return gridController.CanSpawnUnit(spawnX, spawnZ);
    }

    public float ExploreMultiplier()
    {
        return 15f / gridController.explorationImportance;
    }

    public void GetBestPosition(ref SingleMove[,] moves, ref int outX, ref int outZ)
    {
        float bestCost = Mathf.Infinity;

        for (int x = 0; x < gridController.rows; x++)
        {
            for (int z = 0; z < gridController.cols; z++)
            {
                if (moves[x, z] != null)
                {
                    float c = gridController.TacticMoveCost(x, z);
                    if (c < bestCost)
                    {
                        bestCost = c;
                        outX = x;
                        outZ = z;
                    }
                }
            }
        }
    }


    public List<SingleMove> Dijkstra(int objX, int objZ, int initX, int initZ)
    {
        List<SingleMove> queue = new List<SingleMove>();

        SingleMove[,] moves = new SingleMove[gridController.rows, gridController.cols];

        moves[initX, initZ] = new SingleMove(0f, null, initX, initZ);
        queue.Add(moves[initX, initZ]);

        int x, z;
        SingleMove lastMove = null;

        while (queue.Count > 0)
        {
            lastMove = getMin(ref queue);
            queue.Remove(lastMove);
            x = lastMove.x;
            z = lastMove.z;

            //Debug.Log(moves[x, z].x + " " + moves[x, z].z + ":" + x + " " + z);

            if (x > 0)
            {
                float pesoSig = lastMove.cost + gridController.TacticMoveCost(x - 1, z);
                if (gridController.GetCell(x - 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x - 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x - 1, z) == null)
                    if (moves[x - 1, z] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, moves[x, z], x - 1, z);
                        moves[x - 1, z] = newMove;
                        queue.Add(newMove);
                    } else if (moves[x - 1, z].cost > pesoSig)
                    {
                        moves[x - 1, z].cost = pesoSig;
                        moves[x - 1, z].comesFrom = moves[x, z];
                    }
            }
            if (x < gridController.rows - 1)
            {
                float pesoSig = lastMove.cost + gridController.TacticMoveCost(x + 1, z);
                if (gridController.GetCell(x + 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x + 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x + 1, z) == null)
                    if (moves[x + 1, z] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, moves[x, z], x + 1, z);
                        moves[x + 1, z] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x + 1, z].cost > pesoSig)
                    {
                        moves[x + 1, z].cost = pesoSig;
                        moves[x + 1, z].comesFrom = moves[x, z];
                    }
            }
            if (z > 0)
            {
                float pesoSig = lastMove.cost + gridController.TacticMoveCost(x, z - 1);
                if (gridController.GetCell(x, z - 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z - 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z - 1) == null)
                    if (moves[x, z - 1] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, moves[x, z], x, z - 1);
                        moves[x, z - 1] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x, z - 1].cost > pesoSig)
                    {
                        moves[x, z - 1].cost = pesoSig;
                        moves[x, z - 1].comesFrom = moves[x, z];
                    }
            }
            if (z < gridController.cols - 1)
            {
                float pesoSig = lastMove.cost + gridController.TacticMoveCost(x, z + 1);
                if (gridController.GetCell(x, z + 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z + 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z + 1) == null)
                    if (moves[x, z + 1] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, moves[x, z], x, z + 1);
                        moves[x, z + 1] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x, z + 1].cost > pesoSig)
                    {
                        moves[x, z + 1].cost = pesoSig;
                        moves[x, z + 1].comesFrom = moves[x, z];
                    }
            }
        }



        SingleMove obj = moves[objX, objZ];
        if (obj == null)
        {
            obj = lastMove;
        }

        List<SingleMove> listMoves = new List<SingleMove>();

        //listMoves.Add(obj);
        

        while (obj!=null && !(obj.x == initX && obj.z == initZ))
        {
            listMoves.Add(obj);
            obj = obj.comesFrom;
        }

        //listMoves.Add(moves[initX, initZ]);

        return listMoves;
        //yield return Ninja.JumpToUnity;
    }

    public SingleMove getMin(ref List<SingleMove> cola)
    {
        float min = Mathf.Infinity;
        SingleMove best = null;
        foreach (SingleMove move in cola)
        {
            if (move.cost < min)
            {
                min = move.cost;
                best = move;
            }
        }

        return best;
    }

    [Panda.Task]
    bool HasCoins()
    {
        return coins > 0;
    }

    [Panda.Task]
    public void CreateBasic()
    {
        if (HasCoinsFor(UnitType.BASICA) && CanSpawn())
        {
            CreateUnit(UnitType.BASICA);

            Panda.Task.current.Succeed();
        }
        else
        {
            Panda.Task.current.Fail();
        }
    }

    private IEnumerator CreateBasicUnit(float delay)
    {
        

        yield return new WaitForSeconds(delay);
    }

    [Panda.Task]
    public void MoveUnitFromSpawn()
    {
        if (!HasCoins())
        {
            Panda.Task.current.Fail();
        }
        else
        {
            Unit unit = gridController.GetUnit(spawnX, spawnZ);

            if (unit == null)
            {
                Panda.Task.current.Fail();
            }
            else
            {
                MoveUnit(unit, otherBaseX-1, otherBaseZ);

                Panda.Task.current.Succeed();
            }
        }
        
    }

}