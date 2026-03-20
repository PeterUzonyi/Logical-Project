using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    public TextAsset cardFile;

    public List<CardType> WhiteCards { get; private set; } = new List<CardType>();
    public List<CardType> BlackCards { get; private set; } = new List<CardType>();

    public bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        LoadCards();
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
                
                DontDestroyOnLoad(cardObject);

                if (card.Color == "White")
                {
                    WhiteCards.Add(card);
                }
                else
                {
                    BlackCards.Add(card);
                }
            }
        }

        BlackCards = ShuffleList(BlackCards);
        WhiteCards = ShuffleList(WhiteCards);

        IsReady = true;
    }

    /// <summary>
    /// Húz egy lapot a megadott pakliból, majd kiveszi onnan.
    /// color: "White" vagy "Black"
    /// </summary>
    public CardType DrawCard(string color)
    {
        List<CardType> deck = null;
        if (color == "White")
        {
            deck = WhiteCards;
        }
        else
        {
            deck = BlackCards;
        }

        if (deck.Count == 0)
        {
            Debug.LogWarning($"A(z) {color} pakli üres!");
            return null;
        }

        CardType drawn = deck[0];
        deck.RemoveAt(0);

        return drawn;
    }

    /// <summary>
    /// Visszaadja a pakli tetején lévõ lapot kivétel nélkül (csak betekintés).
    /// </summary>
    public CardType PeekCard(string color)
    {
        List<CardType> deck = null;
        if (color == "White")
        {
            deck = WhiteCards;
        }
        else
        {
            deck = BlackCards;
        }

        return deck.Count > 0 ? deck[0] : null;
    }

    //Fisher-Yates shuffle algorithm
    public List<CardType> ShuffleList(List<CardType> cardList)
    {
        /*
            Megkapja a teljes listát, mit ketté választ
            fehérre és feketére (játékosok számától függ a fekete pakli mérete (összesen 20 lap))
            (2 játékos: 12 fekete lap| 3 játékos: 14 fekete lap| 4 játékos: 16 fekete lap)
            és ezeket keveri meg külön-külön ez lesz az alaphelyzete a játéknak
        */
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            CardType temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }

        if (cardList[0].Color == "Black")
        {
            //A playerCount jelenleg 2
            int count = TurnManager.Instance.playerCount;//Itt a gond
            int startIndex = 12;
            int range = 0;

            switch (count)
            {
                case 2:
                    startIndex = 12;
                    break;
                case 3:
                    startIndex = 14;
                    break;
                case 4:
                    startIndex = 16;
                    break;
                default:
                    startIndex = 12;
                    break;
            }

            range = 20 - startIndex;

            //A megkevert fekete pakliból csak a játékosok számától függû db fekete lapot adunk vissza
            cardList.RemoveRange(startIndex, range);
        }

        return cardList;
    }
}
