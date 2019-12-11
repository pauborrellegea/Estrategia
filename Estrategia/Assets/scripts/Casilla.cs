using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casilla : MonoBehaviour
{
    public enum CellType { GRASS, FOREST, MOUNTAIN, TOWER}

    public CellType type;
    GameObject fog;
    GameObject movement;

    private void Awake()
    {
        fog = transform.GetChild(0).gameObject;
        movement = transform.GetChild(1).gameObject;

        fog.SetActive(true);
        movement.SetActive(false);
    }

    public void EnableFog(bool enable)
    {
        fog.SetActive(enable);
    }

    public void EnableIndicator(bool enable)
    {
        movement.SetActive(enable);
    }

    public bool isGrass()
    {
        return type == CellType.GRASS;
    }

    public bool isForest()
    {
        return type == CellType.FOREST;
    }

    public void setColor(bool attack)
    {
        if (attack)
        {
            movement.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
        } else
        {
            movement.GetComponent<Renderer>().material.color = new Color(1f, 0f, 0f);
        }
    }
}
