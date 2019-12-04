﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private GridController gridController;

    public static int monedas;

    public GameObject unidadBasica;
    public GameObject unidadDistancia;
    public GameObject unidadExploradora;
    public GameObject unidadOfensiva;
    public GameObject unidadDefensiva;

    private int spawnX, spawnZ;


    // Use this for initialization
    void Awake()
    {
        GameObject escenario = GameObject.Find("Escenario");

        gridController = escenario.GetComponent<GridController>();

        monedas = 30;

        spawnX = (int)transform.position.x;
        spawnZ = (int)transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (monedas >= 10)
            {
                if (gridController.CanSpawnUnit(spawnX, spawnZ))
                {
                    GameObject newUnit = Instantiate(unidadBasica, transform.position, transform.rotation, transform);
                    gridController.AddUnit(newUnit, spawnX, spawnZ);
                    monedas -= 10;
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (monedas >= 15)
            {
                if (gridController.CanSpawnUnit(spawnX, spawnZ))
                {
                    GameObject newUnit = Instantiate(unidadDistancia, transform.position, transform.rotation, transform);
                    gridController.AddUnit(newUnit, spawnX, spawnZ);
                    monedas -= 15;
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (monedas >= 20)
            {
                if (gridController.CanSpawnUnit(spawnX, spawnZ))
                {
                    GameObject newUnit = Instantiate(unidadExploradora, transform.position, transform.rotation, transform);
                    gridController.AddUnit(newUnit, spawnX, spawnZ);
                    monedas -= 20;
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (monedas >= 35)
            {
                if (gridController.CanSpawnUnit(spawnX, spawnZ))
                {
                    GameObject newUnit = Instantiate(unidadOfensiva, transform.position, transform.rotation, transform);
                    gridController.AddUnit(newUnit, spawnX, spawnZ);
                    monedas -= 35;
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (monedas >= 35)
            {
                if (gridController.CanSpawnUnit(spawnX, spawnZ))
                {
                    GameObject newUnit = Instantiate(unidadDefensiva, transform.position, transform.rotation, transform);
                    gridController.AddUnit(newUnit, spawnX, spawnZ);
                    monedas -= 35;
                }
            }

        }

    }
}
