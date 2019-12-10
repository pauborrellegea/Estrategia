using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseManager : MonoBehaviour
{
    private GridController gridController;

    LayerMask layerMask;

    public GameObject circuloSeleccion;
    public GameObject circuloHighlight;

    private static Unit selectedUnit;
    //[HideInInspector] public bool prueba;

    private int selectedX, selectedZ;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");

        GameObject escenario = GameObject.Find("Escenario");

        circuloSeleccion.SetActive(false);
        circuloHighlight.SetActive(false);

        gridController = escenario.GetComponent<GridController>();
    }

    // Update is called once per frame
    void Update()
    {
        //FALTA: ATACAR

        if (Input.GetMouseButtonDown(0))
        {
            circuloHighlight.SetActive(false);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Transform hitTransform = hitInfo.transform;

                //coordenadas de unidad o de casilla
                selectedX = (int)hitTransform.position.x;
                selectedZ = (int)hitTransform.position.z;



                Unit selection = gridController.GetUnit(selectedX, selectedZ); //puede devolver null

                if (selection != null && selectedUnit != selection) //seleccionar
                {
                    selectedUnit = selection;
                    circuloSeleccion.SetActive(true);
                    circuloSeleccion.transform.position = selectedUnit.transform.position;

                    gridController.SetMovement(selectedUnit);
                }
                else //mover seleccion
                {
                    if (selectedUnit != null)
                    {
                        if (gridController.CanMove(selectedX, selectedZ))
                        {
                            gridController.MoveUnit(selectedUnit, selectedX, selectedZ);
                            //Debug.Log("si");
                            selectedUnit = null;
                            circuloSeleccion.SetActive(false);

                            gridController.SetMovement(null);
                        }
                    }
                }

                //Debug.Log("Raycast hit: " + hitInfo.transform.position);
                //Debug.Log("ourHitObject: " + hitInfo.transform.tag);
                /*
                if (hitInfo.transform.CompareTag("Player") && !hitInfo.collider.isTrigger)
                {
                    //inSelectionUnit(hitInfo.transform.gameObject);
                    SelectUnit(hitInfo.transform);
                }
                else if (hitInfo.transform.CompareTag("ground"))
                {
                    //inDeselectionUnit(hitInfo.transform.gameObject);
                    MoveUnit(hitInfo.transform);

                }*/
                //Debug.Log("unidad elegida:" + seletedUnit.position);


            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Transform hitTransform = hitInfo.transform;

                //coordenadas de unidad o de casilla
                selectedX = (int)hitTransform.position.x;
                selectedZ = (int)hitTransform.position.z;

                circuloHighlight.transform.position = new Vector3(selectedX, 0, selectedZ);

                //highlight
                if (circuloSeleccion.transform.position!= circuloHighlight.transform.position)
                    circuloHighlight.SetActive(true);
                else
                    circuloHighlight.SetActive(false);
            }
            else
            {
                circuloHighlight.SetActive(false);
            }
        }

    }

    /*
    public void inSelectionUnit(GameObject unit)
    {
        GameObject go = unit.transform.Find("x+1").gameObject;
        go.SetActive(true);
        unit.transform.Find("x-1").gameObject.SetActive(true);
        unit.transform.Find("y+1").gameObject.SetActive(true);
        unit.transform.Find("y-1").gameObject.SetActive(true);
    }
    public void inDeselectionUnit(GameObject unit)
    {
        GameObject go = selectedUnit.Find("x+1").gameObject;
        go.SetActive(false);
        selectedUnit.Find("x-1").gameObject.SetActive(false);
        selectedUnit.Find("y+1").gameObject.SetActive(false);
        selectedUnit.Find("y-1").gameObject.SetActive(false);
    }

    public void MoveUnit(Transform cube)
    {

        if (cube.position.x-5 == selectedUnit.position.x && (cube.position.z-5 == selectedUnit.position.z+10 || cube.position.z-5 == selectedUnit.position.z - 10))
        {
            selectedUnit.position = new Vector3(cube.position.x - 5, selectedUnit.position.y, cube.position.z - 5);
        }
        else if (cube.position.z - 5 == selectedUnit.position.z && (cube.position.x - 5 == selectedUnit.position.x + 10 || cube.position.x - 5 == selectedUnit.position.x - 10))
        {
            selectedUnit.position = new Vector3(cube.position.x - 5, selectedUnit.position.y, cube.position.z - 5);
        }
        else
        {
            Debug.Log("Prueba otro movimiento");
        }
        
    }

    public void SelectUnit(Transform unit)
    {

        if (selectedUnit != null)
        {
            selectedUnit.Find("circulo").gameObject.SetActive(false);
        }

        selectedUnit = unit;
        selectedUnit.Find("circulo").gameObject.SetActive(true);

        //casillas posibles adyacentes

        //if(seletedUnit.Find("casillaX+1").gameObject.)
        //Debug.Log("no"+ seletedUnit.Find("casillaX+1").gameObject.get);
    }*/


}
