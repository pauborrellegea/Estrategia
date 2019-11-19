using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCamara : MonoBehaviour {

    public Transform objetivo;
    public float suavidad = 5f;

    Vector3 offset;
    Vector3 targetCamPos;
    Camera camara;

    void Awake()
    {
        offset = transform.position - objetivo.position;
        //camaraZoom.transform.localPosition = new Vector3(0, 5.6f, 0);
        camara = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        targetCamPos = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, suavidad + Time.deltaTime);
    }
}
