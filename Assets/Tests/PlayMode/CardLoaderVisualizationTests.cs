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
    private GameObject cardManagerGO;

    [SetUp]
    public void Setup()
    {
        // Először a CardManager-t kell létrehozni!
        cardManagerGO = new GameObject("CardManager");
        CardManager cardManager = cardManagerGO.AddComponent<CardManager>();

        // CardLoader GameObject létrehozása
        cardLoaderGO = new GameObject("CardLoader");

        // Grid GameObject létrehozása
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

        // UI elemek létrehozása (mielőtt a CardLoader komponens létrejönne)
        GameObject bgImageGO = new GameObject("BgImage");
        bgImageGO.transform.SetParent(cardLoaderGO.transform);
        Image bgImage = bgImageGO.AddComponent<Image>();

        GameObject scoreTextGO = new GameObject("ScoreText");
        scoreTextGO.transform.SetParent(cardLoaderGO.transform);
        TextMeshProUGUI scoreText = scoreTextGO.AddComponent<TextMeshProUGUI>();

        GameObject rewardImageGO = new GameObject("RewardImage");
        rewardImageGO.transform.SetParent(cardLoaderGO.transform);
        Image rewardImage = rewardImageGO.AddComponent<Image>();

        GameObject gridBgImageGO = new GameObject("GridBgImage");
        gridBgImageGO.transform.SetParent(cardLoaderGO.transform);
        Image gridBgImage = gridBgImageGO.AddComponent<Image>();

        // Most adjuk hozzá a CardLoader komponenst
        cardLoader = cardLoaderGO.AddComponent<CardLoader>();
        cardLoader.Grid = gridGO;
        cardLoader.gridScript = myGrid;

        // UI elemek beállítása
        cardLoader.BgImage = bgImage;
        cardLoader.ScoreText = scoreText;
        cardLoader.RewardImage = rewardImage;
        cardLoader.GridBgImage = gridBgImage;

        // RewardSprites mock
        cardLoader.RewardSprites = new Sprite[9];
        for (int i = 0; i < 9; i++)
        {
            Texture2D tex = new Texture2D(10, 10);
            cardLoader.RewardSprites[i] = Sprite.Create(tex, new Rect(0, 0, 10, 10), Vector2.zero);
        }

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
        if (cardManagerGO != null)
        {
            Object.Destroy(cardManagerGO);
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
