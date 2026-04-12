using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

/// <summary>
/// Online körkezelõ. A játék jelenetben egy GameObjecthez csatold.
/// PhotonView komponens is szükséges ugyanezen az objektumon.
/// 
/// Használat a saját GameManager-edben:
///   - Iratkozz fel az OnTurnChanged és OnTimeUp eseményekre
///   - Lépés elküldésekor hívd: OnlineTurnManager.Instance.SubmitMove()
///   - IsMyTurn property-vel ellenõrizd hogy az adott kliens léphet e
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class OnlineTurnManager : MonoBehaviourPun
{
    public static OnlineTurnManager Instance { get; private set; }

    /// <summary>
    /// Events
    /// </summary>
    public static event System.Action<int> OnTurnChanged;  // actorNumber ki következik
    public static event System.Action OnTimeUp;
    public static event System.Action<float> OnTimerTick;    // hátralévõ idõ

    /// <summary>
    /// Online number of players (active players)
    /// </summary>
    private int activeActorNumber = -1;

    /// <summary>
    /// Setting thinking time
    /// </summary>
    private float thinkingTime = 30f;

    /// <summary>
    /// Remaining time
    /// </summary>
    private float remainingTime;

    /// <summary>
    /// Whether the timer is counting
    /// </summary>
    private bool timerRunning = false;

    /// <summary>
    /// Whetehr the timer hits 0
    /// </summary>
    private bool timeUpHandle = false;

    public int ActiveActorNumber => activeActorNumber;

    /// <summary>
    /// Online mode. True, when it is the current player's turn
    /// </summary>
    public bool IsMyTurn => PhotonNetwork.LocalPlayer.ActorNumber == activeActorNumber;

    //Called when the script is loaded
    void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        // Csak online módban töltjük fel Photon adatokból
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        // Játékos színek betöltése Photon property-kbõl
        var palette = new Color[]
        {
        new Color(0.85f, 0.22f, 0.22f),
        new Color(0.22f, 0.45f, 0.85f),
        new Color(0.22f, 0.72f, 0.33f),
        new Color(0.95f, 0.75f, 0.10f),
        new Color(0.70f, 0.25f, 0.80f),
        new Color(0.95f, 0.50f, 0.10f),
        };

        var players = PhotonNetwork.PlayerList.OrderBy(p => p.IsMasterClient ? 0 : 1).ToList();

        GameConfig.PlayerCount = players.Count;
        for (int i = 0; i < players.Count; i++)
        {
            int colorIndex = 0;
            if (players[i].CustomProperties.TryGetValue("colorIndex", out var ci))
            {
                colorIndex = (int)ci;
            }
                
            GameConfig.PlayerColors[i] = palette[colorIndex];
            GameConfig.PlayerNames[i] = players[i].NickName;
        }
    }

    //Start is called before the first frame update
    void Start()
    {
        // Gondolkodási idõ beolvasása a szoba beállításaiból
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("thinkTime", out var t))
        {
            thinkingTime = System.Convert.ToSingle(t);
        }

        // Csak a MasterClient indítja az elsõ kört
        if (PhotonNetwork.IsMasterClient)
        {
            //Késleltetni kell
            StartCoroutine(StartFirstTurn());
        }
    }

    /// <summary>
    /// Wait for the first round to be initialized
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartFirstTurn()
    {
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(RPC_SetTurn),
            RpcTarget.All,
            PhotonNetwork.MasterClient.ActorNumber);
    }

    //Update is called once per frame
    void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        OnTimerTick?.Invoke(remainingTime);

        if (remainingTime <= 0f && !timeUpHandle)
        {
            timeUpHandle = true;
            timerRunning = false;
            // Csak a MasterClient lépteti tovább ha lejár az idõ
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_TimeUp), RpcTarget.All);
            }
                
            OnTimeUp?.Invoke();
        }
    }




    /// <summary>
    /// Online mode. Called when the current player finished an action or the round
    /// </summary>
    public void SubmitMove()
    {
        if (!IsMyTurn)
        {
            return;
        }

        photonView.RPC(nameof(RPC_MoveSubmitted),
            RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber);
    }

    /// <summary>
    /// Online mode. After the move is submitted, then the next round starts
    /// </summary>
    /// <param name="actorNumber"></param>
    [PunRPC]
    private void RPC_MoveSubmitted(int actorNumber)
    {
        // Csak a MasterClient dolgozza fel
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // dupla küldés védelem
        if (actorNumber != activeActorNumber)
        {
            return;
        }

        NextTurn();
    }

    /// <summary>
    /// Online mode. Next round
    /// </summary>
    private void NextTurn()
    {
        var players = PhotonNetwork.PlayerList;
        int currentIdx = System.Array.FindIndex(players, p => p.ActorNumber == activeActorNumber);
        int nextIdx = (currentIdx + 1) % players.Length;

        photonView.RPC(nameof(RPC_SetTurn),
            RpcTarget.All,
            players[nextIdx].ActorNumber);
    }

    /// <summary>
    /// Synchronizes the last round for every player
    /// </summary>
    /// <param name="playerID"></param>
    public void SyncLastRound(int playerID)
    {
        photonView.RPC(nameof(RPC_LastRound), RpcTarget.All, playerID);
    }

    /// <summary>
    /// Synchronizes the Final Touches (Végsõ rendrakás) round for every player
    /// </summary>
    /// <param name="startingPlayerID"></param>
    public void SyncVegsoRendrakas(int startingPlayerID)
    {
        photonView.RPC(nameof(RPC_VegsoRendrakas), RpcTarget.All, startingPlayerID);
    }

    /// <summary>
    /// Synchronizes the Game Over for every player
    /// </summary>
    public void SyncGameOver()
    {
        // Elõször mindenki elküldi a saját adatait, majd a MasterClient elindítja a GameOver-t
        // Kis késleltetés kell hogy az RPC-k megérkezzenek
        StartCoroutine(SyncStatsAndGameOver());
    }

    /// <summary>
    /// Waits for every player to send their stats for the Game Over
    /// </summary>
    /// <returns></returns>
    private IEnumerator SyncStatsAndGameOver()
    {
        // Mindenki küldje el a saját statját
        photonView.RPC(nameof(RPC_RequestStatSync), RpcTarget.All);

        // Várunk hogy az adatok megérkezzenek
        yield return new WaitForSeconds(0.5f);

        // Most már mehet a GameOver
        photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }

    /// <summary>
    /// Synchronizes every player's stats
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="score"></param>
    /// <param name="completedPuzzles"></param>
    /// <param name="remainingElements"></param>
    public void SyncPlayerStats(int playerID, int score, int completedPuzzles, int remainingElements)
    {
        photonView.RPC(nameof(RPC_SyncPlayerStats), RpcTarget.All, playerID, score, completedPuzzles, remainingElements);
    }

    /// <summary>
    /// Requests every players to send their stats
    /// </summary>
    [PunRPC]
    private void RPC_RequestStatSync()
    {
        // Mindenki elküldi a saját helyi játékosának adatait
        Player localPlayer = TurnManager.Instance.players.FirstOrDefault(p => p.PhotonActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        if (localPlayer != null)
        {
            localPlayer.SyncStatsToAll();
        }
    }

    /// <summary>
    /// Synchronizes every player's stats
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="score"></param>
    /// <param name="completedPuzzles"></param>
    /// <param name="remainingElements"></param>
    [PunRPC]
    private void RPC_SyncPlayerStats(int playerID, int score, int completedPuzzles, int remainingElements)
    {
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null)
        {
            return;
        }

        player.PlayerScore = score;
        player.CompletedPuzzles = completedPuzzles;
        player.RemainingElements = remainingElements;

        // Pontszám UI frissítése
        if (player.Score != null)
        {
            player.Score.text = score.ToString();
        }            
    }

    /// <summary>
    /// Online mode. Last round for every player
    /// </summary>
    public void SyncLastRoundExtra()
    {
        photonView.RPC(nameof(RPC_SetLastRoundExtra), RpcTarget.All);
    }

    /// <summary>
    /// Online mode. Last round
    /// </summary>
    /// <param name="playerID"></param>
    [PunRPC]
    private void RPC_LastRound(int playerID)
    {
        TurnManager.Instance.ApplyLastRound(playerID);
    }

    /// <summary>
    /// Online mode. Final Touches (Végsõ rendrakás)
    /// </summary>
    /// <param name="startingPlayerID"></param>
    [PunRPC]
    private void RPC_VegsoRendrakas(int startingPlayerID)
    {
        TurnManager.Instance.ApplyVegsoRendrakas(startingPlayerID);
    }

    /// <summary>
    /// Online mode. Game Over
    /// </summary>
    [PunRPC]
    private void RPC_GameOver()
    {
        TurnManager.Instance.ApplyGameOver();
    }

    /// <summary>
    /// Online mode. Last round
    /// </summary>
    [PunRPC]
    private void RPC_SetLastRoundExtra()
    {
        TurnManager.Instance.lastRoundExtra = true;
    }

    /// <summary>
    /// Online mode. Starting next turn with the correct player
    /// </summary>
    /// <param name="actorNumber"></param>
    [PunRPC]
    private void RPC_SetTurn(int actorNumber)
    {
        // Ez minden kliensen fut egyszerre
        activeActorNumber = actorNumber;
        remainingTime = thinkingTime;
        timerRunning = true;
        timeUpHandle = false;
        OnTurnChanged?.Invoke(actorNumber);
    }

    /// <summary>
    /// Online mode. Reset timer for every player
    /// </summary>
    public void SyncResetTimer()
    {
        photonView.RPC(nameof(RPC_ResetTimer), RpcTarget.All);
    }

    /// <summary>
    /// Online mode. Reset the timer
    /// </summary>
    [PunRPC]
    private void RPC_ResetTimer()
    {
        remainingTime = thinkingTime;
        timerRunning = true;
        timeUpHandle = false;
        ThinkingTimer.Instance?.ResetTimer();
    }

    /// <summary>
    /// Online mode. When the timer hits 0
    /// </summary>
    [PunRPC]
    private void RPC_TimeUp()
    {
        // Minden kliensen resetelünk
        remainingTime = thinkingTime;
        timerRunning = true;
        timeUpHandle = false;
        ThinkingTimer.Instance?.ResetTimer();

        Player current = TurnManager.Instance?.currentPlayer;
        if (current == null)
        {
            return;
        }

        // Csak a soron lévõ játékos kliensén fut le ténylegesen
        if (current.PhotonActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            return;
        }

        // TimeIsUp visszaállítása mielõtt ActionHasEnded lefut
        if (ThinkingTimer.Instance != null)
        {
            ThinkingTimer.Instance.TimeIsUp = false;
        }

        if (TurnManager.Instance.isVegsoRendrakas)
        {
            current.OnEndVegsoRendrakasClicked();
        }
        else
        {
            current.ActionHasEnded();
        }
    }
}
