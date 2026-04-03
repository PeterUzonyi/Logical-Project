using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls inventory for evey player and the Common Reserve
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    /// <summary>
    /// Inventory of a player or the common reserve (index, element)
    /// </summary>
    private Dictionary<int, InventoryItem> InventoryItems = new Dictionary<int, InventoryItem>();

    //Called when the script is loaded
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Storing a new element in the dictionary
    /// </summary>
    /// <param name="item"></param>
    public void RegisterItem(InventoryItem item)
    {
        InventoryItems[item.ID] = item;
    }

    /// <summary>
    /// Gives back an element from the dictionary based on the given index
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public InventoryItem GetItemById(int id) 
    { 
        InventoryItems.TryGetValue(id, out InventoryItem item);
        return item;
    }

    /// <summary>
    /// Gives back the whole inventory of a player or the common reserve
    /// </summary>
    /// <returns></returns>
    public IEnumerable<InventoryItem> GetAllItems()
    {
        return InventoryItems.Values;
    }
}
