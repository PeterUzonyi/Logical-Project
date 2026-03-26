using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon;
using Photon.Pun;
using System.Linq;

public class Player : MonoBehaviour
{
    public int PlayerID;
    public string PlayerName;
    public int ActionCount; //Ha ez eléri a hármat, akkor egy másik játékosra kerül a sor
    public bool IsMyRound = false; //Ez a játékos van-e soron

    public GameObject BlockingPanel; //Ha másik játékos van soron, akkor SetActive(False), különben (True)

    public TMP_Text Score;
    public int PlayerScore = 0;
    public int CompletedPuzzles = 0;
    public int RemainingElements = 0;

    public InventoryManager inventoryManager;

    [SerializeField]
    private CardLoader[] MyCardSlots = new CardLoader[4];

    public GameObject PlayerPanel;
    [SerializeField] private Image panelBackground;

    public ActionType selectedAction;
    public bool masterActionUsed;
    public bool actionHasEnded;
    public bool ElementPlacementSuccessfull;

    public HashSet<MyGrid> gridsUsedInMasterAction = new HashSet<MyGrid>();
    public int masterActionCardCount = 0;

    public GameObject actionBtn;
    public GameObject changePlayerViewBtn;
    public GameObject endMasterActionBtn;
    public GameObject endVegsoRendrakasBtn;

    public int PhotonActorNumber; // Photon ActorNumber tárolása

    [SerializeField] private UpgradePanel upgradePanel;

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
        Debug.Log($"MyTurn: {PlayerID}, value={value}, " +
              $"IsMyTurn={OnlineTurnManager.Instance?.IsMyTurn}, " +
              $"LocalActorNumber={PhotonNetwork.LocalPlayer.ActorNumber}, " +
              $"ActiveActor={OnlineTurnManager.Instance?.ActiveActorNumber}");

        IsMyRound = value;

        // Online módban csak akkor engedélyezzük, ha tényleg a mi actorunk van soron
        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            bool isLocalPlayer = PhotonActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
            bool actuallyMyTurn = isLocalPlayer && OnlineTurnManager.Instance.IsMyTurn;
            BlockingPanel.SetActive(!actuallyMyTurn);

            if (!actuallyMyTurn)
            {
                if (isLocalPlayer)
                {
                    FindAnyObjectByType<ActionSelectionPanel>().HidePanel();
                }                
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
        ElementPlacementSuccessfull = false;

        if (!TurnManager.Instance.isVegsoRendrakas)
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
        else
        {
            endVegsoRendrakasBtn.SetActive(true);
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
        if (selectedAction == ActionType.PlaceElement && ElementPlacementSuccessfull)
        {
            ElementPlacementSuccessfull = false;
        }
        else if (selectedAction == ActionType.PlaceElement && !ElementPlacementSuccessfull)
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
            return;
        }

        ActionCount++;
        Debug.Log(ActionCount);

        if (CardManager.Instance.BlackCards.Count == 0 && TurnManager.Instance.isLastRound == false)
        {
            if (!TurnManager.Instance.isLastRound&&!TurnManager.Instance.isVegsoRendrakas)
            {
                Debug.Log("Utolsó kör eleje: " + TurnManager.Instance.currentPlayer);
                TurnManager.Instance.LastRound();
            }
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

        RemainingElements = 0;
        foreach (var item in inventoryManager.GetAllItems())
        {
            RemainingElements += item.quantity;
        }
    }

    public void TakePuzzle()
    {
        CommonReserve.Instance.CommonReserveBlockingPanel.SetActive(false);
        OpenCommonReserve();
    }

    public void TakeElement()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Online mód
            CommonReserve.Instance.RequestTakeElement(PlayerID);
        }
        else
        {
            //Lokális mód
            if (CommonReserve.Instance.TakeFromInventory(0, 1))
            {
                InventoryItem item = inventoryManager.GetItemById(0);
                item.quantity++;
            }

            Debug.Log("TakeElement has Ended");
            ActionHasEnded();
        }
    }

    public void UpgradeElement()
    {
        PlayerPanel.SetActive(false);

        // Ha az Instance még null (inaktív panel), aktiváljuk a direkt referencián keresztül
        if (upgradePanel != null)
        {
            upgradePanel.gameObject.SetActive(true);
            upgradePanel.Open(this);
        }
        else if (UpgradePanel.Instance != null)
        {
            UpgradePanel.Instance.Open(this);
        }
        else
        {
            Debug.LogError("UpgradePanel nem található!");
        }
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

    public CardLoader GetCardLoaderBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MyCardSlots.Length) return null;
        return MyCardSlots[slotIndex];
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

    public void OnChangePlayerViewClicked()
    {
        int next = (PlayerID % TurnManager.Instance.playerCount);

        PlayerPanel.SetActive(false);
        TurnManager.Instance.players[next].PlayerPanel.SetActive(true);
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
        endVegsoRendrakasBtn.SetActive(false);
        RefreshScore(ActionCount * -1);
        EndMyTurn();
    }

    public void SyncStatsToAll()
    {
        if (!PhotonNetwork.IsConnected) return;

        // Csak a saját kliensünk küldi el a saját adatait
        if (PhotonActorNumber != PhotonNetwork.LocalPlayer.ActorNumber) return;

        OnlineTurnManager.Instance.SyncPlayerStats(
            PlayerID,
            PlayerScore,
            CompletedPuzzles,
            RemainingElements);
    }
}
