using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    public static int monedas;

    public GameObject unidadBasica;
    public GameObject unidadDistancia;
    public GameObject unidadExploradora;
    public GameObject unidadOfensiva;
    public GameObject unidadDefensiva;



    // Use this for initialization
    void Start()
    {
        monedas = 300;

    }

    // Update is called once per frame
    void Update()
    {

        if (true)
        {

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (monedas >= 10)
                {
                    Instantiate(unidadBasica);
                    monedas -= 10;
                }

            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (monedas >= 15)
                {
                    Instantiate(unidadDistancia);
                    monedas -= 15;
                }

            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (monedas >= 20)
                {
                    Instantiate(unidadExploradora);
                    monedas -= 20;
                }

            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (monedas >= 35)
                {
                    Instantiate(unidadOfensiva);
                    monedas -= 35;
                }

            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (monedas >= 35)
                {
                    Instantiate(unidadDefensiva);
                    monedas -= 35;
                }

            }
        }



    }
}
