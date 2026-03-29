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

    // Esemény: a GameManager-ed feliratkozhat rá
    public static event System.Action<int> OnTurnChanged;  // actorNumber ki következik
    public static event System.Action OnTimeUp;
    public static event System.Action<float> OnTimerTick;    // hátralévõ idõ

    private int activeActorNumber = -1;
    private float thinkingTime = 30f;
    private float remainingTime;
    private bool timerRunning = false;
    private bool timeUpHandle = false;

    public int ActiveActorNumber => activeActorNumber;

    // Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Gondolkodási idõ beolvasása a szoba beállításaiból
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("thinkTime", out var t))
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

    private IEnumerator StartFirstTurn()
    {
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(RPC_SetTurn),
            RpcTarget.All,
            PhotonNetwork.MasterClient.ActorNumber);
    }

    void Update()
    {
        if (!timerRunning) return;

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        OnTimerTick?.Invoke(remainingTime);

        if (remainingTime <= 0f && !timeUpHandle)
        {
            timeUpHandle = true;
            timerRunning = false;
            // Csak a MasterClient lépteti tovább ha lejár az idõ
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RPC_TimeUp), RpcTarget.All);
            OnTimeUp?.Invoke();
        }
    }

    // Publikus API

    /// <summary>
    /// Igaz ha a helyi játékos van soron.
    /// A GameManager-edben ezzel ellenõrizd hogy engedélyezed e a lépést.
    /// </summary>
    public bool IsMyTurn => PhotonNetwork.LocalPlayer.ActorNumber == activeActorNumber;

    /// <summary>
    /// Hívd meg amikor a helyi játékos elvégezte a lépését.
    /// </summary>
    public void SubmitMove()
    {
        if (!IsMyTurn) return;

        photonView.RPC(nameof(RPC_MoveSubmitted),
            RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber);
    }

    // RPC-k (minden kliensen futnak) 

    [PunRPC]
    private void RPC_MoveSubmitted(int actorNumber)
    {
        // Csak a MasterClient dolgozza fel
        if (!PhotonNetwork.IsMasterClient) return;
        if (actorNumber != activeActorNumber) return; // dupla küldés védelem

        NextTurn();
    }

    private void NextTurn()
    {
        var players = PhotonNetwork.PlayerList;
        int currentIdx = System.Array.FindIndex(players,
            p => p.ActorNumber == activeActorNumber);
        int nextIdx = (currentIdx + 1) % players.Length;

        photonView.RPC(nameof(RPC_SetTurn),
            RpcTarget.All,
            players[nextIdx].ActorNumber);
    }

    public void SyncLastRound(int playerID)
    {
        photonView.RPC(nameof(RPC_LastRound), RpcTarget.All, playerID);
    }

    public void SyncVegsoRendrakas(int startingPlayerID)
    {
        photonView.RPC(nameof(RPC_VegsoRendrakas), RpcTarget.All, startingPlayerID);
    }

    public void SyncGameOver()
    {
        //photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);

        // Elõször mindenki elküldi a saját adatait, majd a MasterClient elindítja a GameOver-t
        // Kis késleltetés kell hogy az RPC-k megérkezzenek
        StartCoroutine(SyncStatsAndGameOver());
    }

    private IEnumerator SyncStatsAndGameOver()
    {
        // Mindenki küldje el a saját statját
        photonView.RPC(nameof(RPC_RequestStatSync), RpcTarget.All);

        // Várunk hogy az adatok megérkezzenek
        yield return new WaitForSeconds(0.5f);

        // Most már mehet a GameOver
        photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }

    public void SyncPlayerStats(int playerID, int score, int completedPuzzles, int remainingElements)
    {
        photonView.RPC(nameof(RPC_SyncPlayerStats), RpcTarget.All,
            playerID, score, completedPuzzles, remainingElements);
    }

    [PunRPC]
    private void RPC_RequestStatSync()
    {
        // Mindenki elküldi a saját helyi játékosának adatait
        Player localPlayer = TurnManager.Instance.players
            .FirstOrDefault(p => p.PhotonActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        if (localPlayer != null)
        {
            localPlayer.SyncStatsToAll();
        }
    }

    [PunRPC]
    private void RPC_SyncPlayerStats(int playerID, int score, int completedPuzzles, int remainingElements)
    {
        Player player = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null) return;

        player.PlayerScore = score;
        player.CompletedPuzzles = completedPuzzles;
        player.RemainingElements = remainingElements;

        // Pontszám UI frissítése
        if (player.Score != null)
            player.Score.text = score.ToString();
    }

    public void SyncLastRoundExtra()
    {
        photonView.RPC(nameof(RPC_SetLastRoundExtra), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_LastRound(int playerID)
    {
        TurnManager.Instance.ApplyLastRound(playerID);
    }

    [PunRPC]
    private void RPC_VegsoRendrakas(int startingPlayerID)
    {
        TurnManager.Instance.ApplyVegsoRendrakas(startingPlayerID);
    }

    [PunRPC]
    private void RPC_GameOver()
    {
        TurnManager.Instance.ApplyGameOver();
    }

    [PunRPC]
    private void RPC_SetLastRoundExtra()
    {
        TurnManager.Instance.lastRoundExtra = true;
    }

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

    public void SyncResetTimer()
    {
        photonView.RPC(nameof(RPC_ResetTimer), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ResetTimer()
    {
        remainingTime = thinkingTime;
        timerRunning = true;
        timeUpHandle = false;
        ThinkingTimer.Instance?.ResetTimer();
    }

    [PunRPC]
    private void RPC_TimeUp()
    {
        // Minden kliensen resetelünk
        remainingTime = thinkingTime;
        timerRunning = true;
        timeUpHandle = false;
        ThinkingTimer.Instance?.ResetTimer();

        Player current = TurnManager.Instance?.currentPlayer;
        if (current == null) return;

        // Csak a soron lévõ játékos kliensén fut le ténylegesen
        if (current.PhotonActorNumber != PhotonNetwork.LocalPlayer.ActorNumber) return;

        // TimeIsUp visszaállítása mielõtt ActionHasEnded lefut
        if (ThinkingTimer.Instance != null)
            ThinkingTimer.Instance.TimeIsUp = false;


        if (TurnManager.Instance.isVegsoRendrakas)
            current.OnEndVegsoRendrakasClicked();
        else
            current.ActionHasEnded();
    }
}
