using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Photon.Pun;

public class CommonReserve : MonoBehaviourPun
{
    public static CommonReserve Instance { get; private set; }

    // 8 kártyahely — Inspectorban drag & drop
    [SerializeField]
    private CardLoader[] cardSlots = new CardLoader[8];

    // Közös inventory (ugyanúgy mûködik mint a játékosnál)
    public InventoryManager inventoryManager;

    public GameObject CommonReservePanel;
    public GameObject CommonReserveBlockingPanel;
    private Player originPlayer;
    private bool CommonReserveReady = false;

    public GameObject RemainingBlackCards;
    public GameObject RemainingWhiteCards;

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
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeSlotsWhenReady());
        StartCoroutine(LockInventoryWhenReady());
    }

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

    private IEnumerator LockInventoryWhenReady()
    {
        //9 fajta Item van
        int expectedCount = 90;

        while (inventoryManager.GetAllItems().Count() < expectedCount)
        {
            yield return null;
        }

        foreach (var item in inventoryManager.GetAllItems())
        {
            item.SetDraggable(false);
        }

        CommonReserveReady = true;
    }

    public bool IsCommonReserveReady()
    {
        return CommonReserveReady;
    }

    /// <summary>
    /// Egy játékos kiválaszt egy kártyát a közös készletbõl.
    /// A slot helyére automatikusan új lap kerül a pakliból.
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
    /// Egy játékos elvesz inventory itemet a közös készletbõl.
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

    [PunRPC]
    private void RPC_SelectCard(int slotIndex, int playerID)
    {
        if (slotIndex < 0 || slotIndex >= cardSlots.Length) return;

        CardLoader slot = cardSlots[slotIndex];
        CardType selectedCard = slot.CurrentCard;
        if (selectedCard == null) return;

        // Slot frissítése mindenkinél
        string color = slotIndex < 4 ? "White" : "Black";
        CardType nextCard = CardManager.Instance.DrawCard(color);

        if (nextCard != null)
            slot.ShowCard(nextCard);
        else
            slot.gameObject.SetActive(false);

        RefreshRemainingCardCount();

        // Csak a helyi kliensen adjuk át a kártyát a játékosnak
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player != null)
        {
            player.ReceiveCard(selectedCard);
        }

        // Csak az akció tulajdonosánál fut le a kör logika
        if (PhotonNetwork.LocalPlayer.ActorNumber == TurnManager.Instance.players
            .FirstOrDefault(p => p.PlayerID == playerID)?.PhotonActorNumber)
        {
            TurnManager.Instance.currentPlayer.ActionHasEnded();
            OnBackClicked();
        }
    }

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

    public void Open(Player fromPlayer)
    {
        originPlayer = fromPlayer;
        CommonReservePanel.SetActive(true);
        //CommonReserveBlockingPanel.SetActive(true);
        fromPlayer.PlayerPanel.SetActive(false);
    }

    public void RefreshRemainingCardCount()
    {
        TMP_Text BText = RemainingBlackCards.GetComponent<TMP_Text>();
        BText.text = CardManager.Instance.BlackCards.Count().ToString();
        TMP_Text WText = RemainingWhiteCards.GetComponent<TMP_Text>();
        WText.text = CardManager.Instance.WhiteCards.Count().ToString();
    }


    public List<InventoryItem> GetUpgradeOptions(int returnedId, int returnedLevel)
    {
        int targetLevel = returnedLevel + 1;

        var available = inventoryManager.GetAllItems()
            .Where(i => i.quantity > 0)
            .ToList();

        // Célszintû elemek (bármilyen forma)
        var targetItems = available
            .Where(i => i.level == targetLevel)
            .ToList();

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
        var optionalOptions = available
            .Where(i => i.level <= returnedLevel && i.ID != returnedId)
            .ToList();

        return mandatoryOptions.Union(optionalOptions).ToList();
    }
}
