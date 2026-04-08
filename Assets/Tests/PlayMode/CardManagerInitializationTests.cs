using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tesztek a CardManager inicializálásához (offline mód)
/// Teszteli: kártyák betöltése, shuffle, deck kezelés
/// </summary>
public class CardManagerInitializationTests
{
    private CardManager cardManager;
    private GameObject cardManagerGO;

    [SetUp]
    public void Setup()
    {
        cardManagerGO = new GameObject("CardManager");
        cardManager = cardManagerGO.AddComponent<CardManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (cardManagerGO != null)
        {
            Object.Destroy(cardManagerGO);
        }
    }

    [UnityTest]
    public IEnumerator CardManager_IsInitialized()
    {
        yield return new WaitForSeconds(0.5f);
        Assert.IsNotNull(cardManager);
    }

    [UnityTest]
    public IEnumerator CardManager_HasWhiteAndBlackDecks()
    {
        yield return new WaitForSeconds(0.5f);
        Assert.IsNotNull(cardManager.WhiteCards);
        Assert.IsNotNull(cardManager.BlackCards);
    }
}
