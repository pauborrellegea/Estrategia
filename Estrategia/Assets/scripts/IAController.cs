using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;

public class IAController : Player
{
    List<Direction> lista;

    private void Start()
    {
        lista = new List<Direction>();
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

        Dijkstra(newX, newZ, initX, initZ);

        //mover hasta donde se pueda (de 1 en 1)
        int possibleMoves = Mathf.Min(unit.remainingMoves, coins);
        int currentMoves = 0;
        int x = initX;
        int z = initZ;
        while (lista.Count > 0)
        {
            Direction dir = lista[lista.Count - 1];
            lista.RemoveAt(lista.Count - 1);

            int nx = x;
            int nz = z;

            if (dir == Direction.LEFT)
            {
                nx = x + 1;
                nz = z;
            } else if (dir == Direction.RIGHT)
            {
                nx = x - 1;
                nz = z;
            } else if (dir == Direction.UP)
            {
                nz = z- 1;
                nx = x;
            } else if (dir == Direction.DOWN)
            {
                nz = z + 1;
                nx = x;
            }

            Casilla cell = gridController.GetCell(x, z);

            if (cell.type == Casilla.CellType.GRASS)
            {
                currentMoves++;
                if (currentMoves <= possibleMoves)
                {
                    x = nx;
                    z = nz;
                } else
                {
                    break;
                }
            } else if (cell.type == Casilla.CellType.FOREST)
            {
                currentMoves += gridController.forestCost;
                if (currentMoves <= possibleMoves)
                {
                    x = nx;
                    z = nz;
                }
                else
                {
                    break;
                }
            }
        }

        gridController.MoveUnit(unit, x, z);
        SubstractCoins(currentMoves);
        unit.substractMoves(currentMoves);
    }

    public override void resetEndTurn()
    {
        //nada que resetear
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


    public IEnumerator Dijkstra(int objX, int objZ, int initX, int initZ)
    {
        List<SingleMove> queue = new List<SingleMove>();

        SingleMove[,] moves = new SingleMove[gridController.rows, gridController.cols];

        moves[initX, initZ] = new SingleMove(0f, Direction.NONE, initX, initZ);
        queue.Add(moves[initX, initZ]);

        int x, z;

        while (queue.Count > 0)
        {
            SingleMove move = getMin(ref queue);
            queue.Remove(move);
            x = move.x;
            z = move.z;

            if (x==objX && z == objZ)
            {
                break;
            }
            
            if (x > 0)
            {
                float pesoSig = move.cost + gridController.TacticMoveCost(x - 1, z);
                if (gridController.GetCell(x - 1, z).type!=Casilla.CellType.MOUNTAIN && gridController.GetCell(x - 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x - 1, z) == null)
                    if (moves[x - 1, z] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, Direction.RIGHT, x - 1, z);
                        moves[x - 1, z] = newMove;
                        queue.Add(newMove);
                    } else if (moves[x - 1, z].cost > pesoSig)
                    {
                        moves[x - 1, z].cost = pesoSig;
                        moves[x - 1, z].comesFrom = Direction.RIGHT;
                    }
            }
            if (x < gridController.rows - 1)
            {
                float pesoSig = move.cost + gridController.TacticMoveCost(x + 1, z);
                if (gridController.GetCell(x + 1, z).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x + 1, z).type != Casilla.CellType.TOWER && gridController.GetUnit(x + 1, z) == null)
                    if (moves[x + 1, z] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, Direction.LEFT, x + 1, z);
                        moves[x + 1, z] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x + 1, z].cost > pesoSig)
                    {
                        moves[x + 1, z].cost = pesoSig;
                        moves[x + 1, z].comesFrom = Direction.LEFT;
                    }
            }
            if (z > 0)
            {
                float pesoSig = move.cost + gridController.TacticMoveCost(x, z - 1);
                if (gridController.GetCell(x, z - 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z - 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z - 1) == null)
                    if (moves[x, z - 1] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, Direction.UP, x, z - 1);
                        moves[x, z - 1] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x, z - 1].cost > pesoSig)
                    {
                        moves[x, z - 1].cost = pesoSig;
                        moves[x, z - 1].comesFrom = Direction.UP;
                    }
            }
            if (z < gridController.cols - 1)
            {
                float pesoSig = move.cost + gridController.TacticMoveCost(x, z + 1);
                if (gridController.GetCell(x, z + 1).type != Casilla.CellType.MOUNTAIN && gridController.GetCell(x, z + 1).type != Casilla.CellType.TOWER && gridController.GetUnit(x, z + 1)==null)
                    if (moves[x, z + 1] == null)
                    {
                        SingleMove newMove = new SingleMove(pesoSig, Direction.DOWN, x, z + 1);
                        moves[x, z + 1] = newMove;
                        queue.Add(newMove);
                    }
                    else if (moves[x, z + 1].cost > pesoSig)
                    {
                        moves[x, z + 1].cost = pesoSig;
                        moves[x, z + 1].comesFrom = Direction.DOWN;
                    }
            }
        }

        lista = new List<Direction>();

        x = objX;
        z = objZ;

        while(x!=initX && z != objZ)
        {
            Direction dir = moves[x, z].comesFrom;
            if (dir == Direction.DOWN)
            {
                z -= 1;
            } else if (dir == Direction.UP)
            {
                z += 1;
            } else if (dir == Direction.LEFT)
            {
                x -= 1;
            } else if (dir == Direction.RIGHT)
            {
                x += 1;
            } else
            {
                break;
            }
            lista.Add(dir);
        }

        yield return Ninja.JumpToUnity;
    }

    public SingleMove getMin(ref List<SingleMove> cola)
    {
        float min = Mathf.Infinity;
        SingleMove best = null;
        foreach (SingleMove move in cola)
        {
            if (move.cost<min)
            {
                min = move.cost;
                best = move;
            }
        }

        return best;
    }
}
