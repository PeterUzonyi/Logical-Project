using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon;
using Photon.Pun;

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
    [SerializeField] private Image panelBackground;

    public ActionType selectedAction;
    public bool masterActionUsed;
    public bool actionHasEnded;

    public HashSet<MyGrid> gridsUsedInMasterAction = new HashSet<MyGrid>();
    public int masterActionCardCount = 0;

    public GameObject actionBtn;
    public GameObject endMasterActionBtn;
    public GameObject endVegsoRendrakasBtn;

    void Awake()
    {
        if (panelBackground != null)
        {
            panelBackground.color = GameConfig.PlayerColors[PlayerID - 1];
        }            

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

        // Online módban csak akkor engedélyezzük, ha tényleg a mi actorunk van soron
        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            bool actuallyMyTurn = value && OnlineTurnManager.Instance.IsMyTurn;
            BlockingPanel.SetActive(!actuallyMyTurn);
            PlayerPanel.SetActive(actuallyMyTurn);

            if (!actuallyMyTurn)
            {
                return;
            }
        }
        else
        {
            // Lokális logika marad
            PlayerPanel.SetActive(value);
            BlockingPanel.SetActive(!value);
            if (!value)
            {
                return;
            }
        }

        // Közös kör-kezdõ logika
        ActionCount = 0;
        masterActionUsed = false;
        actionHasEnded = false;

        if (!TurnManager.Instance.isVegsoRendrakas)
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
        else
        {
            endVegsoRendrakasBtn.SetActive(true);
        }

        /*
        if (IsMyRound)
        {//Ez a játékos van soron
            PlayerPanel.SetActive(true);
            BlockingPanel.SetActive(false);

            ActionCount = 0;
            masterActionUsed = false;
            actionHasEnded = false;

            
        }
        else
        {//Más játékos van soron
            PlayerPanel.SetActive(false);
            BlockingPanel.SetActive(true);
        }
        */
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
        else if (selectedAction==ActionType.PlaceElement)
        {

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

        if (CardManager.Instance.BlackCards.Count == 0 && TurnManager.Instance.isLastRound == false)
        {
            Debug.Log("Utolsó kör eleje: " + TurnManager.Instance.currentPlayer);
            TurnManager.Instance.LastRound();
        }

        if (!TurnManager.Instance.isVegsoRendrakas)
        {
            if (ActionCount == 3)//Kör vége, megvolt a 3 akció
            {
                EndMyTurn();
            }
            else//Még nem volt meg a 3 akció, következõ akció
            {
                FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
            }
        }
        else
        {
            Debug.Log("Mínusz pontok: -" + ActionCount);
        }
    }

    public void TakePuzzle()
    {
        CommonReserve.Instance.CommonReserveBlockingPanel.SetActive(false);
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
        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            // Online: jelezzük a szervernek hogy végeztünk
            OnlineTurnManager.Instance.SubmitMove();
            // A TurnManager.EndTurn()-t az OnOnlineTurnChanged fogja meghívni
        }
        else
        {
            // Lokális játék: marad a régi logika
            FindAnyObjectByType<TurnManager>().EndTurn();
        }
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
        if (gridsUsedInMasterAction.Count != 0)
        {
            masterActionUsed = true;
            endMasterActionBtn.SetActive(false);
            Debug.Log("MasterAction has Ended");
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
        else
        {
            masterActionUsed = false;
            endMasterActionBtn.SetActive(false);
            Debug.Log("MasterAction was cancelled");
            ActionCount--;
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
        
    }

    public void OnEndVegsoRendrakasClicked()
    {
        RefreshScore(ActionCount * -1);
        EndMyTurn();
    }
}
