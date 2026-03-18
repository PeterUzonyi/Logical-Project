using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public Player player1;
    public Player player2;

    public int playerCount = 2;

    public Player currentPlayer { get; private set; } //Soron lévõ játékos

    public bool isLastRound = false;
    public Player startedLastRound;
    public int lastPlayerTurn;

    public bool isVegsoRendrakas;

    public bool isGameOver = false;

    void Awake()
    {
        Instance = this;
        isLastRound = false;
        isGameOver = false;
        lastPlayerTurn = 0;
        isVegsoRendrakas = false;
    }
    void Start()
    {
        currentPlayer = player1;
        //A CommonReserve Inicializálása miatt kell
        currentPlayer.OpenCommonReserve();
        player1.MyTurn(true);
        player2.MyTurn(false);
        playerCount = 2;
    }

    public void EndTurn()
    {
        if (currentPlayer == startedLastRound && lastPlayerTurn > 1 && isLastRound)
        {
            VegsoRendrakas();
        }

        if (currentPlayer == startedLastRound && lastPlayerTurn > 1 && isVegsoRendrakas)
        {
            isGameOver = true;
            Debug.Log("Game Over");
        }

        if (currentPlayer == player1) 
        {
            currentPlayer = player2;
            player1.MyTurn(false);
            player2.MyTurn(true);
        }
        else 
        {
            currentPlayer = player1;
            player1.MyTurn(true);
            player2.MyTurn(false);
        }

        if (isLastRound)
        {
            lastPlayerTurn++;
        }
        
    }

    public void LastRound()
    {
        isLastRound = true;
        startedLastRound = currentPlayer;
    }

    public void VegsoRendrakas()
    {
        Debug.Log("Végsõ Rendrakás");
        isVegsoRendrakas = true;
        lastPlayerTurn = 0;
        startedLastRound = currentPlayer;
    }
}
