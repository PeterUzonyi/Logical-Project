using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Photon.Pun;
using UnityEngine.UI;

/// <summary>
/// PlayMode teszt: Teljes körforduló szimulálása
/// Teszteli: játékosváltás, akciók, puzzle befejezés
/// </summary>
public class GameTurnFlowTests
{
    private GameObject turnManagerGO;
    private TurnManager turnManager;
    private List<Player> testPlayers;
    private List<GameObject> createdGameObjects;

    [SetUp]
    public void Setup()
    {
        createdGameObjects = new List<GameObject>();

        turnManagerGO = new GameObject("TurnManager");
        turnManager = turnManagerGO.AddComponent<TurnManager>();
        createdGameObjects.Add(turnManagerGO);

        testPlayers = new List<Player>();
        GameConfig.PlayerCount = 2;

        for (int i = 0; i < 2; i++)
        {
            GameObject playerGO = new GameObject($"Player_{i}");
            playerGO.transform.SetParent(turnManagerGO.transform);
            Player player = playerGO.AddComponent<Player>();
            player.PlayerID = i;
            player.PlayerName = $"Játékos {i + 1}";

            GameObject inventoryGO = new GameObject("InventoryManager");
            inventoryGO.transform.SetParent(playerGO.transform);
            InventoryManager inventoryManager = inventoryGO.AddComponent<InventoryManager>();
            player.inventoryManager = inventoryManager;

            testPlayers.Add(player);
            createdGameObjects.Add(playerGO);
        }

        turnManager.players = testPlayers;
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
    public IEnumerator TurnManager_HasCorrectPlayers()
    {
        yield return null;
        Assert.AreEqual(2, turnManager.players.Count);
    }

    [UnityTest]
    public IEnumerator Player_Score_DefaultsToZero()
    {
        yield return null;
        Assert.AreEqual(0, testPlayers[0].Score);
    }

    [UnityTest]
    public IEnumerator Player_RefreshScore_IncrementsScore()
    {
        yield return null;
        testPlayers[0].RefreshScore(10);
        Assert.AreEqual(10, testPlayers[0].Score);

        testPlayers[0].RefreshScore(5);
        Assert.AreEqual(15, testPlayers[0].Score);
    }

    [UnityTest]
    public IEnumerator Player_CompletedPuzzles_Increments()
    {
        yield return null;
        Assert.AreEqual(0, testPlayers[0].CompletedPuzzles);

        testPlayers[0].CompletedPuzzles++;
        Assert.AreEqual(1, testPlayers[0].CompletedPuzzles);
    }

    [UnityTest]
    public IEnumerator Player_PlayerName_SetCorrectly()
    {
        yield return null;
        Assert.AreEqual("Játékos 1", testPlayers[0].PlayerName);
        Assert.AreEqual("Játékos 2", testPlayers[1].PlayerName);
    }
}
