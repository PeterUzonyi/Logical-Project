using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Photon.Pun;
using UnityEngine.Localization.Settings;

/// <summary>
/// This is where players gets their reward elements and puzzle cards.
/// </summary>
public class CommonReserve : MonoBehaviourPun
{
    public static CommonReserve Instance { get; private set; }

    /// <summary>
    /// 8 card slots (4 white and 4 black) theese card are face up
    /// </summary>
    [SerializeField]
    private CardLoader[] cardSlots = new CardLoader[8];

    /// <summary>
    /// Inventory of the common reserve
    /// </summary>
    public InventoryManager inventoryManager;

    /// <summary>
    /// Its panel
    /// </summary>
    public GameObject CommonReservePanel;

    /// <summary>
    /// Its transparent blocking panel
    /// </summary>
    public GameObject CommonReserveBlockingPanel;

    /// <summary>
    /// Whick player opens the common reserve (need for changing the view)
    /// </summary>
    private Player originPlayer;

    /// <summary>
    /// True, when the common reserve is initialized
    /// </summary>
    private bool CommonReserveReady = false;

    /// <summary>
    /// Visualizing the remaining number of cards
    /// </summary>
    public TMP_Text RemainingBlackCards;
    public TMP_Text RemainingWhiteCards;

    //Called when the script is loaded
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CommonReserveBlockingPanel.SetActive(true);
    }

    //Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeSlotsWhenReady());
        StartCoroutine(LockInventoryWhenReady());
    }

    /// <summary>
    /// Wait for the slots and the cards to be initialized
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeSlotsWhenReady()
    {
        while (CardManager.Instance == null || !CardManager.Instance.IsReady)
        {
            yield return null;
        }

        // Megvárjuk, hogy minden CardLoader coroutine-ja is lefusson
        foreach (var slot in cardSlots)
        {
            if (slot == null)
            {
                continue;
            }
            while (slot.gridScript == null || !slot.gridScript.isInitialized)
            {
                yield return null;
            }
        }

        InitializeSlots();
    }

    /// <summary>
    /// Showing the top 4 card of both (white and black) decks
    /// </summary>
    private void InitializeSlots()
    {
        // Minden slotba húz egy lapot a CardManager-bõl
        // Az elsõ 4 slot fehér, a következõ 4 fekete lapot kap
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null)
            {
                continue;
            }

            string color;
            if (i < 4)
            {
                color = "White";
            }
            else
            {
                color = "Black";
            }

            CardType card = CardManager.Instance.DrawCard(color);

            if (card != null)
            {
                cardSlots[i].ShowCard(card);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false); // pakli üres
            }

            RefreshRemainingCardCount();
        }
    }

    /// <summary>
    /// Wait for the inventory to be ready and locks its element. Noone can moves elements in the common reserve
    /// </summary>
    /// <returns></returns>
    private IEnumerator LockInventoryWhenReady()
    {
        //9 fajta Item van
        int expectedCount = 9;

        

        while (inventoryManager.GetAllItems().Count() < expectedCount)
        {
            yield return null;
        }

        foreach (var item in inventoryManager.GetAllItems())
        {
            item.SetDraggable(false);
        }

        TakeFromInventory(0, TurnManager.Instance.playerCount);
        TakeFromInventory(1, TurnManager.Instance.playerCount);

        CommonReserveReady = true;
    }

    /// <summary>
    /// True, when the common reserve is initialized and ready
    /// </summary>
    /// <returns></returns>
    public bool IsCommonReserveReady()
    {
        return CommonReserveReady;
    }

    /// <summary>
    /// The current player chooses a card from the common reserve, 
    /// then a new card is drown (if possible) for the previous cards place
    /// </summary>
    public CardType SelectCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cardSlots.Length)
        {
            return null;
        } 

        CardLoader slot = cardSlots[slotIndex];
        CardType selectedCard = slot.CurrentCard;

        if (selectedCard == null)
        {
            return null;
        }

        // Új lap húzása ugyanabból a pakliból
        string color;
        if (slotIndex < 4)
        {
            color = "White";
        }
        else
        {
            color = "Black";
        }

        CardType nextCard = CardManager.Instance.DrawCard(color);

        if (nextCard != null)
        {
            slot.ShowCard(nextCard);
        }
        else
        {
            slot.gameObject.SetActive(false); // pakli elfogyott
        }
        
        //TakePuzzle has Ended
        TurnManager.Instance.currentPlayer.ActionHasEnded();
        Debug.Log("TakePuzzle has Ended");
        OnBackClicked();

        RefreshRemainingCardCount();

        return selectedCard; // visszaadjuk a játékosnak
    }

    /// <summary>
    /// Takes an element from the common reserve's inventory (take a lvl1 element or upgrade elemt actions)
    /// </summary>
    public bool TakeFromInventory(int itemId, int amount)
    {
        InventoryItem item = inventoryManager.GetItemById(itemId);
        if (item != null && item.quantity >= amount)
        {
            item.quantity -= amount;
            return true;
        }

        //ActionSelectionPanel.Instance.ShowErrorMessage("Nincs több ilyen elem a CommonReserve-ben");
        Debug.LogWarning($"Nincs elég elem (ID: {itemId}) a közös készletben!");
        return false;
    }

    /// <summary>
    /// Online mode. Takes an element from the common reserve's inventory
    /// </summary>
    public void RequestTakeElement(int playerID)
    {
        photonView.RPC(nameof(RPC_TakeElement), RpcTarget.All, playerID);
    }

    /// <summary>
    /// Online mode. Synchronizes taking an element from the common reserve's inventory for every player
    /// </summary>
    /// <param name="playerID"></param>
    [PunRPC]
    private void RPC_TakeElement(int playerID)
    {
        // CommonReserve inventoryból levonás minden kliensen
        InventoryItem commonItem = inventoryManager.GetItemById(0);
        if (commonItem == null || commonItem.quantity < 1)
        {
            Debug.LogWarning("Nincs elég elem (ID: 0) a közös készletben!");
            return;
        }
        commonItem.quantity--;

        // A megfelelõ játékos inventoryjába hozzáadás minden kliensen
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player != null)
        {
            InventoryItem playerItem = player.inventoryManager.GetItemById(0);
            if (playerItem != null)
            {
                playerItem.quantity++;
            }                
        }

        Debug.Log("TakeElement has Ended");

        // ActionHasEnded() csak az akció tulajdonosának kliensén fut le
        if (PhotonNetwork.LocalPlayer.ActorNumber == TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID)?.PhotonActorNumber)
        {
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
    }

    /// <summary>
    /// When the current player chooses a card from the common reserve (take a puzzle card action)
    /// </summary>
    /// <param name="slotIndex"></param>
    public void OnSlotClicked(int slotIndex)
    {
        Player currentPlayer = TurnManager.Instance.currentPlayer;

        if (currentPlayer.IsCardSlotsFull()) //A játékos elõtt 4 db kártya van, nem tud újat elvenni
        {
            Debug.LogWarning($"{currentPlayer.PlayerName} keze tele van, nem lehet több lapot felvenni!");
            return;
        }


        if (PhotonNetwork.IsConnected)
        {
            //Online mód:
            // RPC küldés minden kliensnek
            photonView.RPC(nameof(RPC_SelectCard), RpcTarget.All, slotIndex, currentPlayer.PlayerID);
        }
        else
        {
            //Lokális mód:
            // Megpróbálja átadni a lapot a soron lévõ játékosnak
            CardType selected = SelectCard(slotIndex);

            if (selected != null)
            {
                currentPlayer.ReceiveCard(selected);
            }

            CommonReserveBlockingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Online mode. The choosen card apears at the current player and every other player has to see, too
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="playerID"></param>
    [PunRPC]
    private void RPC_SelectCard(int slotIndex, int playerID)
    {
        if (slotIndex < 0 || slotIndex >= cardSlots.Length)
        {
            return;
        }

        CardLoader slot = cardSlots[slotIndex];
        CardType selectedCard = slot.CurrentCard;
        if (selectedCard == null)
        {
            return;
        }

        // Slot frissítése mindenkinél
        string color = slotIndex < 4 ? "White" : "Black";
        CardType nextCard = CardManager.Instance.DrawCard(color);

        if (nextCard != null)
        {
            slot.ShowCard(nextCard);
        }            
        else
        {
            slot.gameObject.SetActive(false);
        }            

        RefreshRemainingCardCount();

        // Csak a helyi kliensen adjuk át a kártyát a játékosnak
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player != null)
        {
            player.ReceiveCard(selectedCard);
        }

        // Csak az akció tulajdonosánál fut le a kör logika
        if (PhotonNetwork.LocalPlayer.ActorNumber == TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID)?.PhotonActorNumber)
        {
            TurnManager.Instance.currentPlayer.ActionHasEnded();
            OnBackClicked();
        }
    }

    /// <summary>
    /// Changing the view back to the current player
    /// </summary>
    public void OnBackClicked()
    {
        if (originPlayer == null)
        {
            return;
        }

        CommonReservePanel.SetActive(false);
        originPlayer.PlayerPanel.SetActive(false);
        TurnManager.Instance.currentPlayer.PlayerPanel.SetActive(true);
        originPlayer = null;
    }

    /// <summary>
    /// Showing the common reserve panel
    /// </summary>
    /// <param name="fromPlayer"></param>
    public void Open(Player fromPlayer)
    {
        originPlayer = fromPlayer;
        CommonReservePanel.SetActive(true);
        fromPlayer.PlayerPanel.SetActive(false);
    }

    /// <summary>
    /// Displys the remaining card in both dacks
    /// </summary>
    public void RefreshRemainingCardCount()
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            RemainingBlackCards.text = "Fekete Kártyák: \n" + CardManager.Instance.BlackCards.Count();
            RemainingWhiteCards.text = "Fehér Kártyák: \n" + CardManager.Instance.WhiteCards.Count();
        }
        else
        {
            RemainingBlackCards.text = "Black Cards: \n" + CardManager.Instance.BlackCards.Count();
            RemainingWhiteCards.text = "White Cards: \n" + CardManager.Instance.WhiteCards.Count();
        }
    }

    /// <summary>
    /// Gives back every possible upgrade result based on the player's element (upgrade action)
    /// </summary>
    /// <param name="returnedId"></param>
    /// <param name="returnedLevel"></param>
    /// <returns></returns>
    public List<InventoryItem> GetUpgradeOptions(int returnedId, int returnedLevel)
    {
        int targetLevel = returnedLevel + 1;

        var available = inventoryManager.GetAllItems().Where(i => i.quantity > 0).ToList();

        // Célszintû elemek (bármilyen forma)
        var targetItems = available.Where(i => i.level == targetLevel).ToList();

        List<InventoryItem> mandatoryOptions;

        if (targetItems.Count > 0)
        {
            // Van célszintû ezek közül választhat
            mandatoryOptions = targetItems;
        }
        else
        {
            // Nincs célszintû következõ elérhetõ magasabb szint
            mandatoryOptions = new List<InventoryItem>();
            for (int lvl = targetLevel + 1; lvl <= 4; lvl++)
            {
                var higher = available.Where(i => i.level == lvl).ToList();
                if (higher.Count > 0)
                {
                    mandatoryOptions = higher;
                    break;
                }
            }
        }

        // Opcionális: azonos vagy alacsonyabb szintû, de MÁS forma
        var optionalOptions = available.Where(i => i.level <= returnedLevel && i.ID != returnedId).ToList();

        return mandatoryOptions.Union(optionalOptions).ToList();
    }

    /// <summary>
    /// Online mode. Finishing the upgrade action by swapping the two elements
    /// </summary>
    public void RequestUpgradeElement(int playerID, int returnId, int returnLevel, int receiveId, int receiveLevel)
    {
        photonView.RPC(nameof(RPC_UpgradeElement), RpcTarget.All, playerID, returnId, returnLevel, receiveId, receiveLevel);
    }

    /// <summary>
    /// Online mode. Finishing the upgrade action by swapping the two elements
    /// </summary>
    [PunRPC]
    private void RPC_UpgradeElement(int playerID, int returnId, int returnLevel, int receiveId, int receiveLevel)
    {
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null)
        {
            return;
        }

        // Játékos visszaadja a saját elemét a CommonReserve-be
        InventoryItem playerReturn = player.inventoryManager.GetAllItems().FirstOrDefault(i => i.ID == returnId && i.level == returnLevel);
        if (playerReturn != null) 
        { 
            playerReturn.quantity--; 
            playerReturn.RefreshCount(); 
        }

        InventoryItem commonReturn = inventoryManager.GetAllItems().FirstOrDefault(i => i.ID == returnId && i.level == returnLevel);
        if (commonReturn != null) 
        { 
            commonReturn.quantity++; 
            commonReturn.RefreshCount(); 
        }

        // Játékos megkapja a közös elem a CommonReserve-bõl
        InventoryItem commonGive = inventoryManager.GetAllItems().FirstOrDefault(i => i.ID == receiveId && i.level == receiveLevel);
        if (commonGive != null) 
        { 
            commonGive.quantity--; 
            commonGive.RefreshCount(); 
        }

        InventoryItem playerReceive = player.inventoryManager.GetAllItems().FirstOrDefault(i => i.ID == receiveId && i.level == receiveLevel);
        if (playerReceive != null) 
        { 
            playerReceive.quantity++; 
            playerReceive.RefreshCount(); 
        }

        Debug.Log($"RPC_UpgradeElement: visszaadta [ID={returnId} Lv{returnLevel}] kapta [ID={receiveId} Lv{receiveLevel}]");

        // ActionHasEnded() csak az akció tulajdonosánál
        if (PhotonNetwork.LocalPlayer.ActorNumber == player.PhotonActorNumber)
        {
            TurnManager.Instance.currentPlayer.ActionHasEnded();
        }
    }
}
