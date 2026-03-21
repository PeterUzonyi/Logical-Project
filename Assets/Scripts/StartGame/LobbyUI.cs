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
        {
            float value = System.Convert.ToSingle(t);
            if (thinkingTimeLabel)
                thinkingTimeLabel.text = $"Gondolkodási idõ: {value}s";

            // Ha a slider látható (csak hostnál), frissítsd azt is
            if (thinkingTimeSlider && thinkingTimeSlider.gameObject.activeSelf)
                thinkingTimeSlider.SetValueWithoutNotify(value);
        }
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
}
