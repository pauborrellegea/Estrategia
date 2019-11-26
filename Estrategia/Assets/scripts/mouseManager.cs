using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseManager : MonoBehaviour
{

    Transform seletedUnit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject ourHitObject = hitInfo.collider.transform.gameObject;

                Debug.Log("Raycast hit:" + hitInfo.transform.position);

                //Debug.Log("ourHitObject:" + ourHitObject.transform.position);

                if (hitInfo.transform.CompareTag("Player"))
                {
                    SelectUnit(hitInfo.transform);
                }
                else if (hitInfo.transform.CompareTag("ground"))
                {
                    MoveUnit(hitInfo.transform);
                }
                //Debug.Log("unidad elegida:" + seletedUnit.position);


            }
        }

    }

    public void MoveUnit(Transform cube)
    {
        seletedUnit.position = new Vector3(cube.position.x -5, seletedUnit.position.y, cube.position.z -5);
    }

    public void SelectUnit(Transform unit)
    {

        if (seletedUnit != null)
        {
            seletedUnit.Find("circulo").gameObject.SetActive(false);
        }

        seletedUnit = unit;
        seletedUnit.Find("circulo").gameObject.SetActive(true);
    }

}
