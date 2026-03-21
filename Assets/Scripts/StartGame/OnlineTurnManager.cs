using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
            StartCoroutine(StartFirstTurn());
            /*
            photonView.RPC(nameof(RPC_SetTurn),
                RpcTarget.All,
                PhotonNetwork.MasterClient.ActorNumber);
            */
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

        if (remainingTime <= 0f)
        {
            timerRunning = false;
            // Csak a MasterClient lépteti tovább ha lejár az idõ
            if (PhotonNetwork.IsMasterClient)
                NextTurn();
            OnTimeUp?.Invoke();
        }
    }

    // Publikus API

    /// <summary>
    /// Igaz ha a helyi játékos van soron.
    /// A GameManager-edben ezzel ellenõrizd hogy engedélyezed e a lépést.
    /// </summary>
    public bool IsMyTurn =>
        PhotonNetwork.LocalPlayer.ActorNumber == activeActorNumber;

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

    [PunRPC]
    private void RPC_SetTurn(int actorNumber)
    {
        // Ez minden kliensen fut egyszerre
        activeActorNumber = actorNumber;
        remainingTime = thinkingTime;
        timerRunning = true;

        OnTurnChanged?.Invoke(actorNumber);
    }
}
