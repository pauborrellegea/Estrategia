using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class celdasAdyacentes : MonoBehaviour
{
    Transform unit;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    /*
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("ground"))
        {

            //Transform go = collision.gameObject.transform.Find("ground_dirt");
            //MeshRenderer mr = go.GetComponentInChildren<MeshRenderer>();
            //mr.material.SetColor("natureKit - leavesGreen", Color.red);

            GameObject go = collision.gameObject.transform.Find("pos").gameObject;
            go.SetActive(true);

            //go.SetActive(false);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            GameObject go = collision.gameObject.transform.Find("pos").gameObject;
            go.SetActive(false);

            //go.SetActive(false);

        }
    }
    */
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            MeshRenderer mr = transform.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            MeshRenderer mr = transform.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            MeshRenderer mr = transform.GetComponent<MeshRenderer>();
            mr.enabled = false;
        }
    }
    
    /*
    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.transform.Find("pos").gameObject;
        if (other.gameObject.tag == "Player")
        {
            go.SetActive(false);
        }
        if (other.gameObject.tag == "ground")
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        GameObject go = other.transform.Find("pos").gameObject;
        if (other.gameObject.tag == "ground")
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject go = other.transform.Find("pos").gameObject;
        if (other.gameObject.tag == "ground")
        {
            go.SetActive(true);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.enabled = false;
        }
    }*/
}
