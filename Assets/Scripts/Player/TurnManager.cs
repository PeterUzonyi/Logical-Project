using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public Player player1;
    public Player player2;

    private Player currentPlayer;//Soron lévõ játékos

    void Start()
    {
        currentPlayer = player1;
        player1.MyTurn(true);
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
