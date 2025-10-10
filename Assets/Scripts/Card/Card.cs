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
    public TextMeshProUGUI text;
    
    public Sprite[] RewardSprites;
    public Image RewardImage;


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
        for (int i = 0; i < 1; i++)
        {
            //Background Color
            if (Cards[i].Color == "White")
            {
                BgImage.color = Color.white;
                text.color = Color.black;
            }
            else if (Cards[i].Color == "Black")
            {
                BgImage.color = Color.black;
                text.color = Color.white;
            }

            //Score
            text.text = Cards[i].Score.ToString();

            //Reward Element
            RewardImage.sprite = RewardSprites[Cards[i].RewardElement - 1];

            //Matrix

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
