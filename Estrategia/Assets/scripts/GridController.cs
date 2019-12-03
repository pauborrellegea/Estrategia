using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    int rows = 17;
    int cols = 12;

    private GameObject[,] gridObjects;

    //crear 


    void Awake()
    {
        //En awake se guardan las referencias de gameObjects de casillas en una matriz, con su posicion como indice

        gridObjects = new GameObject[rows, cols];

        for (int i = 0; i<transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            //Local position para que solo dependa de la posicion interna

            //NO SE DEBE MOVER LAS CASILLAS DE DENTRO DEL OBJETO ESCENARIO (deben ir de x,z = 0,0 a 16,11)

            int x = (int)child.localPosition.x;
            int y = (int)child.localPosition.z;

            gridObjects[x, y] = child.gameObject;
            
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
}
