using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonReserve : MonoBehaviour
{
    public static CommonReserve Instance { get; private set; }

    // 8 kártyahely — Inspectorban drag & drop
    [SerializeField]
    private CardLoader[] cardSlots = new CardLoader[8];

    // Közös inventory (ugyanúgy mûködik mint a játékosnál)
    public InventoryManager inventoryManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        // Minden slotba húz egy lapot a CardManager-bõl
        // Az elsõ 4 slot fehér, a következõ 4 fekete lapot kap
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;

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
                cardSlots[i].ShowCard(card);
            else
                cardSlots[i].gameObject.SetActive(false); // pakli üres
        }
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
            slot.ShowCard(nextCard);
        else
            slot.gameObject.SetActive(false); // pakli elfogyott

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

        // Megpróbálja átadni a lapot a soron lévõ játékosnak
        CardType selected = SelectCard(slotIndex);

        if (selected != null)
        {
            currentPlayer.ReceiveCard(selected);
        }
    }
}
