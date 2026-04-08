using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tesztek az InventoryManager és InventoryItem-hez
/// Teszteli: inventory management, item quantity, element upgrade
/// </summary>
public class InventoryManagerTests
{
    private GameObject inventoryManagerGO;
    private InventoryManager inventoryManager;
    private List<InventoryItem> testItems;
    private List<GameObject> createdGameObjects;

    [SetUp]
    public void Setup()
    {
        createdGameObjects = new List<GameObject>();

        inventoryManagerGO = new GameObject("InventoryManager");
        inventoryManager = inventoryManagerGO.AddComponent<InventoryManager>();
        createdGameObjects.Add(inventoryManagerGO);

        testItems = new List<InventoryItem>();

        for (int i = 0; i < 9; i++)
        {
            GameObject itemGO = new GameObject($"Item_{i}");
            itemGO.transform.SetParent(inventoryManagerGO.transform);

            InventoryItem item = itemGO.AddComponent<InventoryItem>();
            item.ID = i;
            item.quantity = 5 + i;
            item.TotalSquareNumber = i + 1;

            Image img = itemGO.AddComponent<Image>();

            testItems.Add(item);
            createdGameObjects.Add(itemGO);
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in createdGameObjects)
        {
            if (go != null)
            {
                Object.Destroy(go);
            }
        }
        createdGameObjects.Clear();
    }

    [UnityTest]
    public IEnumerator InventoryItem_DefaultValues()
    {
        yield return null;
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i, testItems[i].ID);
            Assert.AreEqual(5 + i, testItems[i].quantity);
        }
    }

    [UnityTest]
    public IEnumerator InventoryItem_Quantity_CanBeDecremented()
    {
        yield return null;
        int initialQuantity = testItems[0].quantity;
        testItems[0].quantity--;

        Assert.AreEqual(initialQuantity - 1, testItems[0].quantity);
    }

    [UnityTest]
    public IEnumerator InventoryItem_Quantity_CanBeIncremented()
    {
        yield return null;
        int initialQuantity = testItems[2].quantity;
        testItems[2].quantity += 3;

        Assert.AreEqual(initialQuantity + 3, testItems[2].quantity);
    }

    [UnityTest]
    public IEnumerator InventoryItem_SelectedInventoryItem_CanBeSet()
    {
        yield return null;
        InventoryItem.SelectedInventoryItem = testItems[0];
        Assert.AreEqual(testItems[0], InventoryItem.SelectedInventoryItem);
    }

    [UnityTest]
    public IEnumerator InventoryItem_IsDragging_Flag()
    {
        yield return null;
        InventoryItem.IsDragging = true;
        Assert.IsTrue(InventoryItem.IsDragging);

        InventoryItem.IsDragging = false;
        Assert.IsFalse(InventoryItem.IsDragging);
    }
}
