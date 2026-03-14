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

    public GameObject PlayerPanel;

    public ActionType selectedAction;
    public bool masterActionUsed;
    public bool actionHasEnded;

    public HashSet<MyGrid> gridsUsedInMasterAction = new HashSet<MyGrid>();
    public int masterActionCardCount = 0;

    public GameObject actionBtn;
    public GameObject endMasterActionBtn;

    void Awake()
    {
        RefreshScore(0);

        //Eltûnjenek az üres kártya prefabok
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            RemoveCard(i);
        }
    }
    public void MyTurn(bool value)
    {
        IsMyRound = value;

        if (IsMyRound)
        {//Ez a játékos van soron
            PlayerPanel.SetActive(true);
            BlockingPanel.SetActive(false);

            ActionCount = 0;
            masterActionUsed = false;
            actionHasEnded = false;

            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
        else
        {//Más játékos van soron
            PlayerPanel.SetActive(false);
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


    public void SetSelectedAction(ActionType action)//Kiválasztott Akció
    {
        selectedAction = action;
        UseAction();
    }

    public void UseAction()//Akció végrehajtása
    {
        if (selectedAction == ActionType.TakePuzzle)
        {
            TakePuzzle();
        }
        else if (selectedAction == ActionType.TakeElement)
        {
            TakeElement();
        }
        else if (selectedAction == ActionType.UpdrageElement)
        {
            UpgradeElement();
        }
        else if (selectedAction == ActionType.MesterAction && masterActionUsed == false)
        {
            MasterAction();
        }
        else
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
    }

    public void ActionHasEnded()
    {
        ActionCount++;
        Debug.Log(ActionCount);

        if (ActionCount == 3)//Kör vége, megvolt a 3 akció
        {
            EndMyTurn();
        }
        else//Még nem volt meg a 3 akció, következõ akció
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
    }

    public void TakePuzzle()
    {
        OpenCommonReserve();
    }

    public void TakeElement()
    {
        if (CommonReserve.Instance.TakeFromInventory(0, 1))
        {
            InventoryItem item = inventoryManager.GetItemById(0);
            item.quantity++;
        }

        Debug.Log("TakeElement has Ended");
        ActionHasEnded();
    }

    public void UpgradeElement()
    {
        PlayerPanel.SetActive(false);
        UpgradePanel.Instance.Open(this);
        //ActionHasEnded();
    }

    public void MasterAction()
    {
        gridsUsedInMasterAction.Clear();

        masterActionCardCount = 0;
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            if (MyCardSlots[i].CurrentCard != null)
            {
                masterActionCardCount++;
            }
        }
    }

    public void EndMyTurn()
    {
        FindAnyObjectByType<TurnManager>().EndTurn();
        Debug.Log("Másik játékos köre");
    }

    public void RefreshScore(int value)
    {
        PlayerScore += value;
        if(Score.text != PlayerScore.ToString())
        {
            Score.text = PlayerScore.ToString();
        }
    }

    public void OpenCommonReserve()
    {
        CommonReserve.Instance.Open(this);
    }

    public void OnEndMasterActionClicked()
    {
        masterActionUsed = true;
        endMasterActionBtn.SetActive(false);
        Debug.Log("MasterAction has Ended");
        TurnManager.Instance.currentPlayer.ActionHasEnded();
    }
}
