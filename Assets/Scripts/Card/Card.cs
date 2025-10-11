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
    public List<CardType> Cards = new List<CardType>();
    
    public Image BgImage;
    public TextMeshProUGUI ScoreText;
    
    public Sprite[] RewardSprites;
    public Image RewardImage;

    public Image GridBgImage;
    public Image GridSquareImage;

    public void LoadCards()
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

    public void Visualize()
    {
        for (int i = 51; i < 52; i++)
        {
            //Background Color
            if (Cards[i].Color == "White")
            {
                BgImage.color = Color.white;
                ScoreText.color = Color.black;

                //Grid bg
                GridBgImage.color = Color.black;
                GridSquareImage.color = Color.white;
            }
            else if (Cards[i].Color == "Black")
            {
                BgImage.color = Color.black;
                ScoreText.color = Color.white;

                //Grid bg
                GridBgImage.color = Color.white;
                GridSquareImage.color = Color.black;
            }

            //Score
            ScoreText.text = Cards[i].Score.ToString();

            //Reward Element
            RewardImage.sprite = RewardSprites[Cards[i].RewardElement - 1];

            //Grid squares

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadCards();
        Visualize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
