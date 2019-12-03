using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    int rows = 17;
    int cols = 12;

    //Grids globales
    private GameObject[,] gridCells;
    private GameObject[,] gridUnits;


    //crear 


    void Awake()
    {
        //En awake se guardan las referencias de gameObjects de casillas en una matriz, con su posicion como indice

        gridCells = new GameObject[rows, cols];
        gridUnits = new GameObject[rows, cols];

        for (int i = 0; i<transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            //Local position para que solo dependa de la posicion interna

            //NO SE DEBE MOVER LAS CASILLAS DE DENTRO DEL OBJETO ESCENARIO (deben ir de x,z = 0,0 a 16,11)

            int x = (int)child.localPosition.x;
            int y = (int)child.localPosition.z;

            gridCells[x, y] = child.gameObject;
            
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

    public void AddUnit(GameObject unit, int x, int z)
    {
        //puede que sea mejor instanciar unidades en un objeto hijo del escenario, y guardar todas las casillas en otro hijo
        gridUnits[x, z] = unit;
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
            unit.transform.position = new Vector3(toX, 0, toZ);
            gridUnits[toX, toZ] = unit;
        }
    }
}
