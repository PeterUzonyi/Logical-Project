using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
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

    private List<CardType> Cards = new List<CardType>();

    // Start is called before the first frame update
    void Start()
    {
        LoadCards();
        StartCoroutine(WaitForInitialization());
    }

    // Update is called once per frame
    void Update()
    {

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

                CardType card = new CardType(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), Matrix);

                Cards.Add(card);
            }
        }
    }

    IEnumerator WaitForInitialization()
    {
        Grid gridScript = Grid.GetComponent<Grid>();

        while (!gridScript.isInitialized)
        {
            //Wait, until the gridsquares are initialized
            yield return null;
        }

        Visualize();
    }

    private void Visualize()
    {
        for (int i = 0; i < 1; i++)
        {
            //Background Color
            if (Cards[i].Color == "White")
            {
                BgImage.color = Color.white;
                ScoreText.color = Color.black;

                //Grid bg
                GridBgImage.color = Color.gray;                
            }
            else if (Cards[i].Color == "Black")
            {
                BgImage.color = Color.black;
                ScoreText.color = Color.white;

                //Grid bg
                GridBgImage.color = Color.gray;
            }

            //Score
            ScoreText.text = Cards[i].Score.ToString();

            //Reward Element
            RewardImage.sprite = RewardSprites[Cards[i].RewardElement - 1];

            //Grid squares
            for (int j = 0; j < Grid.transform.childCount; j++)
            {
                var square = Grid.transform.GetChild(j).gameObject;
                var img = square.GetComponent<Image>();
                if (Cards[i].Matrix[j / 7, j % 7] == 10)
                {
                    img.color = Color.black;
                }
                else if (Cards[i].Matrix[(j / 7), (j % 7)] == 0)
                {
                    img.color = Color.white;
                }   
            }
        }
    }
}
