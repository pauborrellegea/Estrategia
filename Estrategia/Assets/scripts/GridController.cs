using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    int rows = 17;
    int cols = 12;

    int forestCost = 2;

    //Grids globales
    private Casilla[,] gridCells;
    private GameObject[,] gridUnits;

    private bool[,] movementCells; //Destacadas y solo permiten moverse en ese rango

    public int[,] playerVisibility; //numerado de 0 a N (N es la cantidad de unidades que pueden ver esa casilla)


    void Awake()
    {
        //En awake se guardan las referencias de gameObjects de casillas en una matriz, con su posicion como indice

        gridCells = new Casilla[rows, cols];
        gridUnits = new GameObject[rows, cols];
        movementCells = new bool[rows, cols];
        playerVisibility = new int[rows, cols];
        

        for (int i = 0; i<transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            //Local position para que solo dependa de la posicion interna

            //NO SE DEBE MOVER LAS CASILLAS DE DENTRO DEL OBJETO ESCENARIO (deben ir de x,z = 0,0 a 16,11)

            int x = (int)child.localPosition.x;
            int y = (int)child.localPosition.z;

            gridCells[x, y] = child.GetComponent<Casilla>(); ;
            
            /*
            if (gridObjects[x, y] == null)
            {
                Debug.Log("done " + x + " " + y);
                gridObjects[x, y] = child.gameObject;
            } else
            {
                Debug.Log("repeated " + x + " " + y);
            }*/
        }
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

    public void AddUnit(GameObject unit, int x, int z)
    {
        //puede que sea mejor instanciar unidades en un objeto hijo del escenario, y guardar todas las casillas en otro hijo
        gridUnits[x, z] = unit;
        AddVisibility(x, z, unit.GetComponent<unidades_parametros>().vision, true);

        UpdateFog();
    }

    public GameObject GetUnit(int x, int z)
    {
        return gridUnits[x, z];
    }

    public void MoveUnit(GameObject unit, int toX, int toZ)
    {
        //Esto deberia tener en cuenta mas cosas, y moverse suavemente
        

        //si se puede mover se mueve
        
        if (gridUnits[toX, toZ] == null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            gridUnits[x, z] = null;

            //eliminar visibilidad
            AddVisibility(x, z, unit.GetComponent<unidades_parametros>().vision, false);
            unit.transform.position = new Vector3(toX, 0, toZ);
            gridUnits[toX, toZ] = unit;
            //poner en la posicion nueva
            AddVisibility(toX, toZ, unit.GetComponent<unidades_parametros>().vision, true);

            UpdateFog();
        }
    }

    private void AddVisibility(int x, int z, int range, bool sum) //PROBLEMA: se ve visibilidad cuadriculada (puede ser la intencion pero no se) 
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

            playerVisibility[x, z] += v; //suma o resta

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

    public void SetMovement(GameObject unit)
    {
        movementCells = new bool[rows, cols]; //reset
        if (unit != null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            int range = unit.GetComponent<unidades_parametros>().rangoDeMovimiento;

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
                    if (!movementCells[x - 1, z] && range > 0)
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
                    if (!movementCells[x + 1, z] && range > 0)
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
                    if (!movementCells[x, z - 1] && range > 0)
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
                    if (!movementCells[x, z + 1] && range > 0)
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
                    //Lo ve
                    gridCells[r, c].EnableIndicator(true);
                }
                else
                {
                    //No lo ve ninguna unidad
                    gridCells[r, c].EnableIndicator(false);
                }
            }
        }
    }
}
