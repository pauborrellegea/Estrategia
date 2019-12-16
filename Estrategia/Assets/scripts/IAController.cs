using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using Panda;

public class IAController : Player
{
    Unit unitToAttack = null;
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
                newUnit.SetPlayer(player, gridController.iaBase);

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

    [Panda.Task]
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
                if (gridController.GetCell(x - 1, z)==null || (gridController.GetCell(x - 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x - 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x - 1, z) == null))
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
                if (gridController.GetCell(x + 1, z) == null || (gridController.GetCell(x + 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x + 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x + 1, z) == null))
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
                if (gridController.GetCell(x, z - 1) == null || (gridController.GetCell(x, z - 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z - 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z - 1) == null))
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
                if (gridController.GetCell(x, z + 1) == null || (gridController.GetCell(x, z + 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z + 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z + 1) == null))
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
            if (gridController.GetCell(obj.x, obj.z).type!=Casilla.CellType.MOUNTAIN && gridController.GetCell(obj.x, obj.z).type != Casilla.CellType.TOWER && gridController.GetUnit(obj.x, obj.z) == null)
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
            if (move.cost < min || best==null)
            {
                min = move.cost;
                best = move;
            }
        }

        return best;
    }

    [Panda.Task]
    bool HasCoins(int x=0)
    {
        return coins > x;
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

    [Panda.Task]
    public void MoveUnitWithInfluence()
    {
        if (!HasCoins())
        {
            Panda.Task.current.Fail();
        }
        else
        {
            Unit unit = UnitLessInfluence();

            if (unit == null)
            {
                Panda.Task.current.Fail();
            }
            else
            {
                int x = 0;
                int z = 0;

                gridController.GetBestInfluence(ref x, ref z);

                MoveUnit(unit, x, z);

                Panda.Task.current.Succeed();
            }
        }
    }

    [Panda.Task]
    public bool HasUnits()
    {
        return gridController.iaUnits.Count > 0;
    }

    public Unit UnitLessInfluence()
    {
        float lessInf = -Mathf.Infinity;
        Unit worseUnit = null;

        foreach (Unit u in gridController.iaUnits)
        {
            float inf = gridController.TacticMoveCost((int)u.transform.position.x, (int)u.transform.position.z);
            if (u.remainingMoves>0 && isNotBlocked(u) && inf > lessInf)
            {
                lessInf = inf;
                worseUnit = u;
            }
        }
        return worseUnit;
    }

    [Panda.Task]
    public void CreateControlledUnit()
    {
        int s = nextSpawnUnit();
        if (HasCoinsFor((UnitType)s) && CanSpawn())
        {
            CreateUnit((UnitType)s);

            Panda.Task.current.Succeed();
        }
        else
        {
            Panda.Task.current.Fail();
        }
    }

    [Panda.Task]
    public void PassTurn()
    {
        EndTurn();
        Panda.Task.current.Succeed();
    }


    public int nextSpawnUnit()
    {
        float[] unitsAmount = new float[gridController.unitsWeight.Length];

        int total = 0;

        foreach (Unit unit in gridController.iaUnits)
        {
            unitsAmount[(int)unit.type]++;
            total++;
        }
        

        float min = Mathf.Infinity;
        int bestI = -1;

        for (int i = 0; i < unitsAmount.Length; i++)
        {
            float w = -gridController.unitsWeight[i];
            if (total > 0)
            {
                w += unitsAmount[i] / total;
            }
            if (w < min)
            {
                min = w;
                bestI = i;
            }
        }

        return bestI;
    }

    [Panda.Task]
    public bool HasUnitsToMove()
    {
        if (coins == 0) return false;
        foreach (Unit unit in gridController.iaUnits)
        {
            if (unit.remainingMoves > 0 && isNotBlocked(unit))
            {
                return true;
            }
        }

        return false;
    }

    public bool isNotBlocked(Unit unit)
    {
        int x = (int)unit.transform.position.x;
        int z = (int)unit.transform.position.z;

        if (x > 0 && gridController.GetCell(x - 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x - 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x - 1, z) == null) return true;
        if (x < gridController.rows-1 && gridController.GetCell(x + 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x + 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x + 1, z) == null) return true;
        if (z < gridController.cols - 1 && gridController.GetCell(x, z + 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z + 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z + 1) == null) return true;
        if (z > 0 && gridController.GetCell(x, z - 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z - 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z - 1) == null) return true;


        return false;
    }

    [Panda.Task]
    public bool EnemyInRange()
    {
        foreach (Unit unit in gridController.playerUnits)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            if (gridController.iaVisibility[x, z]>0 && gridController.attackInfluence[x, z] > 0)
            {
                unitToAttack = unit;
                Panda.Task.current.Succeed();
                return true;
            }
        }

        Panda.Task.current.Fail();
        return false;
    }

    public bool PotentialRange(int initX, int initZ, int objX, int objZ, int range)
    {
        return Mathf.Abs(initX - objX) + Mathf.Abs(initZ - objZ) <= range;
    }

    [Panda.Task]
    public void TryToKill()
    {
        if (unitToAttack == null)
        {
            Panda.Task.current.Fail();
            return;
        }
        int ax = (int)unitToAttack.transform.position.x;
        int az = (int)unitToAttack.transform.position.z;
        if (gridController.attackInfluence[ax, az] >= unitToAttack.vida)
        {
            foreach (Unit u in gridController.iaUnits)
            {
                int ux = (int)u.transform.position.x;
                int uz = (int)u.transform.position.z;
                if (u.ataque > 0 && !u.hasAttacked && PotentialRange(ax, az, ux, uz, u.rangoDeAtaque + u.remainingMoves))
                {
                    if (PotentialRange(ax, az, ux, uz, u.rangoDeAtaque))
                    {
                        Attack(u, ax, az);
                    }
                    else
                    {
                        MoveUnit(u, ax, az);
                        if (PotentialRange(ax, az, ux, uz, u.rangoDeAtaque))
                        {
                            Attack(u, ax, az);
                        }
                    }

                    if (unitToAttack.IsDead()) { break; }
                }
            }
        }
        else
        {
            Panda.Task.current.Fail();
            return;
        }

        if (unitToAttack.IsDead())
        {
            Panda.Task.current.Succeed();
        }
        else
        {
            Panda.Task.current.Fail();
        }
    }

    [Panda.Task]
    public void AttackOnce()
    {
        int ax = (int)unitToAttack.transform.position.x;
        int az = (int)unitToAttack.transform.position.z;
        int ux = -1;
        int uz = -1;

        bool canAttackDirect = false;
        int bestAttack = 0;
        Unit bestUnit = null;
        if (gridController.attackInfluence[ax, az] > 0)
        {
            foreach (Unit u in gridController.iaUnits)
            {
                ux = (int)u.transform.position.x;
                uz = (int)u.transform.position.z;
                if (u.ataque > 0 && !u.hasAttacked && PotentialRange(ax, az, ux, uz, u.rangoDeAtaque + u.remainingMoves))
                {
                    bool thisAttack = PotentialRange(ax, az, ux, uz, u.rangoDeAtaque);
                    if (!canAttackDirect)
                    {
                        if (thisAttack)
                        {
                            canAttackDirect = true;
                            bestAttack = u.ataque;
                            bestUnit = u;
                        } else
                        {
                            if (!canAttackDirect && bestAttack<u.ataque)
                            {
                                bestAttack = u.ataque;
                                bestUnit = u;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Panda.Task.current.Fail();
        }


        //estar en rango
        //mejor ataque si estan varias
        //si no estan, mejor ataque tambien
        if (bestUnit != null)
        {
            ux = (int)bestUnit.transform.position.x;
            uz = (int)bestUnit.transform.position.z;
            if (canAttackDirect)
            {
                Attack(bestUnit, ax, az);
            }
            else
            {
                MoveUnit(bestUnit, ax, az);
                if (PotentialRange(ax, az, ux, uz, bestUnit.rangoDeAtaque))
                {
                    Attack(bestUnit, ax, az);
                }
            }
            Panda.Task.current.Succeed();
        } else
        {
            Panda.Task.current.Fail();
        }
    }

    [Panda.Task]
    public bool CanAttackBase()
    {
        return gridController.attackInfluence[otherBaseX, otherBaseZ] > 0f;
    }

    [Panda.Task]
    public void AttackBaseOnce()
    {
        int ax = otherBaseX;
        int az = otherBaseZ;
        int ux = -1, uz = -1;
        bool canAttackDirect = false;
        int bestAttack = 0;
        Unit bestUnit = null;
        if (gridController.attackInfluence[ax, az] > 0)
        {
            foreach (Unit u in gridController.iaUnits)
            {
                ux = (int)u.transform.position.x;
                uz = (int)u.transform.position.z;
                if (u.ataque > 0 && !u.hasAttacked && PotentialRange(ax, az, ux, uz, u.rangoDeAtaque + u.remainingMoves))
                {
                    bool thisAttack = PotentialRange(ax, az, ux, uz, u.rangoDeAtaque);
                    if (!canAttackDirect)
                    {
                        if (thisAttack)
                        {
                            canAttackDirect = true;
                            bestAttack = u.ataque;
                            bestUnit = u;
                        }
                        else
                        {
                            if (!canAttackDirect && bestAttack < u.ataque)
                            {
                                bestAttack = u.ataque;
                                bestUnit = u;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Panda.Task.current.Fail();
        }


        //estar en rango
        //mejor ataque si estan varias
        //si no estan, mejor ataque tambien
        if (bestUnit != null)
        {
            ux = (int)bestUnit.transform.position.x;
            uz = (int)bestUnit.transform.position.z;
            if (canAttackDirect)
            {
                Attack(bestUnit, ax, az);
            }
            else
            {
                MoveUnit(bestUnit, ax, az);
                if (PotentialRange(ax, az, ux, uz, bestUnit.rangoDeAtaque))
                {
                    Attack(bestUnit, ax, az);
                }
            }
            Panda.Task.current.Succeed();
        }
        else
        {
            Panda.Task.current.Fail();
        }
    }
}