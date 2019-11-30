using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseManager : MonoBehaviour
{

    [HideInInspector] public Transform seletedUnit;
    [HideInInspector] public bool prueba;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject ourHitObject = hitInfo.collider.transform.gameObject;

                //Debug.Log("Raycast hit:" + hitInfo.transform.position);
                //Debug.Log("ourHitObject:" + ourHitObject.transform.position);
                //Debug.Log("ourHitObject:" + ourHitObject.transform.tag);

                if (hitInfo.transform.CompareTag("Player"))
                {
                    //inSelectionUnit(hitInfo.transform.gameObject);
                    SelectUnit(hitInfo.transform);
                }
                else if (hitInfo.transform.CompareTag("ground"))
                {
                    //inDeselectionUnit(hitInfo.transform.gameObject);
                    MoveUnit(hitInfo.transform);

                }
                //Debug.Log("unidad elegida:" + seletedUnit.position);


            }
        }

    }

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
        GameObject go = seletedUnit.Find("x+1").gameObject;
        go.SetActive(false);
        seletedUnit.Find("x-1").gameObject.SetActive(false);
        seletedUnit.Find("y+1").gameObject.SetActive(false);
        seletedUnit.Find("y-1").gameObject.SetActive(false);
    }

    public void MoveUnit(Transform cube)
    {
        if (cube.position.x-5 == seletedUnit.position.x && (cube.position.z-5 == seletedUnit.position.z+10 || cube.position.z-5 == seletedUnit.position.z - 10))
        {
            seletedUnit.position = new Vector3(cube.position.x - 5, seletedUnit.position.y, cube.position.z - 5);
        }
        else if (cube.position.z - 5 == seletedUnit.position.z && (cube.position.x - 5 == seletedUnit.position.x + 10 || cube.position.x - 5 == seletedUnit.position.x - 10))
        {
            seletedUnit.position = new Vector3(cube.position.x - 5, seletedUnit.position.y, cube.position.z - 5);
        }
        else
        {
            Debug.Log("Prueba otro movimiento");
        }
        
    }

    public void SelectUnit(Transform unit)
    {

        if (seletedUnit != null)
        {
            seletedUnit.Find("circulo").gameObject.SetActive(false);
        }

        seletedUnit = unit;
        seletedUnit.Find("circulo").gameObject.SetActive(true);

        //casillas posibles adyacentes

        //if(seletedUnit.Find("casillaX+1").gameObject.)
        //Debug.Log("no"+ seletedUnit.Find("casillaX+1").gameObject.get);
    }


}
