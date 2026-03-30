using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// A Lobby UI kezelõje. Ugyanabban a menü jelenetben él mint a fõmenü.
/// 
/// Panel struktúra a Canvas-on:
///   LobbyCanvas
///     - ConnectPanel       - névbevitel + csatlakozás gomb
///     - RoomListPanel      - szobák listája + létrehozás
///     - WaitingRoomPanel   - várakozó szoba (játékosok + ready)
/// </summary>
public class LobbyUI : MonoBehaviourPunCallbacks
{
    [Header("Panelek")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private GameObject waitingRoomPanel;

    [Header("Connect Panel")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Room List Panel")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TMP_InputField createRoomNameInput;
    [SerializeField] private Button createRoomButton;
    //[SerializeField] private Button joinRandomButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Waiting Room Panel")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;        // csak host látja
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private TextMeshProUGUI roomNameLabel;
    [SerializeField] private Slider thinkingTimeSlider; // csak host kezeli
    [SerializeField] private TextMeshProUGUI thinkingTimeLabel;

    [Header("Játék jelenet neve")]
    [SerializeField] private string gameSceneName = "GameScene"; // a te meglévõ scene neved

    [Header("Szín választók (Waiting Room)")]
    [SerializeField] private TMP_Dropdown localColorDropdown; // csak a saját játékos dropdownja

    private readonly Color[] palette = new Color[]
    {
        new Color(0.85f, 0.22f, 0.22f),
        new Color(0.22f, 0.45f, 0.85f),
        new Color(0.22f, 0.72f, 0.33f),
        new Color(0.95f, 0.75f, 0.10f),
        new Color(0.70f, 0.25f, 0.80f),
        new Color(0.95f, 0.50f, 0.10f),
    };

    // Belsõ állapot 
    private Dictionary<string, GameObject> roomItems = new();
    private Dictionary<int, GameObject> playerItems = new();
    private bool isReady = false;

    // Lifecycle
    void Start()
    {
        ShowPanel(connectPanel);

        connectButton.onClick.AddListener(OnConnectClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        //joinRandomButton.onClick.AddListener(() => NetworkManager.Instance.JoinRandomRoom());
        leaveRoomButton.onClick.AddListener(() => NetworkManager.Instance.LeaveRoom());
        backToMainMenuButton?.onClick.AddListener(OnBackToMainMenu);
        readyButton.onClick.AddListener(ToggleReady);
        startButton.onClick.AddListener(OnStartClicked);
        thinkingTimeSlider?.onValueChanged.AddListener(OnThinkingTimeChanged);
        SetupColorDropdown();

        // Feliratkozás NetworkManager eseményekre
        NetworkManager.Instance.OnStatusChanged += msg => { if (statusLabel) statusLabel.text = msg; };
        NetworkManager.Instance.OnLobbyJoined += () => ShowPanel(roomListPanel);
        NetworkManager.Instance.OnRoomJoined += OnJoinedRoom;
        NetworkManager.Instance.OnRoomLeft += OnLeftRoom;
        NetworkManager.Instance.OnOtherPlayerEntered += _ => RefreshPlayerList();
        NetworkManager.Instance.OnOtherPlayerLeft += _ => RefreshPlayerList();
    }

    //  Connect Panel 
    private void OnConnectClicked()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name))
            name = "Játékos_" + Random.Range(100, 999);
        NetworkManager.Instance.ConnectToPhoton(name);
    }

    private void OnBackToMainMenu()
    {
        // Visszanavigálás a fõmenübe a te MenuManager-ed ShowMainMenu() hívása
        ShowPanel(connectPanel);
        // MenuManager.Instance?.ShowMainMenu(); // ha van MenuManager-ed, hívd így
    }

    // Room List Panel
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (roomItems.TryGetValue(info.Name, out var old))
                {
                    Destroy(old);
                    roomItems.Remove(info.Name);
                }
                continue;
            }

            if (!roomItems.ContainsKey(info.Name))
            {
                var go = Instantiate(roomListItemPrefab, roomListContainer);
                roomItems[info.Name] = go;

                var item = go.GetComponent<RoomListItem>();
                item?.Setup(info, () => NetworkManager.Instance.JoinRoom(info.Name));
            }
        }
    }

    private void OnCreateRoomClicked()
    {
        string name = createRoomNameInput != null ? createRoomNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name))
            name = "Szoba_" + Random.Range(1000, 9999);
        NetworkManager.Instance.CreateRoom(name);
    }

    // Waiting Room Panel
    public override void OnJoinedRoom()
    {
        ShowPanel(waitingRoomPanel);

        if (roomNameLabel)
            roomNameLabel.text = PhotonNetwork.CurrentRoom.Name;

        bool isHost = PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(isHost);
        if (thinkingTimeSlider) thinkingTimeSlider.gameObject.SetActive(isHost);

        //Új belépéskor adatok frissítése
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("thinkTime", out var t))
            ApplyThinkingTime(System.Convert.ToSingle(t));

        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        foreach (var item in playerItems.Values) Destroy(item);
        playerItems.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var go = Instantiate(playerListItemPrefab, playerListContainer);
            go.GetComponent<PlayerListItem>()?.Setup(player);
            playerItems[player.ActorNumber] = go;
        }

        if (PhotonNetwork.IsMasterClient)
            startButton.interactable = AllPlayersReady();
    }

    private void ToggleReady()
    {
        isReady = !isReady;

        var label = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = isReady ? "Mégsem" : "Kész";

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "ready", isReady } }
        );
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("thinkTime", out var t))
            ApplyThinkingTime(System.Convert.ToSingle(t));
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer,
        ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshPlayerList();
        if (PhotonNetwork.IsMasterClient)
            startButton.interactable = AllPlayersReady();
    }

    private bool AllPlayersReady()
    {
        if (PhotonNetwork.PlayerList.Length < 2) return false;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient) continue; // host nem kell hogy ready legyen
            if (!p.CustomProperties.TryGetValue("ready", out var r) || !(bool)r)
                return false;
        }
        return true;
    }

    private void OnThinkingTimeChanged(float value)
    {
        float rounded = Mathf.Round(value / 5f) * 5f; // 5 mp-es lépések
        if (thinkingTimeLabel)
            thinkingTimeLabel.text = $"Gondolkodási idõ: {rounded}s";
        NetworkManager.Instance.SetThinkingTime(rounded);
    }

    private void OnStartClicked()
    {
        NetworkManager.Instance.StartGame(gameSceneName);
    }

    public override void OnLeftRoom()
    {
        isReady = false;
        ShowPanel(roomListPanel);
    }

    // Helper 
    private void ShowPanel(GameObject target)
    {
        if (connectPanel)
        {
            connectPanel.SetActive(target == connectPanel);
        }
        if (roomListPanel)
        {
            roomListPanel.SetActive(target == roomListPanel);
        }
        if (waitingRoomPanel)
        {
            waitingRoomPanel.SetActive(target == waitingRoomPanel);
        }
    }

    //Gondolkodási idõ lekérése
    private void ApplyThinkingTime(float value)
    {
        if (thinkingTimeLabel)
            thinkingTimeLabel.text = $"Gondolkodási idõ: {value}s";

        if (thinkingTimeSlider && thinkingTimeSlider.gameObject.activeSelf)
            thinkingTimeSlider.SetValueWithoutNotify(value);
    }

    private void SetupColorDropdown()
    {
        if (localColorDropdown == null) return;

        localColorDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        for (int j = 0; j < palette.Length; j++)
            options.Add(new TMP_Dropdown.OptionData("", CreateColorSprite(palette[j])));

        localColorDropdown.AddOptions(options);

        int savedIndex = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("colorIndex", out var ci))
        {
            savedIndex = (int)ci;
        }

        localColorDropdown.SetValueWithoutNotify(savedIndex);
        localColorDropdown.RefreshShownValue();
        localColorDropdown.onValueChanged.AddListener(OnLocalColorChanged);

        OnLocalColorChanged(savedIndex);
    }

    private void OnLocalColorChanged(int colorIndex)
    {
        // Szín szinkronizálása Photonon keresztül
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "colorIndex", colorIndex } });
    }

    private Sprite CreateColorSprite(Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }
}
