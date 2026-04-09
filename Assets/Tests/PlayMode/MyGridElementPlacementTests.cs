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

        GameObject cardManagerGO = new GameObject("CardManager");
        CardManager cardManager = cardManagerGO.AddComponent<CardManager>();

        GameObject gridSquarePrefab = new GameObject("GridSquare");
        gridSquarePrefab.AddComponent<RectTransform>();
        GridSquare gridSquareScript = gridSquarePrefab.AddComponent<GridSquare>();
        Image img = gridSquarePrefab.AddComponent<Image>();
        gridSquareScript.activeImage = img;

        myGrid.columns = 7;
        myGrid.rows = 7;
        myGrid.gridSquare = gridSquarePrefab;
        myGrid.isInitialized = true;

        GameObject cardLoaderGO = new GameObject("CardLoader");
        CardLoader cardLoader = cardLoaderGO.AddComponent<CardLoader>();

        cardLoader.Grid = gridGO;
        cardLoader.gridScript = myGrid;

        myGrid.OwnerCardLoader = cardLoader;
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
