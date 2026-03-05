using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class CardLoader : MonoBehaviour
{
    public TextAsset cardFile; // Drag & drop ide a fájlt Unity-ben
    
    public Image BgImage;
    public TextMeshProUGUI ScoreText;
    
    public Sprite[] RewardSprites;
    public Image RewardImage;

    public Image GridBgImage;
    public GameObject Grid;

    public List<CardType> Cards = new List<CardType>();
    public Grid gridScript;

    // Start is called before the first frame update
    void Start()
    {
        LoadCards();
        StartCoroutine(WaitForInitialization());
    }

    private void LoadCards()
    {
        string[] lines = cardFile.text.Split('\n');

        foreach (string line in lines)
        {
            string[] parts = line.Split(';');
            if (parts.Length == 52)
            {
                int[,] Matrix = new int[7, 7];
                for (int i = 0; i < 49; i++)
                {
                    int row = i / 7;
                    int col = i % 7;
                    Matrix[row, col] = int.Parse(parts[i + 3]);
                }

                GameObject cardObject = new GameObject("Card");
                CardType card = cardObject.AddComponent<CardType>();
                card.Color = parts[0];
                card.Score = int.Parse(parts[1]);
                card.RewardElement = int.Parse(parts[2]);
                card.Matrix = Matrix;
                //CardType card = new CardType(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), Matrix);

                Cards.Add(card);
            }
        }
    }

    IEnumerator WaitForInitialization()
    {
        gridScript = Grid.GetComponent<Grid>();

        while (!gridScript.isInitialized)
        {
            //Wait, until the gridsquares are initialized
            yield return null;
        }

        Visualize();
    }

    private void Visualize()
    {
        Cards = ShuffleList(Cards);

        //Background Color
        if (Cards[0].Color == "White")
        {
            BgImage.color = Color.white;
            ScoreText.color = Color.black;

            //Grid bg
            GridBgImage.color = Color.gray;
        }
        else if (Cards[0].Color == "Black")
        {
            BgImage.color = Color.black;
            ScoreText.color = Color.white;

            //Grid bg
            GridBgImage.color = Color.gray;
        }

        //Score
        ScoreText.text = Cards[0].Score.ToString();
        gridScript.scoreNumber = Cards[0].Score;

        //Reward Element
        RewardImage.sprite = RewardSprites[Cards[0].RewardElement - 1];
        gridScript.rewardElement = Cards[0].RewardElement - 1;

        //Grid squares
        for (int i = 0; i < Grid.transform.childCount; i++)
        {
            var square = Grid.transform.GetChild(i).gameObject;
            var img = square.GetComponent<Image>();
            if (Cards[0].Matrix[i / 7, i % 7] == 10)
            {
                img.color = Color.black;
            }
            else if (Cards[0].Matrix[(i / 7), (i % 7)] == 0)
            {
                img.color = Color.white;
            }
        }
    }

    //Fisher-Yates shuffle algorithm
    public List<CardType> ShuffleList(List<CardType> cardList)
    {
        /*
            Megkapja a teljes listát, mit ketté választ
            fehérre és feketére (játékosok számától függ a fekete pakli mérete) 
            és ezeket keveri meg külön-külön ez lesz az alaphelyzete a játéknak
        */
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            CardType temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }
        return cardList;
    }
}
