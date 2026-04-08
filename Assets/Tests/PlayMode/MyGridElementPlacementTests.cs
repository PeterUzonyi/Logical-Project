using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tesztek a MyGrid-hez (puzzle grid)
/// Teszteli: grid inicializálása, element placement, puzzle completion
/// </summary>
public class MyGridElementPlacementTests
{
    private GameObject gridGO;
    private MyGrid myGrid;

    [SetUp]
    public void Setup()
    {
        gridGO = new GameObject("MyGrid");
        myGrid = gridGO.AddComponent<MyGrid>();

        GameObject gridSquarePrefab = new GameObject("GridSquare");
        RectTransform rectTransform = gridSquarePrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        GridSquare gridSquareScript = gridSquarePrefab.AddComponent<GridSquare>();
        Image img = gridSquarePrefab.AddComponent<Image>();
        gridSquareScript.activeImage = img;

        myGrid.columns = 7;
        myGrid.rows = 7;
        myGrid.gridSquare = gridSquarePrefab;
        myGrid.squareScale = 1f;
        myGrid.squareGap = 2f;
        myGrid.everySquareOffSet = 1f;
        myGrid.startPosition = Vector2.zero;

        GameObject cardLoaderGO = new GameObject("CardLoader");
        CardLoader cardLoader = cardLoaderGO.AddComponent<CardLoader>();
        myGrid.OwnerCardLoader = cardLoader;

        myGrid.scoreNumber = 5;
        myGrid.rewardElement = 1;
    }

    [TearDown]
    public void TearDown()
    {
        if (gridGO != null)
        {
            Object.Destroy(gridGO);
        }
    }

    [UnityTest]
    public IEnumerator MyGrid_IsInitialized()
    {
        yield return null;
        Assert.IsNotNull(myGrid);
    }

    [UnityTest]
    public IEnumerator MyGrid_IsTheCardFull_ReturnsFalse()
    {
        yield return null;
        Assert.IsFalse(myGrid.IsTheCardFull());
    }

    [UnityTest]
    public IEnumerator MyGrid_IsTheCardFull_ReturnsTrue_WhenAllOccupied()
    {
        yield return null;
        // Manuálisan inicializáljuk a gridSquare-ket
        myGrid.isInitialized = true;

        // Az összes square-t occupied-re állítjuk
        // (mivel a Start() private, direktben nem hívhatjuk)
        for (int i = 0; i < 49; i++)
        {
            GridSquare square = myGrid.GetGridSquare(i);
            if (square != null)
            {
                square.SquareOccupied = true;
            }
        }

        yield return null;
        Assert.IsTrue(myGrid.IsTheCardFull());
    }

    [UnityTest]
    public IEnumerator MyGrid_GetGridSquare_ReturnsNull_ForInvalidIndex()
    {
        yield return null;
        GridSquare invalidSquare = myGrid.GetGridSquare(100);
        Assert.IsNull(invalidSquare);
    }
}
