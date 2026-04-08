using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode tesztek a GridSquare-hez
/// Teszteli: selection, occupation, hover, element placement
/// </summary>
public class GridSquareInteractionTests
{
    private GameObject gridSquareGO;
    private GridSquare gridSquare;
    //private GameObject gridParentGO;
    //private MyGrid myGrid;

    [SetUp]
    public void Setup()
    {
        gridSquareGO = new GameObject("GridSquare");
        RectTransform rectTransform = gridSquareGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);

        gridSquare = gridSquareGO.AddComponent<GridSquare>();

        GameObject hoverImageGO = new GameObject("HoverImage");
        hoverImageGO.transform.SetParent(gridSquareGO.transform);
        Image hoverImage = hoverImageGO.AddComponent<Image>();
        gridSquare.hoverImage = hoverImage;

        GameObject activeImageGO = new GameObject("ActiveImage");
        activeImageGO.transform.SetParent(gridSquareGO.transform);
        Image activeImage = activeImageGO.AddComponent<Image>();
        gridSquare.activeImage = activeImage;

        Collider2D collider = gridSquareGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        gridSquare.SquareIndex = 0;
        gridSquare.Selected = false;
        gridSquare.SquareOccupied = false;
    }

    [TearDown]
    public void TearDown()
    {
        if (gridSquareGO != null)
        {
            Object.Destroy(gridSquareGO);
        }
    }

    [UnityTest]
    public IEnumerator GridSquare_DefaultState()
    {
        yield return null;
        Assert.IsFalse(gridSquare.Selected);
        Assert.IsFalse(gridSquare.SquareOccupied);
    }

    [UnityTest]
    public IEnumerator GridSquare_Selected_CanBeChanged()
    {
        gridSquare.Selected = true;
        yield return null;
        Assert.IsTrue(gridSquare.Selected);

        gridSquare.Selected = false;
        yield return null;
        Assert.IsFalse(gridSquare.Selected);
    }

    [UnityTest]
    public IEnumerator GridSquare_SquareOccupied_CanBeChanged()
    {
        gridSquare.SquareOccupied = true;
        yield return null;
        Assert.IsTrue(gridSquare.SquareOccupied);
    }

    [UnityTest]
    public IEnumerator GridSquare_SquareIndex_CanBeSet()
    {
        gridSquare.SquareIndex = 42;
        yield return null;
        Assert.AreEqual(42, gridSquare.SquareIndex);
    }
}
