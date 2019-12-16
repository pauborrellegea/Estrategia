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

    public bool hasAttacked;
    public int remainingMoves;

    public bool dead = false;

    public UnitType type;

    public void SetPlayer(bool p, Material m)
    {
        player = p;
        hasAttacked = false;
        remainingMoves = rangoDeMovimiento;

        transform.GetChild(0).GetComponent<Renderer>().material = m;
    }

    public void ReceiveDamage(int amount)
    {
        vida -= amount;
        if (vida < 0)
        {
            dead = true;
        }
    }

    public void substractMoves(int amount)
    {
        remainingMoves -= amount;
    }

    public void Attacked()
    {
        hasAttacked = true;
    }

    public void ResetTurn()
    {
        hasAttacked = false;
        remainingMoves = rangoDeMovimiento;
    }

    public bool IsDead()
    {
        return dead;
    }
}
