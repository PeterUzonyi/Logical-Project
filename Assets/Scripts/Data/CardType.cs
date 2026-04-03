using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Puzzle card structure
/// </summary>
public class CardType// : MonoBehaviour
{
    /// <summary>
    /// Color of the puzzle card (black or white)
    /// </summary>
    public string Color;

    /// <summary>
    /// Getting this, when the puzzle is completed (reward score)
    /// </summary>
    public int Score;

    /// <summary>
    /// Getting this, when the puzzle is completed (reward element)
    /// </summary>
    public int RewardElement;

    /// <summary>
    /// The puzzle it self (grid)
    /// </summary>
    public int[,] Matrix = new int[7,7];

    /// <summary>
    /// The ID of the card, need for the synchronized shuffle
    /// </summary>
    public int UniqueID;

    public CardType(string color, int score, int rewardElement, int[,] matrix)
    {
        Color = color;
        Score = score;
        RewardElement = rewardElement;
        Matrix = matrix;
    }

    public CardType(string color, int score, int rewardElement, int[,] matrix, int uniqueID)
    {
        Color = color;
        Score = score;
        RewardElement = rewardElement;
        Matrix = matrix;
        UniqueID = uniqueID;
    }
}
