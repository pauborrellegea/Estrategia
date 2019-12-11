using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int ataque;
    public int vision;
    public int rangoDeAtaque;
    public int rangoDeMovimiento;
    public int vida;
    public int coste;

    public bool player;

    public void SetPlayer(bool p)
    {
        player = p;
    }

    public void ReceiveDamage(int amount)
    {
        vida -= amount;
        if (vida < 0)
        {
            Destroy(gameObject);
        }
    }
}
