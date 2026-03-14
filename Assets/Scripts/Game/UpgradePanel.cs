using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel Instance { get; private set; }

    [Header("Panel")]
    public GameObject upgradePanel;

    [Header("Info szöveg")]
    public TMP_Text infoText;

    [Header("Játékos elemei felsõ rész")]
    public Transform playerItemsContainer;  // a 9 InventoryItem szülõje (játékos)

    [Header("CommonReserve elemei alsó rész")]
    public Transform commonItemsContainer;  // a 9 InventoryItem szülõje (CommonReserve)

    [Header("Gombok")]
    public Button confirmButton;   // "Csere" gomb csak ha mindkét elem ki van választva
    public Button cancelButton;    // "Mégse"

    // Belsõ állapot
    private Player currentPlayer;
    private InventoryItem selectedPlayerItem;    // mit ad vissza a játékos
    private InventoryItem selectedCommonItem;    // mit vesz el a CommonReserve-bõl
    private List<InventoryItem> validCommonOptions = new List<InventoryItem>();

    void Awake()
    {
        Instance = this;
        upgradePanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    //
    // Megnyitás
    //

    public void Open(Player player)
    {
        currentPlayer = player;
        selectedPlayerItem = null;
        selectedCommonItem = null;
        validCommonOptions.Clear();

        upgradePanel.SetActive(true);
        confirmButton.interactable = false;

        RefreshPlayerItems();
        ClearCommonSelection();

        infoText.text = "Válaszd ki, melyik saját alkatrészedet adod vissza!";
    }

    //
    // 1. lépés: játékos saját elemei (felsõ rész)
    //

    private void RefreshPlayerItems()
    {
        // Végigmegyünk a containerben lévõ InventoryItem-eken
        foreach (Transform child in playerItemsContainer)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item == null) continue;

            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;

            // Kattintható: van belõle, és szintje < 4
            bool selectable = item.quantity > 0 && item.level < 4;
            btn.interactable = selectable;

            // Vizuális highlight törlése
            SetHighlight(child, false);

            // onClick újrakötése (lambda capture miatt így biztonságos)
            btn.onClick.RemoveAllListeners();
            if (selectable)
            {
                InventoryItem captured = item;
                btn.onClick.AddListener(() => OnPlayerItemClicked(captured));
            }
        }
    }

    public void OnPlayerItemClicked(InventoryItem item)
    {
        if (item.quantity <= 0 || item.level >= 4) return;

        // Elõzõ highlight törlése
        if (selectedPlayerItem != null)
            SetHighlight(selectedPlayerItem.transform, false);

        selectedPlayerItem = item;
        SetHighlight(item.transform, true);

        selectedCommonItem = null;
        confirmButton.interactable = false;

        RefreshCommonItems();

        infoText.text = $"Kiválasztottad: forma {item.ID}, {item.level}. szint. " +
                        $"Most válassz egy elemet a közös tartalékból!";
    }

    //
    // 2. lépés: CommonReserve elemei (alsó rész)
    // 

    private void RefreshCommonItems()
    {
        validCommonOptions = CommonReserve.Instance.GetUpgradeOptions(
            selectedPlayerItem.ID,
            selectedPlayerItem.level
        );

        foreach (Transform child in commonItemsContainer)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item == null) continue;

            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;

            bool selectable = validCommonOptions.Contains(item) && item.quantity > 0;
            btn.interactable = selectable;

            SetHighlight(child, false);

            btn.onClick.RemoveAllListeners();
            if (selectable)
            {
                InventoryItem captured = item;
                btn.onClick.AddListener(() => OnCommonItemClicked(captured));
            }
        }
    }

    private void ClearCommonSelection()
    {
        foreach (Transform child in commonItemsContainer)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
            }
            SetHighlight(child, false);
        }
    }

    public void OnCommonItemClicked(InventoryItem item)
    {
        if (!validCommonOptions.Contains(item) || item.quantity <= 0) return;

        // Elõzõ highlight törlése
        if (selectedCommonItem != null)
            SetHighlight(selectedCommonItem.transform, false);

        selectedCommonItem = item;
        SetHighlight(item.transform, true);
        confirmButton.interactable = true;

        int levelDiff = item.level - selectedPlayerItem.level;
        string levelInfo = levelDiff > 0
            ? $"+{levelDiff} szint"
            : levelDiff == 0 ? "azonos szint" : $"{levelDiff} szint";

        infoText.text = $"Csere: forma {selectedPlayerItem.ID} Lv{selectedPlayerItem.level} " +
                        $"forma {item.ID} Lv{item.level} ({levelInfo}). Erõsítsd meg!";
    }

    //
    // Megerõsítés és végrehajtás
    //

    private void OnConfirmClicked()
    {
        if (selectedPlayerItem == null || selectedCommonItem == null) return;

        // Játékos visszaadja a saját elemét CommonReserve +1
        selectedPlayerItem.quantity--;
        selectedPlayerItem.RefreshCount();

        InventoryItem commonReturnTarget = GetCommonItemByIdAndLevel(
            selectedPlayerItem.ID,
            selectedPlayerItem.level
        );
        if (commonReturnTarget != null)
        {
            commonReturnTarget.quantity++;
            commonReturnTarget.RefreshCount();
        }

        // Játékos megkapja a kiválasztott CommonReserve elemet játékos inventory +1
        selectedCommonItem.quantity--;
        selectedCommonItem.RefreshCount();

        InventoryItem playerReceiveTarget = GetPlayerItemByIdAndLevel(
            selectedCommonItem.ID,
            selectedCommonItem.level
        );
        if (playerReceiveTarget != null)
        {
            playerReceiveTarget.quantity++;
            playerReceiveTarget.RefreshCount();
        }

        Debug.Log($"Upgrade végrehajtva: visszaadta [ID={selectedPlayerItem.ID} Lv{selectedPlayerItem.level}]" +
                  $" kapta [ID={selectedCommonItem.ID} Lv{selectedCommonItem.level}]");

        Close();
        currentPlayer.ActionHasEnded();
    }

    //
    // Segédek az ID+level alapú kereséshez
    //

    private InventoryItem GetCommonItemByIdAndLevel(int id, int level)
    {
        return CommonReserve.Instance.inventoryManager
            .GetAllItems()
            .FirstOrDefault(i => i.ID == id && i.level == level);
    }

    private InventoryItem GetPlayerItemByIdAndLevel(int id, int level)
    {
        return currentPlayer.inventoryManager
            .GetAllItems()
            .FirstOrDefault(i => i.ID == id && i.level == level);
    }

    // 
    // Mégse és bezárás
    //

    private void OnCancelClicked()
    {
        Close();
        FindAnyObjectByType<ActionSelectionPanel>().ShowPanel();
        // Szándékosan NEM hívjuk ActionHasEnded()-et
    }

    private void Close()
    {
        selectedPlayerItem = null;
        selectedCommonItem = null;
        validCommonOptions.Clear();
        upgradePanel.SetActive(false);
        currentPlayer.PlayerPanel.SetActive(true);
    }

    // 
    // Vizuális highlight
    //

    private void SetHighlight(Transform itemTransform, bool active)
    {
        // Outline komponenst használunk, ha van  különben háttérszín váltás
        Outline outline = itemTransform.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = active;
            return;
        }

        // Fallback: Image color
        Image bg = itemTransform.GetComponent<Image>();
        if (bg != null)
            bg.color = active ? new Color(1f, 0.85f, 0f, 1f) : Color.white;
    }
}