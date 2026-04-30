using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon;
using Photon.Pun;
using System.Linq;
using UnityEngine.Localization.Settings;

/// <summary>
/// This controlls what can a player do during the game (both online and local)
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// uniq ID
    /// </summary>
    public int PlayerID;

    /// <summary>
    /// Player's name
    /// </summary>
    public string PlayerName;

    /// <summary>
    /// Actions done is this round (maximum: 3)
    /// </summary>
    public int ActionCount;

    /// <summary>
    /// True, when it is this player's turn
    /// </summary>
    public bool IsMyRound = false;

    /// <summary>
    /// Transparent blocking panel
    /// </summary>
    public GameObject BlockingPanel;

    /// <summary>
    /// Player's score
    /// </summary>
    public TMP_Text Score;
    public int PlayerScore;

    /// <summary>
    /// Number of completed puzzles
    /// </summary>
    public int CompletedPuzzles = 0;

    /// <summary>
    /// Number of elements is the player's inventory
    /// </summary>
    public int RemainingElements = 0;

    /// <summary>
    /// The player's inventory
    /// </summary>
    public InventoryManager inventoryManager;

    /// <summary>
    /// The player's card slots (maximum 4 cards)
    /// </summary>
    [SerializeField]
    private CardLoader[] MyCardSlots = new CardLoader[4];

    /// <summary>
    /// The panel it self
    /// </summary>
    public GameObject PlayerPanel;

    /// <summary>
    /// The background image color
    /// </summary>
    [SerializeField] private Image panelBackground;

    /// <summary>
    /// The selected action
    /// </summary>
    public ActionType selectedAction;

    /// <summary>
    /// True, when the player already used master action in this round
    /// </summary>
    public bool masterActionUsed;

    /// <summary>
    /// True, when an action is finished
    /// </summary>
    public bool actionHasEnded;

    /// <summary>
    /// True, when an element is successfully placed on a puzzle card
    /// </summary>
    public bool ElementPlacementSuccessfull;

    /// <summary>
    /// Used cards, needs for the master action
    /// </summary>
    public HashSet<MyGrid> gridsUsedInMasterAction = new HashSet<MyGrid>();

    /// <summary>
    /// How many cards are there, when the player started master action
    /// </summary>
    public int masterActionCardCount = 0;

    public GameObject actionBtn;
    public GameObject changePlayerViewBtn;
    public GameObject exitActionBtn;
    public GameObject endMasterActionBtn;
    public GameObject endVegsoRendrakasBtn;

    public int PhotonActorNumber; // Photon ActorNumber tárolása

    [SerializeField] private UpgradePanel upgradePanel;

    //Called when the script is loaded
    void Awake()
    {
        PlayerScore = 0;
        RefreshScore(0);

        //Eltûnjenek az üres kártya prefabok
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            RemoveCard(i);
        }
    }

    /// <summary>
    /// If it is this player's trun, then able to choose actions.
    /// If it is not this player's turn, cannot do anything, just watching
    /// </summary>
    /// <param name="value"></param>
    public void MyTurn(bool value)
    {
        RefreshScore(0);
        IsMyRound = value;

        // Online módban csak akkor engedélyezzük, ha tényleg a mi actorunk van soron
        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            bool isLocalPlayer = PhotonActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
            bool actuallyMyTurn = isLocalPlayer && OnlineTurnManager.Instance.IsMyTurn;
            if (BlockingPanel != null)
            {
                BlockingPanel.SetActive(!actuallyMyTurn);
            }

            if (!actuallyMyTurn)
            {
                if (isLocalPlayer)
                {
                    if (FindAnyObjectByType<ActionSelectionPanel>() != null)
                    {
                        FindAnyObjectByType<ActionSelectionPanel>().HidePanel();
                    }
                }                
                return;
            }
        }
        else
        {// Lokális logika marad
            if (PlayerPanel != null)
            {
                
                PlayerPanel.SetActive(value);
            }
            if (BlockingPanel != null)
            {
                BlockingPanel.SetActive(!value);
            }
            
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
            if (FindAnyObjectByType<ActionSelectionPanel>() != null)
            {
                FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
            }
        }
        else
        {
            endVegsoRendrakasBtn.SetActive(true);
        }

        ThinkingTimer.Instance?.StartTimer();
    }

    /// <summary>
    /// True, when the player has 4 card (maximum: 4 cards)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// True, when the player has 0 cards (maximum: 4 cards)
    /// </summary>
    /// <returns></returns>
    public bool IsCardSlotsEmpty()
    {
        for (int i = 0; i < MyCardSlots.Length; i++)
        {
            if (MyCardSlots[i].CurrentCard != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Showing the chosen card from the common reserev (take a puzzle card action)
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Removing a card from the slots, happens when completing a puzzle card
    /// </summary>
    /// <param name="slotIndex"></param>
    public void RemoveCard(int slotIndex)
    {
        if (MyCardSlots == null || MyCardSlots[slotIndex] == null || slotIndex < 0 || slotIndex >= MyCardSlots.Length)
        {
            return;
        }

        MyCardSlots[slotIndex].CurrentCard = null;
        MyCardSlots[slotIndex].ResetGrid();
        MyCardSlots[slotIndex].gameObject.SetActive(false);
    }

    /// <summary>
    /// Stores the chosen action type
    /// </summary>
    /// <param name="action"></param>
    public void SetSelectedAction(ActionType action)
    {
        selectedAction = action;
        UseAction();
    }

    /// <summary>
    /// The player doing the chosen action
    /// </summary>
    private void UseAction()
    {
        if (selectedAction == ActionType.TakePuzzle)
        {
            TakePuzzle();
        }
        else if (selectedAction == ActionType.TakeElement)
        {
            TakeElement();
        }
        else if (selectedAction == ActionType.UpgradeElement)
        {
            UpgradeElement();
        }
        else if (selectedAction==ActionType.PlaceElement)
        {
            exitActionBtn.SetActive(true);
        }
        else if (selectedAction == ActionType.MasterAction && masterActionUsed == false)
        {
            MasterAction();
        }
        else
        {
            FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        }
    }

    /// <summary>
    /// Once an action is finished, the actionCount count it. 
    /// Once it reaches 3, then the next player's trun begins
    /// </summary>
    public void ActionHasEnded()
    {
        if (!ThinkingTimer.Instance.TimeIsUp)
        {
            //A Timer nem járt le
            if (selectedAction == ActionType.PlaceElement && ElementPlacementSuccessfull)
            {
                ElementPlacementSuccessfull = false;
            }
            else if (selectedAction == ActionType.PlaceElement && !ElementPlacementSuccessfull)
            {
                FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
                return;
            }
        }

        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            if (!ThinkingTimer.Instance.TimeIsUp)
            {
                OnlineTurnManager.Instance.SyncResetTimer();
            }
        }
        else
        {
            ThinkingTimer.Instance?.ResetTimer();
        }

        //Esetleges gombok eltüntetése
        actionBtn.SetActive(false);
        exitActionBtn.SetActive(false);
        endMasterActionBtn.SetActive(false);
        endVegsoRendrakasBtn.SetActive(false);
        if (upgradePanel != null && upgradePanel.gameObject.activeSelf)
        {
            upgradePanel.Close();
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
                ThinkingTimer.Instance?.StopTimer();
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

    /// <summary>
    /// Take a new puzzle from the common reserve (take puzzle action)
    /// </summary>
    private void TakePuzzle()
    {
        exitActionBtn.SetActive(true);
        CommonReserve.Instance.CommonReserveBlockingPanel.SetActive(false);
        OpenCommonReserve();
    }

    /// <summary>
    /// Takeing a lvl1 element from the common reserve's inventory (take an element action)
    /// </summary>
    private void TakeElement()
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

    /// <summary>
    /// Upgrades a player inventory element to another element from the common reserve's inventory (upgrade acion)
    /// </summary>
    private void UpgradeElement()
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

    /// <summary>
    /// The player can place down maximum 1 element into every puzzle (master action)
    /// </summary>
    private void MasterAction()
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

    /// <summary>
    /// This player's turn is over, the next player is up next
    /// </summary>
    public void EndMyTurn()
    {
        ThinkingTimer.Instance?.StopTimer();

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public CardLoader GetCardLoaderBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MyCardSlots.Length)
        {
            return null;
        }

        return MyCardSlots[slotIndex];
    }

    /// <summary>
    /// When the player gets reward score or loses during the Final Touches (Végsõ rendrakás) it desplays the change
    /// </summary>
    /// <param name="value"></param>
    public void RefreshScore(int value)
    {
        PlayerScore += value;

        if (Score != null)
        {
            string code = LocalizationSettings.SelectedLocale.Identifier.Code;

            if (code == "hu")
            {
                Score.text = PlayerName + " pontszáma: \n" + PlayerScore.ToString();
            }
            else
            {
                Score.text = PlayerName + "'s score: \n" + PlayerScore.ToString();
            }
        }
    }

    /// <summary>
    /// Changing the view from the player, to the common reserve
    /// </summary>
    public void OpenCommonReserve()
    {
        if (CommonReserve.Instance != null)
        {
            CommonReserve.Instance.Open(this);
        }
        
    }

    /// <summary>
    /// Changing the view from the player, to the next player in line
    /// </summary>
    public void OnChangePlayerViewClicked()
    {
        int next = (PlayerID % TurnManager.Instance.playerCount);

        PlayerPanel.SetActive(false);
        TurnManager.Instance.players[next].PlayerPanel.SetActive(true);
    }

    /// <summary>
    /// Cancelling TakePuzzle and PlaceElement actions
    /// </summary>
    public void OnExitActionClicked()
    {
        exitActionBtn.SetActive(false);
        
        if (selectedAction == ActionType.PlaceElement)
        {
            Debug.Log("PlaceElement was cancelled");
            ElementPlacementSuccessfull = false;
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
        else
        {
            Debug.Log("TakePuzzle was cancelled");
            ActionCount--;
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
    }

    /// <summary>
    /// Finishing the master action before placing into every puzzle card (master action)
    /// </summary>
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

    /// <summary>
    /// Finishing the Final Touches (Végsõ rendrakás)
    /// </summary>
    public void OnEndVegsoRendrakasClicked()
    {
        endVegsoRendrakasBtn.SetActive(false);
        RefreshScore(ActionCount * -1);
        EndMyTurn();
    }

    /// <summary>
    /// Online mode. Sends the player stats for the final standings once the game is over
    /// </summary>
    public void SyncStatsToAll()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        // Csak a saját kliensünk küldi el a saját adatait
        if (PhotonActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            return;
        }

        OnlineTurnManager.Instance.SyncPlayerStats(PlayerID, PlayerScore, CompletedPuzzles, RemainingElements);
    }

    /// <summary>
    /// Sets the background color for the player
    /// </summary>
    public void ApplyColor()
    {
        if (panelBackground != null)
        {
            panelBackground.color = GameConfig.PlayerColors[PlayerID - 1];
        }            
    }
}
