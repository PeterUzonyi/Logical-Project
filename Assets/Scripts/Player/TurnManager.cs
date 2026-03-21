using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    /*
    public Player player1;
    public Player player2;

    public int playerCount = 2;
    */
    [SerializeField] private List<Player> allPlayers; // Inspectorban: mind a 4 Player bekötve
    public List<Player> players = new List<Player>(); // csak az aktív játékosok
    public Player currentPlayer { get; private set; } //Soron lévõ játékos
    public int playerCount => players.Count;

    public bool isLastRound = false;
    public Player startedLastRound;
    public int lastPlayerTurn;

    public bool isVegsoRendrakas;

    public bool isGameOver = false;

    void Awake()
    {
        Instance = this;
        isLastRound = false;
        isGameOver = false;
        lastPlayerTurn = 0;
        isVegsoRendrakas = false;


        int count = PhotonNetwork.IsConnected
            ? PhotonNetwork.PlayerList.Length  // online: szobában lévõk száma
            : GameConfig.PlayerCount;           // lokális: fõmenübõl beállított

        // Csak annyi játékost aktiválunk amennyien játszanak
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (i < count)
            {
                allPlayers[i].gameObject.SetActive(true);
                players.Add(allPlayers[i]);
            }
            else
            {
                allPlayers[i].gameObject.SetActive(false);
            }
        }
    }
    void Start()
    {
        // Online módban a helyi játékos legyen players[0]
        if (PhotonNetwork.IsConnected)
        {
            // ActorNumber beállítása a Photon sorrendnek megfelelõen
            var photonPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.IsMasterClient ? 0 : 1).ToList();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerID = photonPlayers[i].ActorNumber;
                players[i].PlayerName = photonPlayers[i].NickName;
            }

            // Helyi játékos legyen players[0]
            int localIdx = players.FindIndex(p => p.PlayerID == PhotonNetwork.LocalPlayer.ActorNumber);

            if (localIdx > 0)
            {
                var tmp = players[0];
                players[0] = players[localIdx];
                players[localIdx] = tmp;
            }
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerName = GameConfig.PlayerNames[i];
            }
        }

        currentPlayer = players[0];
        currentPlayer.OpenCommonReserve();

        for (int i = 0; i < players.Count; i++)
        {
            if (i == 0)
            {
                players[i].MyTurn(true);
            }
            else
            {
                players[i].MyTurn(false);
            }
        }

        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            OnlineTurnManager.OnTurnChanged += OnOnlineTurnChanged;
            OnlineTurnManager.OnTimeUp += OnOnlineTimeUp;
        }

        /*
        // Online módban: a saját gépen a helyi játékos legyen players[0]
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayersOnline(count);
        }
        else
        {
            SpawnPlayersLocal(count);
        }

        currentPlayer = players[0];
        currentPlayer.OpenCommonReserve();

        for (int i = 0; i < players.Count; i++)
            players[i].MyTurn(i == 0);

        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            OnlineTurnManager.OnTurnChanged += OnOnlineTurnChanged;
            OnlineTurnManager.OnTimeUp += OnOnlineTimeUp;
        }
        */
        /*
        // Online módban: player1 legyen mindig a helyi játékos
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            var temp = player1;
            player1 = player2;
            player2 = temp;
        }

        currentPlayer = player1;
        //A CommonReserve Inicializálása miatt kell
        currentPlayer.OpenCommonReserve();
        player1.MyTurn(true);
        player2.MyTurn(false);
        playerCount = 2;

        // Ha online mód van, feliratkozunk az OnlineTurnManager eseményeire
        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            OnlineTurnManager.OnTurnChanged += OnOnlineTurnChanged;
            OnlineTurnManager.OnTimeUp += OnOnlineTimeUp;
        }
        */
    }

    /*
    private void SpawnPlayersLocal(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Player p = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
            p.PlayerID = i + 1;
            p.PlayerName = $"Játékos {i + 1}";
            players.Add(p);
        }
    }

    private void SpawnPlayersOnline(int count)
    {
        // MasterClient = players[0], többi sorrendben
        var photonPlayers = PhotonNetwork.PlayerList
            .OrderBy(p => p.IsMasterClient ? 0 : 1)
            .ToList();

        for (int i = 0; i < count; i++)
        {
            Player p = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
            p.PlayerID = photonPlayers[i].ActorNumber;
            p.PlayerName = photonPlayers[i].NickName;
            players.Add(p);
        }

        // Helyi játékos legyen players[0]
        int localIdx = players.FindIndex(
            p => p.PlayerID == PhotonNetwork.LocalPlayer.ActorNumber);
        if (localIdx > 0)
        {
            var tmp = players[0];
            players[0] = players[localIdx];
            players[localIdx] = tmp;
        }
    }
    */

    void OnDestroy()
    {
        OnlineTurnManager.OnTurnChanged -= OnOnlineTurnChanged;
        OnlineTurnManager.OnTimeUp -= OnOnlineTimeUp;
    }

    public void EndTurn()
    {
        if (currentPlayer == startedLastRound && lastPlayerTurn > 1 && isLastRound)
        {
            VegsoRendrakas();
        }

        if (currentPlayer == startedLastRound && lastPlayerTurn > 1 && isVegsoRendrakas)
        {
            isGameOver = true;
            Debug.Log("Game Over");
        }

        // Következõ játékos körbe forgatva
        int idx = players.IndexOf(currentPlayer);
        int nextIdx = (idx + 1) % players.Count;

        currentPlayer = players[nextIdx];

        for (int i = 0; i < players.Count; i++)
        {
            if (i == nextIdx)
            {
                players[i].MyTurn(true);
            }
            else
            {
                players[i].MyTurn(false);
            }
            
        }
        /*
        if (currentPlayer == player1) 
        {
            currentPlayer = player2;
            player1.MyTurn(false);
            player2.MyTurn(true);
        }
        else 
        {
            currentPlayer = player1;
            player1.MyTurn(true);
            player2.MyTurn(false);
        }
        */
        if (isLastRound)
        {
            lastPlayerTurn++;
        }
        
    }

    public void LastRound()
    {
        isLastRound = true;
        startedLastRound = currentPlayer;
    }

    public void VegsoRendrakas()
    {
        Debug.Log("Végsõ Rendrakás");
        isVegsoRendrakas = true;
        lastPlayerTurn = 0;
        startedLastRound = currentPlayer;
    }

    private void OnOnlineTurnChanged(int actorNumber)
    {
        // Az actorNumber alapján döntjük el ki a currentPlayer
        // 1. MasterClient = player1, 2. másik játékos = player2 (Photon sorrendben)
        var next=players.FirstOrDefault(p => p.PlayerID == actorNumber);
        if (next == null)
        {
            return;
        }

        currentPlayer = next;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == currentPlayer)
            {
                players[i].MyTurn(true);
            }
            else
            {
                players[i].MyTurn(false);
            }
        }
        /*
        var photonPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
        if (photonPlayer == null)
        {
            return;
        }

        bool masterClientIsNext = photonPlayer.IsMasterClient;

        if (masterClientIsNext)
        {
            currentPlayer = player1;
            player1.MyTurn(true);
            player2.MyTurn(false);
        }
        else
        {
            currentPlayer = player2;
            player1.MyTurn(false);
            player2.MyTurn(true);
        }
        */
    }
    private void OnOnlineTimeUp()
    {
        // Ha lejárt az idõ, a játékos körét befejezettnek tekintjük
        // (az OnlineTurnManager MasterClient-en már léptette a kört)
        Debug.Log("Idõ lejárt, kör kényszerített vége.");
    }
}
