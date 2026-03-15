using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel Instance { get; private set; }

    [Header("Panel")]
    public GameObject upgradePanel;


    [Header("Játékos elemei felsõ rész")]
    public Transform playerItemsContainer;  // a 9 InventoryItem szülõje (játékos)

    [Header("CommonReserve elemei alsó rész")]
    public Transform commonItemsContainer;  // a 9 InventoryItem szülõje (CommonReserve)

    public GameObject itemButtonPrefab; // Image + Button + TMP_Text

    // Belsõ állapot
    private Player currentPlayer;
    private InventoryItem selectedPlayerItem;    // mit ad vissza a játékos
    private List<InventoryItem> validCommonOptions = new List<InventoryItem>();

    void Awake()
    {
        Instance = this;
        upgradePanel.SetActive(false);
    }

    //
    // Megnyitás
    //

    public void Open(Player player)
    {
        currentPlayer = player;
        selectedPlayerItem = null;
        validCommonOptions.Clear();

        upgradePanel.SetActive(true);

        BuildPlayerButtons();
        BuildCommonButtons(null);
    }

    private void BuildPlayerButtons()
    {
        ClearContainer(playerItemsContainer);

        foreach (InventoryItem item in currentPlayer.inventoryManager.GetAllItems())
        {
            bool selectable = item.quantity > 0;
            InventoryItem captured = item;
            CreateButton(
                playerItemsContainer,
                item,
                selectable,
                selectable ? () => OnPlayerItemClicked(captured) : (System.Action)null
            );
        }
    }

    private void BuildCommonButtons(List<InventoryItem> validOptions)
    {
        ClearContainer(commonItemsContainer);

        foreach (InventoryItem item in CommonReserve.Instance.inventoryManager.GetAllItems())
        {
            bool selectable = validOptions != null && validOptions.Contains(item) && item.quantity > 0;
            InventoryItem captured = item;
            CreateButton(
                commonItemsContainer,
                item,
                selectable,
                selectable ? () => ExecuteSwap(captured) : (System.Action)null
            );
        }
    }

    private void CreateButton(Transform container, InventoryItem item, bool selectable, System.Action onClick)
    {
        GameObject btn = Instantiate(itemButtonPrefab, container);

        // Sprite betöltése
        Sprite sprite = Resources.Load<Sprite>($"Elements/Element{item.ID + 1}");
        Image img = btn.GetComponent<Image>();
        if (img != null && sprite != null)
            img.sprite = sprite;

        // Darabszám szöveg
        TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = $"Lv{item.level}\nx{item.quantity}";

        // Gomb
        Button button = btn.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = selectable;
            if (onClick != null)
                button.onClick.AddListener(() => onClick());
        }

        // Highlight törlése
        SetHighlight(btn.transform, false);
    }

    private void OnPlayerItemClicked(InventoryItem item)
    {
        if (item.quantity <= 0) return;

        selectedPlayerItem = item;

        // Highlight frissítése
        RefreshHighlight(playerItemsContainer, item);

        // Common oldal újraépítése
        validCommonOptions = CommonReserve.Instance.GetUpgradeOptions(item.ID, item.level);
        BuildCommonButtons(validCommonOptions);
    }

    private void ExecuteSwap(InventoryItem commonItem)
    {
        if (selectedPlayerItem == null || commonItem == null) return;

        // Játékos visszaadja CommonReserve +1
        selectedPlayerItem.quantity--;
        selectedPlayerItem.RefreshCount();

        InventoryItem commonReturnTarget = CommonReserve.Instance.inventoryManager
            .GetAllItems()
            .FirstOrDefault(i => i.ID == selectedPlayerItem.ID && i.level == selectedPlayerItem.level);

        if (commonReturnTarget != null)
        {
            commonReturnTarget.quantity++;
            commonReturnTarget.RefreshCount();
        }

        // Játékos megkapja játékos inventory +1
        commonItem.quantity--;
        commonItem.RefreshCount();

        InventoryItem playerReceiveTarget = currentPlayer.inventoryManager
            .GetAllItems()
            .FirstOrDefault(i => i.ID == commonItem.ID && i.level == commonItem.level);

        if (playerReceiveTarget != null)
        {
            playerReceiveTarget.quantity++;
            playerReceiveTarget.RefreshCount();
        }

        Debug.Log($"Upgrade: visszaadta [ID={selectedPlayerItem.ID} Lv{selectedPlayerItem.level}] " +
                  $"kapta [ID={commonItem.ID} Lv{commonItem.level}]");

        Close();
        currentPlayer.ActionHasEnded();
    }

    //
    //Segédek
    //
    private void RefreshHighlight(Transform container, InventoryItem selectedItem)
    {
        int index = 0;
        var items = currentPlayer.inventoryManager.GetAllItems().ToList();

        foreach (Transform child in container)
        {
            bool isSelected = index < items.Count && items[index] == selectedItem;
            SetHighlight(child, isSelected);
            index++;
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void Close()
    {
        ClearContainer(playerItemsContainer);
        ClearContainer(commonItemsContainer);

        selectedPlayerItem = null;
        validCommonOptions.Clear();
        upgradePanel.SetActive(false);
        currentPlayer.PlayerPanel.SetActive(true);
    }

    private void SetHighlight(Transform t, bool active)
    {
        Outline outline = t.GetComponent<Outline>();
        if (outline != null) { outline.enabled = active; return; }

        Image bg = t.GetComponent<Image>();
        if (bg != null)
            bg.color = active ? new Color(1f, 0.85f, 0f, 1f) : Color.white;
    }
}