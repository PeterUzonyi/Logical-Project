using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<int, InventoryItem> InventoryItems = new Dictionary<int, InventoryItem>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterItem(InventoryItem item)
    {
        InventoryItems[item.ID] = item;
    }

    public InventoryItem GetItemById(int id) 
    { 
        InventoryItems.TryGetValue(id, out InventoryItem item);
        return item;
    }
}
