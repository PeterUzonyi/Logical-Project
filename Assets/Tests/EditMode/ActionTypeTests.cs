using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class ActionTypeTests
{
    [Test]
    public void ActionType_TakePuzzle_HasCorrectValue()
    {
        Assert.AreEqual(0, (int)ActionType.TakePuzzle);
    }

    [Test]
    public void ActionType_TakeElement_HasCorrectValue()
    {
        Assert.AreEqual(1, (int)ActionType.TakeElement);
    }

    [Test]
    public void ActionType_UpgradeElement_HasCorrectValue()
    {
        Assert.AreEqual(2, (int)ActionType.UpgradeElement);
    }

    [Test]
    public void ActionType_PlaceElement_HasCorrectValue()
    {
        Assert.AreEqual(3, (int)ActionType.PlaceElement);
    }

    [Test]
    public void ActionType_MasterAction_HasCorrectValue()
    {
        Assert.AreEqual(4, (int)ActionType.MasterAction);
    }

    [Test]
    public void ActionType_TotalCount_IsFive()
    {
        var values = System.Enum.GetValues(typeof(ActionType));
        Assert.AreEqual(5, values.Length);
    }

    [Test]
    public void ActionType_CanBeCastFromInt()
    {
        ActionType action = (ActionType)3;
        Assert.AreEqual(ActionType.PlaceElement, action);
    }

    [Test]
    public void ActionType_CanBeCompared()
    {
        ActionType a = ActionType.TakePuzzle;
        ActionType b = ActionType.TakePuzzle;
        Assert.AreEqual(a, b);
    }
}
