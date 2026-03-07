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

    [SerializeField]
    private CardLoader[] MyCardSlots = new CardLoader[4];

    void Awake()
    {
        RefreshScore(0);
    }
    public void MyTurn(bool value)
    {
        IsMyRound = value;

        //Eltûnjenek az üres kártya prefabok
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            RemoveCard(i);
        }

        if (IsMyRound)
        {//Ez a játékos van soron
            BlockingPanel.SetActive(false);
        }
        else
        {//Más játékos van soron
            BlockingPanel.SetActive(true);
        }
    }

    public bool IsCardSlotsFull()
    {
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            if (MyCardSlots[i].CurrentCard == null)
            {
                return false;
            }
        }
        return true;
    }

    // Kártya átvétele a CommonReserve-bõl
    public bool ReceiveCard(CardType card)
    {
        // Megkeresi az elsõ üres slotot
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            if (MyCardSlots[i].CurrentCard == null)
            {
                MyCardSlots[i].gameObject.SetActive(true);
                MyCardSlots[i].ShowCard(card);
                return true;
            }
        }

        Debug.LogWarning($"{PlayerName} keze tele van, nem lehet több lapot felvenni!");
        return false;
    }

    // Kártya eltávolítása egy slotból (ha megoldja a lapot)
    public void RemoveCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MyCardSlots.Length)
        {
            return;
        }

        MyCardSlots[slotIndex].CurrentCard = null;
        MyCardSlots[slotIndex].ResetGrid();
        MyCardSlots[slotIndex].gameObject.SetActive(false);
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
