using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

/// <summary>
/// Controls both the white and the black puzzle decks
/// </summary>
public class CardManager : MonoBehaviourPun
{
    public static CardManager Instance { get; private set; }

    public string filePath = "Cards/Cards";

    /// <summary>
    /// The white puzzle cards deck
    /// </summary>
    public List<CardType> WhiteCards { get; private set; } = new List<CardType>();

    /// <summary>
    /// The black puzzle cards deck
    /// </summary>
    public List<CardType> BlackCards { get; private set; } = new List<CardType>();

    /// <summary>
    /// True, when the cardmanager is initialized
    /// </summary>
    public bool IsReady { get; private set; } = false;


    //Called when the script is loaded
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Start is called before the first frame update
    void Start()
    {
        LoadCards(filePath);
    }

    /// <summary>
    /// Load every cards from the .txt file and store them in the two decks (white and black). 
    /// Then shuffle both decks
    /// </summary>
    private void LoadCards(string path)
    {
        int idCounter = 0;
        TextAsset file = Resources.Load<TextAsset>(path);
        string[] lines = file.text.Split('\n');

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

                CardType card = new CardType(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), Matrix, idCounter);
                idCounter++;

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

        if (PhotonNetwork.IsConnected)
        {
            //Online
            if (PhotonNetwork.IsMasterClient)
            {
                // Csak a host kever és vágja le a fekete paklit
                BlackCards = ShuffleList(BlackCards);
                WhiteCards = ShuffleList(WhiteCards);

                // MasterClient elküldi a keverés sorrendjét
                int[] blackOrder = BlackCards.Select(c => c.UniqueID).ToArray();
                int[] whiteOrder = WhiteCards.Select(c => c.UniqueID).ToArray();
                photonView.RPC(nameof(RPC_SyncCardOrder), RpcTarget.Others, blackOrder, whiteOrder);
                IsReady = true;
            }
            else
            {
                //Többi cliens várja az RPC-t
                IsReady = false;
            }
        }
        else
        {
            //Lokális
            BlackCards = ShuffleList(BlackCards);
            WhiteCards = ShuffleList(WhiteCards);
            IsReady = true;
        }
    }

    /// <summary>
    /// In online mode, the order of both decks must be the same at every player. 
    /// This method arranges the client's decks to be the same as the MasterClient's decks.
    /// </summary>
    /// <param name="blackOrder"></param>
    /// <param name="whiteOrder"></param>
    [PunRPC]
    private void RPC_SyncCardOrder(int[] blackOrder, int[] whiteOrder)
    {
        // Átrendezi a fekete paklit a MasterClient sorrendje szerint
        BlackCards = blackOrder.Select(id => BlackCards.FirstOrDefault(c => c.UniqueID == id)).Where(c => c != null).ToList();

        // Átrendezi a fehér paklit a MasterClient sorrendje szerint
        WhiteCards = whiteOrder.Select(id => WhiteCards.FirstOrDefault(c => c.UniqueID == id)).Where(c => c != null).ToList();

        // Most már szinkronban van, készen áll
        IsReady = true;
    }

    /// <summary>
    /// Draw a card from the given color deck
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
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
    /// Shuffles the given deck using the Fisher-Yetes shuffle algorithm
    /// </summary>
    /// <param name="cardList"></param>
    /// <returns>
    /// With the black puzzle deck, after the shuffling, it removes some cards from the deck to get 
    /// the correct amount of cards for the game depending on the number of players
    /// </returns>
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
            //A count 2-4
            int count = 2;
            if (TurnManager.Instance != null)
            {
                count = TurnManager.Instance.playerCount;
            }
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
