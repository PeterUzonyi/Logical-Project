using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int PlayerID;
    public string PlayerName;
    public int ActionCount; //Ha ez eléri a hármat, akkor egy másik játékosra kerül a sor
    public bool IsMyRound = false; //Ez a játékos van-e soron

    public GameObject BlockingPanel; //Ha másik játékos van soron, akkor SetActive(False), különben (True)

    public void MyTurn(bool value)
    {
        IsMyRound = value;

        if (IsMyRound)
        {//Ez a játékos van soron
            BlockingPanel.SetActive(false);
        }
        else
        {//Más játékos van soron
            BlockingPanel.SetActive(true);
        }
    }

    public void EndMyTurn()
    {
        FindAnyObjectByType<TurnManager>().EndTurn();
    }
}
