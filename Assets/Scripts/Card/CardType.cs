using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardType : MonoBehaviour
{
    public string Color;
    public int Score;
    public int RewardElement;
    public int[,] Matrix = new int[7,7];

    public CardType(string color, int score, int rewardElement, int[,] matrix)
    {
        Color = color;
        Score = score;
        RewardElement = rewardElement;
        Matrix = matrix;
    }
}
