using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public int PlayerID;
    public string PlayerName;
    public int ActionCount; //Ha ez eléri a hármat, akkor egy másik játékosra kerül a sor
    public bool IsMyRound = false; //Ez a játékos van-e soron

    public GameObject BlockingPanel; //Ha másik játékos van soron, akkor SetActive(False), különben (True)

    public TMP_Text Score;
    public int PlayerScore = 0;

    public InventoryManager inventoryManager;

    void Awake()
    {
        RefreshScore(0);
    }
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

    public void RefreshScore(int value)
    {
        PlayerScore += value;
        if(Score.text != PlayerScore.ToString())
        {
            Score.text = PlayerScore.ToString();
        }
    }
}
