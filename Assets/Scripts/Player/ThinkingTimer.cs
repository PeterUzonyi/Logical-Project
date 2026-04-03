using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;

/// <summary>
/// Gondolkodási idõzítõ megjelenítõ és vezérlõ.
/// Csatold a ThinkingTime GameObject-hez, amelyen TMP_Text is van.
///
/// Online módban az OnlineTurnManager.OnTimerTick eseményre iratkozik fel
/// és az ott futó idõzítõt jeleníti meg.
/// Lokális módban saját maga futtatja a visszaszámlálót.
/// 
/// Reset-elés: hívd a ResetTimer()-t minden akció végén (ActionHasEnded-bõl).
/// </summary>
public class ThinkingTimer : MonoBehaviour
{
    public static ThinkingTimer Instance { get; private set; }

    /// <summary>
    /// Displays the time
    /// </summary>
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    /// <summary>
    /// Local time management
    /// </summary>
    [Header("Beállítások")]
    [Tooltip("Csak lokális módban használt. Online-ban a szoba CustomProperty-bõl olvas.")]
    [SerializeField] private float localThinkingTime = 30f;
    [SerializeField] public bool TimeIsUp;

    // Lokális mód belsõ állapot
    private float remainingTime;
    private bool timerRunning = false;
    private bool timeUpHandled = false;

    //Called when the script is loaded
    void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject);
            return; 
        }
        Instance = this;

        if (timerText == null)
        {
            timerText = GetComponentInChildren<TMP_Text>();
        }

        // Gondolkodási idõ betöltése GameConfig-ból (mindkét módban)
        localThinkingTime = GameConfig.ThinkingTime;

        TimeIsUp = false;
    }

    //Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Online: feliratkozás az OnlineTurnManager tick eseményére
            OnlineTurnManager.OnTimerTick += OnOnlineTimerTick;
            OnlineTurnManager.OnTurnChanged += OnOnlineTurnChanged;
        }
        else
        {
            // Lokális: saját idõzítõ indul a kör kezdetekor
            // A TurnManager hívja a StartTimer()-t
        }
    }

    /// <summary>
    /// Unsubscribes from the OnlineTurnManager events
    /// </summary>
    void OnDestroy()
    {
        OnlineTurnManager.OnTimerTick -= OnOnlineTimerTick;
        OnlineTurnManager.OnTurnChanged -= OnOnlineTurnChanged;
    }

    //Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        } // Online-ban az event kezeli

        if (!timerRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        UpdateDisplay(remainingTime);

        if (remainingTime <= 0f && !timeUpHandled)
        {
            timeUpHandled = true;
            timerRunning = false;
            HandleTimeUp();
        }
    }

    /// <summary>
    /// Starts or restarts the timer.
    /// Local mode. calls the TurnManager for next player's turn
    /// </summary>
    public void StartTimer()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        }// Online-ban az OnlineTurnManager vezérel

        remainingTime = localThinkingTime;
        timeUpHandled = false;
        timerRunning = true;
        TimeIsUp = false;
        UpdateDisplay(remainingTime);
    }

    /// <summary>
    /// Resets the timer, called after action has ended
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = localThinkingTime;
        timeUpHandled = false;
        timerRunning = true;
        TimeIsUp = false;
        UpdateDisplay(remainingTime);
    }

    /// <summary>
    /// Stops the timer, called at game over
    /// </summary>
    public void StopTimer()
    {
        timerRunning = false;
    }

/// <summary>
/// Online mode. The timer is ticking
/// </summary>
/// <param name="time"></param>
    private void OnOnlineTimerTick(float time)
    {
        UpdateDisplay(time);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actorNumber"></param>
    private void OnOnlineTurnChanged(int actorNumber)
    {
        // Az OnlineTurnManager.RPC_SetTurn már reseteli az idõt,
        // az OnTimerTick majd frissíti a UI-t automatikusan.
        // Ha szükséges, itt vizuálisan is jelezhetjük a körváltást.
    }

    // --- Belsõ segédmetódusok ---

    /// <summary>
    /// Online mode. Displays the time for every player
    /// </summary>
    /// <param name="time"></param>
    private void UpdateDisplay(float time)
    {
        if (timerText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(time);
        timerText.text = seconds.ToString();

        // Szín figyelmeztetés: piros ha 10 mp alatt
        timerText.color = seconds <= 10 ? Color.red : Color.black;
    }

    /// <summary>
    /// When the time is up, then the action is finished. 
    /// If this was the 3-rd action, then next player's turn starts.
    /// </summary>
    private void HandleTimeUp()
    {
        Debug.Log("[ThinkingTimer] Idõ lejárt, kör kényszer-befejezése.");
        TimeIsUp = true;

        Player current = TurnManager.Instance?.currentPlayer;
        if (current == null)
        {
            return;
        }

        if (TurnManager.Instance.isVegsoRendrakas)
        {
            // Végsõ rendrakás: pontlevonással fejezi be
            current.OnEndVegsoRendrakasClicked();
        }
        else
        {
            // Normál kör: aktuális akciót megszakítjuk, kör véget ér
            // Ha épp akció kiválasztásra vár, sima körváltás
            current.ActionHasEnded();
        }
    }
}