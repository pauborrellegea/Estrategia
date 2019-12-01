using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nieblaController : MonoBehaviour
{

    Material transparente;
    Material niebla;
    Material nieblaInicial;

    // Start is called before the first frame update
    void Start()
    {
        transparente = Resources.Load<Material>("Transparente");
        niebla = Resources.Load<Material>("Niebla");
        nieblaInicial = Resources.Load<Material>("NieblaInicial");
        gameObject.GetComponent<Renderer>().material = nieblaInicial;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
   
            gameObject.GetComponent<Renderer>().material = transparente;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            gameObject.GetComponent<Renderer>().material = transparente;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            gameObject.GetComponent<Renderer>().material = niebla;
        }
    }
}
