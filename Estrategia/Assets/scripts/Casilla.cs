using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casilla : MonoBehaviour
{
    public enum CellType { GRASS, FOREST, MOUNTAIN, RIVER}

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
}
