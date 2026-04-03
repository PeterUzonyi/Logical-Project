using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using Photon.Pun;

/// <summary>
/// Visualizes and executes the upgrade action
/// </summary>
public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel Instance { get; private set; }

    /// <summary>
    /// The panel it self
    /// </summary>
    [Header("Panel")]
    public GameObject upgradePanel;

    /// <summary>
    /// The player's invetory
    /// </summary>
    [Header("Játékos elemei felsõ rész")]
    public Transform playerItemsContainer;  // a 9 InventoryItem szülõje (játékos)

    /// <summary>
    /// The Common Reserve's invenoty
    /// </summary>
    [Header("CommonReserve elemei alsó rész")]
    public Transform commonItemsContainer;  // a 9 InventoryItem szülõje (CommonReserve)

    /// <summary>
    /// A button that contains an image of an element and a text (element's level and quantity)
    /// </summary>
    public GameObject itemButtonPrefab; // Image + Button + TMP_Text

    /// <summary>
    /// The player, whose turn is this
    /// </summary>
    private Player currentPlayer;

    /// <summary>
    /// The selected element from the player's inventory (this will be upgraded)
    /// </summary>
    private InventoryItem selectedPlayerItem;    // mit ad vissza a játékos

    /// <summary>
    /// Possible upgrade elements options from the Common Reserve's inventory
    /// </summary>
    private List<InventoryItem> validCommonOptions = new List<InventoryItem>();

    //Called when the script is loaded
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// The player opens the upgrade panel
    /// </summary>
    /// <param name="player"></param>
    public void Open(Player player)
    {
        currentPlayer = player;
        selectedPlayerItem = null;
        validCommonOptions.Clear();

        upgradePanel.SetActive(true);

        BuildPlayerButtons();
        BuildCommonButtons(null);
    }

    /// <summary>
    /// Building the player's inventory with buttons
    /// </summary>
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

    /// <summary>
    /// Building the Common Reserve's inventory with buttons
    /// </summary>
    /// <param name="validOptions"></param>
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

    /// <summary>
    /// Creating the buttons for the invetorys
    /// </summary>
    /// <param name="container"></param>
    /// <param name="item"></param>
    /// <param name="selectable"></param>
    /// <param name="onClick"></param>
    private void CreateButton(Transform container, InventoryItem item, bool selectable, System.Action onClick)
    {
        GameObject btn = Instantiate(itemButtonPrefab, container);

        // Sprite betöltése
        Sprite sprite = Resources.Load<Sprite>($"Elements/Element{item.ID + 1}");
        Image img = btn.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
        }

        // Darabszám szöveg
        TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = $"Lv{item.level}\nx{item.quantity}";
        }

        // Gomb
        Button button = btn.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = selectable;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        // Highlight törlése
        SetHighlight(btn.transform, false);
    }

    /// <summary>
    /// Tiggered when the player chooses the element wants to upgrade
    /// </summary>
    /// <param name="item"></param>
    private void OnPlayerItemClicked(InventoryItem item)
    {
        if (item.quantity <= 0)
        {
            return;
        }

        selectedPlayerItem = item;

        // Highlight frissítése
        RefreshHighlight(playerItemsContainer, item);

        // Common oldal újraépítése
        validCommonOptions = CommonReserve.Instance.GetUpgradeOptions(item.ID, item.level);
        BuildCommonButtons(validCommonOptions);
    }

    /// <summary>
    /// Swapping the two element: the player loses the chosen element (the common reserve gets it) and 
    /// gets the chosen element from the Common Reserve's inventory (the common reserve loses it)
    /// </summary>
    /// <param name="commonItem"></param>
    private void ExecuteSwap(InventoryItem commonItem)
    {
        if (selectedPlayerItem == null || commonItem == null)
        {
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            //Online rész
            CommonReserve.Instance.RequestUpgradeElement(
            currentPlayer.PlayerID,
            selectedPlayerItem.ID, selectedPlayerItem.level,
            commonItem.ID, commonItem.level);
            Close();
        }
        else
        {
            //Lokális rész
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
    }

    /// <summary>
    /// Highlights the possibilities for the swap based on the player's selected element
    /// </summary>
    /// <param name="container"></param>
    /// <param name="selectedItem"></param>
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

    /// <summary>
    /// Clearing the container for the next use
    /// </summary>
    /// <param name="container"></param>
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Closes the upgrade panel
    /// </summary>
    private void Close()
    {
        ClearContainer(playerItemsContainer);
        ClearContainer(commonItemsContainer);

        selectedPlayerItem = null;
        validCommonOptions.Clear();
        upgradePanel.SetActive(false);
        currentPlayer.PlayerPanel.SetActive(true);
    }

    /// <summary>
    /// Highlights a given element
    /// </summary>
    /// <param name="t"></param>
    /// <param name="active"></param>
    private void SetHighlight(Transform t, bool active)
    {
        Outline outline = t.GetComponent<Outline>();
        if (outline != null) 
        { 
            outline.enabled = active; 
            return; 
        }

        Image bg = t.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = active ? new Color(1f, 0.85f, 0f, 1f) : Color.white;
        }
    }
}