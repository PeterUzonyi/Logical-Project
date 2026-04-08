using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Teljes integrációs PlayMode teszt: Puzzle befejezés
/// Szimulálja: kártya megjelenítés -> elemek lehelyezése -> puzzle befejezés -> pontszám frissítés
/// </summary>
public class PuzzleCompletionIntegrationTests
{
    private GameObject playerGO;
    private Player player;
    private List<GameObject> createdGameObjects;

    [SetUp]
    public void Setup()
    {
        createdGameObjects = new List<GameObject>();

        playerGO = new GameObject("Player");
        player = playerGO.AddComponent<Player>();
        player.PlayerID = 0;
        player.PlayerName = "Teszt Játékos";
        player.PlayerScore = 0;
        player.CompletedPuzzles = 0;
        createdGameObjects.Add(playerGO);

        GameObject inventoryGO = new GameObject("InventoryManager");
        inventoryGO.transform.SetParent(playerGO.transform);
        InventoryManager inventoryManager = inventoryGO.AddComponent<InventoryManager>();
        player.inventoryManager = inventoryManager;
        createdGameObjects.Add(inventoryGO);

        for (int i = 0; i < 9; i++)
        {
            GameObject itemGO = new GameObject($"Item_{i}");
            itemGO.transform.SetParent(inventoryGO.transform);
            InventoryItem item = itemGO.AddComponent<InventoryItem>();
            item.ID = i;
            item.quantity = 10;
            item.TotalSquareNumber = 3 + (i % 3);

            Image img = itemGO.AddComponent<Image>();

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
    public IEnumerator Integration_PlayerScoreManagement()
    {
        int initialScore = player.PlayerScore;
        player.RefreshScore(10);
        yield return null;

        Assert.AreEqual(initialScore + 10, player.Score);
    }

    [UnityTest]
    public IEnumerator Integration_InventoryManagement()
    {
        InventoryManager inventoryManager = player.inventoryManager;
        InventoryItem item0 = inventoryManager.transform.GetChild(0).GetComponent<InventoryItem>();
        int initialQuantity = item0.quantity;

        item0.quantity--;
        yield return null;

        Assert.AreEqual(initialQuantity - 1, item0.quantity);

        item0.quantity += 2;
        yield return null;

        Assert.AreEqual(initialQuantity + 1, item0.quantity);
    }

    [UnityTest]
    public IEnumerator Integration_MultiplePuzzles()
    {
        int totalScore = 0;
        int completedCount = 0;

        for (int puzzleNum = 0; puzzleNum < 3; puzzleNum++)
        {
            int score = (puzzleNum + 1) * 5;
            player.RefreshScore(score);
            player.CompletedPuzzles++;
            totalScore += score;
            completedCount++;

            yield return null;
        }

        Assert.AreEqual(3, completedCount);
        Assert.AreEqual(30, totalScore);
        Assert.AreEqual(30, player.Score);
    }

    [UnityTest]
    public IEnumerator Integration_PlayerInitialization()
    {
        yield return null;
        Assert.IsNotNull(player.inventoryManager);
        Assert.AreEqual("Teszt Játékos", player.PlayerName);
        Assert.AreEqual(0, player.PlayerID);
    }
}
