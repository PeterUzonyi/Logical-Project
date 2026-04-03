using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class GameConfigTests
{
    [SetUp]
    public void ResetDefaults()
    {
        GameConfig.PlayerCount = 2;
        GameConfig.ThinkingTime = 30f;
        GameConfig.PlayerNames = new string[] { "Játékos 1", "Játékos 2", "Játékos 3", "Játékos 4" };
        GameConfig.PlayerColors = new Color[]
        {
            new Color(0.8f, 0.2f, 0.2f),
            new Color(0.2f, 0.4f, 0.8f),
            new Color(0.2f, 0.7f, 0.3f),
            new Color(0.9f, 0.7f, 0.1f)
        };
    }

    [Test]
    public void GameConfig_DefaultPlayerCount_Is2()
    {
        Assert.AreEqual(2, GameConfig.PlayerCount);
    }

    [Test]
    public void GameConfig_DefaultThinkingTime_Is30()
    {
        Assert.AreEqual(30f, GameConfig.ThinkingTime);
    }

    [Test]
    public void GameConfig_DefaultPlayerNames_AreCorrect()
    {
        Assert.AreEqual("Játékos 1", GameConfig.PlayerNames[0]);
        Assert.AreEqual("Játékos 2", GameConfig.PlayerNames[1]);
        Assert.AreEqual("Játékos 3", GameConfig.PlayerNames[2]);
        Assert.AreEqual("Játékos 4", GameConfig.PlayerNames[3]);
    }

    [Test]
    public void GameConfig_PlayerCount_CanBeChanged()
    {
        GameConfig.PlayerCount = 4;
        Assert.AreEqual(4, GameConfig.PlayerCount);
    }

    [Test]
    public void GameConfig_ThinkingTime_CanBeChanged()
    {
        GameConfig.ThinkingTime = 60f;
        Assert.AreEqual(60f, GameConfig.ThinkingTime);
    }

    [Test]
    public void GameConfig_PlayerNames_CanBeChanged()
    {
        GameConfig.PlayerNames[0] = "Alice";
        Assert.AreEqual("Alice", GameConfig.PlayerNames[0]);
    }

    [Test]
    public void GameConfig_PlayerColors_HasFourEntries()
    {
        Assert.AreEqual(4, GameConfig.PlayerColors.Length);
    }

    [Test]
    public void GameConfig_PlayerColors_CanBeChanged()
    {
        GameConfig.PlayerColors[0] = Color.blue;
        Assert.AreEqual(Color.blue, GameConfig.PlayerColors[0]);
    }
}
