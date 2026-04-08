using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PlayMode tesztek a CardLoader vizualizációjához
/// Teszteli: kártya megjelenítése, UI frissítés, grid inicializálása
/// </summary>
public class CardLoaderVisualizationTests
{
    private GameObject cardLoaderGO;
    private CardLoader cardLoader;
    private GameObject gridGO;
    private MyGrid myGrid;
    private CardType testCard;

    [SetUp]
    public void Setup()
    {
        // CardLoader GameObject létrehozása
        cardLoaderGO = new GameObject("CardLoader");
        cardLoader = cardLoaderGO.AddComponent<CardLoader>();

        // UI elemek mock-olása
        GameObject bgImageGO = new GameObject("BgImage");
        bgImageGO.transform.SetParent(cardLoaderGO.transform);
        Image bgImage = bgImageGO.AddComponent<Image>();
        cardLoader.BgImage = bgImage;

        GameObject scoreTextGO = new GameObject("ScoreText");
        scoreTextGO.transform.SetParent(cardLoaderGO.transform);
        TextMeshProUGUI scoreText = scoreTextGO.AddComponent<TextMeshProUGUI>();
        cardLoader.ScoreText = scoreText;

        GameObject rewardImageGO = new GameObject("RewardImage");
        rewardImageGO.transform.SetParent(cardLoaderGO.transform);
        Image rewardImage = rewardImageGO.AddComponent<Image>();
        cardLoader.RewardImage = rewardImage;

        GameObject gridBgImageGO = new GameObject("GridBgImage");
        gridBgImageGO.transform.SetParent(cardLoaderGO.transform);
        Image gridBgImage = gridBgImageGO.AddComponent<Image>();
        cardLoader.GridBgImage = gridBgImage;

        // Grid GameObject
        gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(cardLoaderGO.transform);
        myGrid = gridGO.AddComponent<MyGrid>();
        
        // Mock GridSquare prefab
        GameObject gridSquarePrefab = new GameObject("GridSquare");
        RectTransform rectTransform = gridSquarePrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        GridSquare gridSquareComponent = gridSquarePrefab.AddComponent<GridSquare>();
        Image img = gridSquarePrefab.AddComponent<Image>();
        gridSquareComponent.activeImage = img;
        
        myGrid.columns = 7;
        myGrid.rows = 7;
        myGrid.gridSquare = gridSquarePrefab;
        myGrid.squareScale = 1f;
        myGrid.squareGap = 2f;
        myGrid.everySquareOffSet = 1f;
        myGrid.startPosition = Vector2.zero;

        cardLoader.Grid = gridGO;
        cardLoader.gridScript = myGrid;

        // Test kártya létrehozása
        int[,] testMatrix = new int[7, 7];
        testMatrix[0, 0] = 10;
        testMatrix[1, 1] = 10;
        testCard = new CardType("White", 5, 1, testMatrix, 0);
    }

    [TearDown]
    public void TearDown()
    {
        if (cardLoaderGO != null)
        {
            Object.Destroy(cardLoaderGO);
        }
    }

    [UnityTest]
    public IEnumerator CardLoader_ShowCard_SetCurrentCard()
    {
        myGrid.isInitialized = true;
        cardLoader.ShowCard(testCard);
        yield return null;

        Assert.IsNotNull(cardLoader.CurrentCard);
        Assert.AreEqual(testCard.Color, cardLoader.CurrentCard.Color);
    }

    [UnityTest]
    public IEnumerator CardLoader_WhiteCard_HasCorrectColors()
    {
        myGrid.isInitialized = true;
        cardLoader.ShowCard(testCard);
        yield return null;

        Assert.AreEqual(Color.white, cardLoader.BgImage.color);
        Assert.AreEqual(Color.black, cardLoader.ScoreText.color);
    }
}
