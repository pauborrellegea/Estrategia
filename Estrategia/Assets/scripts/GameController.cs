using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Player player;
    public Player ia;

    public Unit[] spawnableUnits;

    private float timePerTurn = 20f;
    private float ticks;

    private int turnsElapsed;

    private int startCoins = 100;
    public int coinsPerTurn; //cada turno la recompensa aumenta


    private bool playerTurn; //de quien es el turno (true=player, false=ia)

    GridController gridController;

    public Text timerText, endText;

    public GameObject canvasGame, canvasLose;

    private void Awake()
    {
        ticks = 0f;
        gridController = GetComponent<GridController>();
        playerTurn = true; //aleatorio?

        coinsPerTurn = 20;

        turnsElapsed = 0;
        canvasLose.SetActive(false);
        canvasGame.SetActive(true);
    }

    private void Start()
    {
        player.AddCoins(startCoins);
        ia.AddCoins(startCoins);

        player.setOtherBase(1, gridController.cols - 2);
        ia.setOtherBase(gridController.rows - 2, 1);

        player.setSpawn(gridController.rows - 1, 0);
        ia.setSpawn(0, gridController.cols - 1);
    }

    public bool turnOfPlayer()
    {
        return playerTurn;
    }

    public void Update()
    {
        if (playerTurn)
        {
            timerText.color = Color.blue;
            timerText.text = "Your turn: " + Mathf.Round(ticks * 100f) / 100f;

        }
        else
        {
            timerText.color = Color.red;
            timerText.text = "Other turn: " + Mathf.Round(ticks * 100f) / 100f;
        }
    }

    private void FixedUpdate()
    {
        ticks += Time.fixedDeltaTime;

        if (ticks >= timePerTurn)
        {
            EndTurn();
        }
    }
    
    public void EndTurn()
    {
        turnsElapsed += 1;
        if (turnsElapsed >= 2)
        {
            coinsPerTurn += 2;
            turnsElapsed -= 2;
        }

        gridController.resetAllUnits();

        playerTurn = !playerTurn;
        ticks = 0f;
        if (playerTurn)
        {
            ia.AddCoins(coinsPerTurn);
            ia.resetEndTurn();
        }
        else
        {
            player.AddCoins(coinsPerTurn);
            player.resetEndTurn();
        }
    }

    public void AttackBase(int amount, bool p)
    {
        if (p)
        {
            ia.baseAttacked(amount);
        }
        else
        {
            player.baseAttacked(amount);
        }
    }

    public void LoseGame(bool loses)
    {
        player.enabled = false;
        ia.enabled = false;
        canvasLose.SetActive(true);
        canvasGame.SetActive(false);

        if (loses)
        {
            endText.text = "YOU LOSE";
            endText.color = Color.red;
        }
        else
        {
            endText.text = "YOU WIN";
            endText.color = Color.blue;
        }
    }
}
