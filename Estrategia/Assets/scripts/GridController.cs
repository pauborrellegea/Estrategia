using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Casilla grassCell, forestCell, mountainCell, towerCell;
    public Material playerBase, iaBase;

    GameController gameController;

    //perlin noise
    public float scale = 10f;
    public float offsetX, offsetY = 0f;
    public float fadeDist = 8f;
    public float cutGrass = 0.5f;
    public float cutForest = 0.35f;


    public int rows = 30; //mejor que sean iguales
    public int cols = 30;

    int forestCost = 2;

    public Transform scene;

    //Grids globales
    private Casilla[,] gridCells;
    private Unit[,] gridUnits;

    private bool[,] movementCells; //Destacadas y solo permiten moverse en ese rango
    private bool[,] attackCells;

    public int[,] playerVisibility; //numerado de 0 a N (N es la cantidad de unidades que pueden ver esa casilla)

    public int[,] iaVisibility;

    void Awake()
    {
        gameController = GetComponent<GameController>();

        gridCells = new Casilla[rows, cols];
        gridUnits = new Unit[rows, cols];
        movementCells = new bool[rows, cols];
        attackCells = new bool[rows, cols];
        playerVisibility = new int[rows, cols];

        GenerateMap();
    }

    //con otras funciones se pueden crear mapas de influencia o 
    //hacer busqueda de profundidad para comprobar la visibilidad del jugador Y del enemigo (por separado)
    //tambien se puede hacer un pathfiding sencillo con esa bfs



    public bool CanSpawnUnit(int x, int z)
    {
        return gridUnits[x, z] == null;
    }
    public bool CanMove(int x, int z)
    {
        return movementCells[x, z];
    }

    public bool CanAttack(int x, int z)
    {
        return attackCells[x, z];
    }

    public void AddUnit(Unit unit, int x, int z)
    {
        gridUnits[x, z] = unit;
        AddVisibility(x, z, unit.vision, true, unit.player);

        UpdateFog();
    }

    public Unit GetUnit(int x, int z)
    {
        return gridUnits[x, z];
    }

    public void MoveUnit(Unit unit, int toX, int toZ)
    {
        //Esto deberia tener en cuenta mas cosas, y moverse suavemente
        

        //si se puede mover se mueve
        
        if (gridUnits[toX, toZ] == null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            gridUnits[x, z] = null;

            //eliminar visibilidad
            AddVisibility(x, z, unit.vision, false, unit.player);
            unit.transform.position = new Vector3(toX, 0, toZ);
            gridUnits[toX, toZ] = unit;
            //poner en la posicion nueva
            AddVisibility(toX, toZ, unit.vision, true, unit.player);

            UpdateFog();
        }
    }

    private void AddVisibility(int x, int z, int range, bool sum, bool player) //PROBLEMA: se ve visibilidad cuadriculada (puede ser la intencion pero no se) 
    {
        int v = sum ? 1 : -1;
        bool[,] visitedCells = new bool[rows, cols];

        Queue<int[]> queue = new Queue<int[]>();

        visitedCells[x, z] = true;
        queue.Enqueue(new int[3]{x, z, range});
        
        while (queue.Count > 0)
        {
            int[] cell = queue.Dequeue();
            x = cell[0];
            z = cell[1];
            range = cell[2];

            if (player)
                playerVisibility[x, z] += v; //suma o resta
            else
                iaVisibility[x, z] += v;

            if (x > 0)
            {
                if (!visitedCells[x - 1, z] && range>0)
                {
                    visitedCells[x - 1, z] = true;
                    queue.Enqueue(new int[3] { x - 1, z, range - 1 });
                }
            }
            if (x < rows-1)
            {
                if (!visitedCells[x + 1, z] && range > 0)
                {
                    visitedCells[x + 1, z] = true;
                    queue.Enqueue(new int[3] { x + 1, z, range - 1 });
                }
            }
            if (z > 0)
            {
                if (!visitedCells[x, z - 1] && range > 0)
                {
                    visitedCells[x, z - 1] = true;
                    queue.Enqueue(new int[3] { x, z - 1, range - 1 });
                }
            }
            if (z < cols-1)
            {
                if (!visitedCells[x, z + 1] && range > 0)
                {
                    visitedCells[x, z + 1] = true;
                    queue.Enqueue(new int[3] { x, z + 1, range - 1 });
                }
            }
        }
    }

    private void UpdateFog()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (playerVisibility[r,c] > 0)
                {
                    //Lo ve
                    gridCells[r, c].EnableFog(false);
                } else
                {
                    //No lo ve ninguna unidad
                    gridCells[r, c].EnableFog(true);
                }
            }
        }
    }

    public void SetMovement(Unit unit)
    {
        movementCells = new bool[rows, cols]; //reset
        if (unit != null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            int range = unit.rangoDeMovimiento;

            Queue<int[]> queue = new Queue<int[]>();

            movementCells[x, z] = true;
            queue.Enqueue(new int[3] { x, z, range });

            while (queue.Count > 0)
            {
                int[] cell = queue.Dequeue();
                x = cell[0];
                z = cell[1];
                range = cell[2];

                
                if (x > 0)
                {
                    if (!movementCells[x - 1, z] && range > 0 && gridUnits[x - 1, z]==null && playerVisibility[x - 1, z]>0)
                    {
                        if (gridCells[x - 1, z].isGrass())
                        {
                            movementCells[x - 1, z] = true;
                            queue.Enqueue(new int[3] { x - 1, z, range - 1 });
                        } else if (gridCells[x - 1, z].isForest() && range >= forestCost)
                        {
                            movementCells[x - 1, z] = true;
                            queue.Enqueue(new int[3] { x - 1, z, range - forestCost });
                        }
                        
                    }
                }
                if (x < rows - 1)
                {
                    if (!movementCells[x + 1, z] && range > 0 && gridUnits[x + 1, z] == null && playerVisibility[x + 1, z] > 0)
                    {
                        if (gridCells[x + 1, z].isGrass())
                        {
                            movementCells[x + 1, z] = true;
                            queue.Enqueue(new int[3] { x + 1, z, range - 1 });
                        }
                        else if (gridCells[x + 1, z].isForest() && range >= forestCost)
                        {
                            movementCells[x + 1, z] = true;
                            queue.Enqueue(new int[3] { x + 1, z, range - forestCost });
                        }
                    }
                }
                if (z > 0)
                {
                    if (!movementCells[x, z - 1] && range > 0 && gridUnits[x, z - 1] == null && playerVisibility[x, z - 1] > 0)
                    {
                        if (gridCells[x, z - 1].isGrass())
                        {
                            movementCells[x, z - 1] = true;
                            queue.Enqueue(new int[3] { x, z - 1, range - 1 });
                        }
                        else if (gridCells[x, z - 1].isForest() && range >= forestCost)
                        {
                            movementCells[x, z - 1] = true;
                            queue.Enqueue(new int[3] { x, z - 1, range - forestCost });
                        }
                    }
                }
                if (z < cols - 1)
                {
                    if (!movementCells[x, z + 1] && range > 0 && gridUnits[x, z + 1] == null && playerVisibility[x, z + 1] > 0)
                    {
                        if (gridCells[x, z + 1].isGrass())
                        {
                            movementCells[x, z + 1] = true;
                            queue.Enqueue(new int[3] { x, z + 1, range - 1 });
                        }
                        else if (gridCells[x, z + 1].isForest() && range >= forestCost)
                        {
                            movementCells[x, z + 1] = true;
                            queue.Enqueue(new int[3] { x, z + 1, range - forestCost });
                        }
                    }
                }
            }
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (movementCells[r, c])
                {
                    gridCells[r, c].EnableIndicator(true);
                    gridCells[r, c].setColor(false);
                }
                else
                {
                    gridCells[r, c].EnableIndicator(false);
                }
            }
        }

        //gridCells[0, 0].setColor(false);
    }

    public void SetAttack(Unit unit)
    {
        attackCells = new bool[rows, cols]; //reset
        if (unit != null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            int range = unit.rangoDeAtaque;

            Queue<int[]> queue = new Queue<int[]>();

            attackCells[x, z] = true;
            queue.Enqueue(new int[3] { x, z, range });

            while (queue.Count > 0)
            {
                int[] cell = queue.Dequeue();
                x = cell[0];
                z = cell[1];
                range = cell[2];


                if (x > 0)
                {
                    if (!attackCells[x - 1, z] && range > 0)
                    {
                        if (gridUnits[x - 1, z]==null || !gridUnits[x - 1, z].player)
                        {
                            attackCells[x - 1, z] = true;
                        }
                        queue.Enqueue(new int[3] { x - 1, z, range - 1 });
                    }
                }
                if (x < rows - 1)
                {
                    if (!attackCells[x + 1, z] && range > 0)
                    {
                        if (gridUnits[x + 1, z] == null || !gridUnits[x + 1, z].player)
                        {
                            attackCells[x + 1, z] = true;
                        }
                        queue.Enqueue(new int[3] { x + 1, z, range - 1 });
                    }
                }
                if (z > 0)
                {
                    if (!attackCells[x, z - 1] && range > 0)
                    {
                        if (gridUnits[x, z - 1] == null || !gridUnits[x, z - 1].player)
                        {
                            attackCells[x, z - 1] = true;
                        }
                        queue.Enqueue(new int[3] { x, z - 1, range - 1 });
                    }
                }
                if (z < cols - 1)
                {
                    if (!attackCells[x, z + 1] && range > 0)
                    {
                        if (gridUnits[x, z + 1] == null || !gridUnits[x, z + 1].player)
                        {
                            attackCells[x, z + 1] = true;
                        }
                        queue.Enqueue(new int[3] { x, z + 1, range - 1 });
                    }
                }
            }
            x = (int)unit.transform.position.x;
            z = (int)unit.transform.position.z;
            attackCells[x, z] = false;
            
            attackCells[rows - 1, 0] = false;
        }
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (attackCells[r, c])
                {
                    gridCells[r, c].EnableIndicator(true);
                    gridCells[r, c].setColor(true);
                }
                else
                {
                    gridCells[r, c].EnableIndicator(false);
                }
            }
        }
        
    }

    public void GenerateMap()
    {
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                

                Casilla newCell;
                if (x==0 && z == cols - 1)
                {
                    //ia
                    newCell = Instantiate(towerCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    setBaseColor(newCell, false);
                    gridCells[x, z] = newCell;
                    continue;
                } else if (x == rows - 1 && z == 0)
                {
                    //player base
                    newCell = Instantiate(towerCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    setBaseColor(newCell, true);
                    gridCells[x, z] = newCell;
                    continue;
                } else
                {
                    float sample = CalculateCell(x, z);
                    if (sample > cutGrass)
                    {
                        newCell = Instantiate(grassCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    else if (sample > cutForest)
                    {
                        newCell = Instantiate(forestCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    else
                    {
                        newCell = Instantiate(mountainCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    gridCells[x, z] = newCell;
                }
            }
        }
    }

    float CalculateCell(int x, int y)
    {
        float dist = Mathf.Min(x, y, rows - x - 1, cols - y - 1);

        float fade = 1f;

        if (dist < rows / fadeDist)
        {
            fade = (1+fadeDist) * dist / rows;
        }

        float xCoord = (float)x / rows * scale + offsetX;
        float yCoord = (float)y / cols * scale + offsetY;

        float sample = 1f - Mathf.PerlinNoise(xCoord, yCoord) * fade;

        return sample;

    }

    void setBaseColor(Casilla tower, bool player)
    {
        Material color = player ? playerBase : iaBase;

        tower.transform.GetChild(2).GetChild(0).GetComponent<Renderer>().material = color;
        tower.transform.GetChild(2).GetChild(1).GetComponent<Renderer>().material = color;
    }

}