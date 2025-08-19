using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CardLoader : MonoBehaviour
{
    public TextAsset cardFile; // Drag & drop ide a fájlt Unity-ben

    public List<CardType> LoadCards()
    {
        List<CardType> cards = new List<CardType>();
        string[] lines = cardFile.text.Split('\n');

        foreach (string line in lines)
        {
            string[] parts = line.Trim().Split(';');
            if (parts.Length < 52)
            {
                int[,] Matrix = new int[7, 7];
                for (int i = 0; i < 49; i++)
                {
                    int row = i / 7;
                    int col = i % 7;
                    Matrix[row, col] = int.Parse(parts[i + 3]);
                }

                CardType card = new CardType(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), Matrix);

                cards.Add(card);
            }
        }

        return cards;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
