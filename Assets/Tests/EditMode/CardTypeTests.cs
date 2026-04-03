using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class CardTypeTests
{
    private int[,] CreateEmptyMatrix()
    {
        return new int[7, 7];
    }


    [Test]
    public void CardType_WhiteCard_ColorIsWhite()
    {
        var card = new CardType("White", 5, 1, CreateEmptyMatrix());
        Assert.AreEqual("White", card.Color);
    }

    [Test]
    public void CardType_BlackCard_ColorIsBlack()
    {
        var card = new CardType("Black", 3, 2, CreateEmptyMatrix());
        Assert.AreEqual("Black", card.Color);
    }

    [Test]
    public void CardType_Score_IsSetCorrectly()
    {
        var card = new CardType("White", 10, 1, CreateEmptyMatrix());
        Assert.AreEqual(10, card.Score);
    }

    [Test]
    public void CardType_RewardElement_IsSetCorrectly()
    {
        var card = new CardType("Black", 3, 5, CreateEmptyMatrix());
        Assert.AreEqual(5, card.RewardElement);
    }

    [Test]
    public void CardType_UniqueID_IsSetCorrectly()
    {
        var card = new CardType("White", 5, 1, CreateEmptyMatrix(), 42);
        Assert.AreEqual(42, card.UniqueID);
    }

    [Test]
    public void CardType_Matrix_IsCorrectSize()
    {
        var matrix = CreateEmptyMatrix();
        var card = new CardType("White", 5, 1, matrix);
        Assert.AreEqual(7, card.Matrix.GetLength(0));
        Assert.AreEqual(7, card.Matrix.GetLength(1));
    }

    [Test]
    public void CardType_Matrix_StoresValuesCorrectly()
    {
        var matrix = CreateEmptyMatrix();
        matrix[0, 0] = 10;
        matrix[3, 4] = 10;

        var card = new CardType("White", 5, 1, matrix);

        Assert.AreEqual(10, card.Matrix[0, 0]);
        Assert.AreEqual(10, card.Matrix[3, 4]);
        Assert.AreEqual(0, card.Matrix[1, 1]);
    }
}
