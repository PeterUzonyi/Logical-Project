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

    void Awake()
    {
        Instance = this;
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
    }
}
