using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

using PhotonPlayer = Photon.Realtime.Player;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine.Localization.Settings;
using Unity.VisualScripting;

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
    /// <summary>
    /// The 3 subpanel of the online game lobby
    /// </summary>
    [Header("Panelek")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private GameObject waitingRoomPanel;

    /// <summary>
    /// The first panel: Setting player's name and connecting to the photon server
    /// </summary>
    [Header("Connect Panel")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    /// <summary>
    /// The second panel: Creating or joining to available online rooms
    /// </summary>
    [Header("Room List Panel")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TMP_InputField createRoomNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button backToMainMenuButton;

    /// <summary>
    /// Third panel: Setting the remaining datas before starting the online game
    /// </summary>
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

    /// <summary>
    /// Backgroung color change for each player
    /// </summary>
    [Header("Szín választók (Waiting Room)")]
    [SerializeField] private TMP_Dropdown localColorDropdown; // csak a saját játékos dropdownja

    /// <summary>
    /// possible background colors
    /// </summary>
    private readonly Color[] palette = new Color[]
    {
        new Color(0.85f, 0.22f, 0.22f),
        new Color(0.22f, 0.45f, 0.85f),
        new Color(0.22f, 0.72f, 0.33f),
        new Color(0.95f, 0.75f, 0.10f),
        new Color(0.70f, 0.25f, 0.80f),
        new Color(0.95f, 0.50f, 0.10f),
    };

    /// <summary>
    /// Every online room on the photon server
    /// </summary>
    private Dictionary<string, GameObject> roomItems = new();

    /// <summary>
    /// Every online player on the photon server
    /// </summary>
    private Dictionary<int, GameObject> playerItems = new();

    /// <summary>
    /// True, when the online lobby ui is initialized and ready
    /// </summary>
    private bool isReady = false;

    //Start is called before the first frame update
    void Start()
    {
        ShowPanel(connectPanel);

        //Eventek
        connectButton.onClick.AddListener(OnConnectClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
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

    /// <summary>
    /// Showing the first panel
    /// Connecting the player to the photon server
    /// </summary>
    private void OnConnectClicked()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name))
        {
            name = "Játékos_" + Random.Range(100, 999);
        }

        if (PhotonNetwork.IsConnected)
        {
            // Már csatlakozva vagyunk, csak navigálunk
            PhotonNetwork.LocalPlayer.NickName = name;
            ShowPanel(roomListPanel);
            return;
        }

        NetworkManager.Instance.ConnectToPhoton(name);
    }

    /// <summary>
    /// Navigate back in the online lobby system
    /// </summary>
    private void OnBackToMainMenu()
    {
        // Visszanavigálás a fõmenübe a te MenuManager-ed ShowMainMenu() hívása
        ShowPanel(connectPanel);
        // MenuManager.Instance?.ShowMainMenu(); // ha van MenuManager-ed, hívd így
    }

    /// <summary>
    /// Showing the second panel
    /// Creating or joining online rooms
    /// </summary>
    /// <param name="roomList"></param>
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

    /// <summary>
    /// Creating a new online room
    /// </summary>
    private void OnCreateRoomClicked()
    {
        string name = createRoomNameInput != null ? createRoomNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name))
        {
            name = "Szoba_" + Random.Range(1000, 9999);
        }
            
        NetworkManager.Instance.CreateRoom(name);
    }

    /// <summary>
    /// Third panel
    /// Joining to an online room and waiting to start the game
    /// </summary>
    public override void OnJoinedRoom()
    {
        ShowPanel(waitingRoomPanel);

        if (roomNameLabel)
        {
            roomNameLabel.text = PhotonNetwork.CurrentRoom.Name;
        }

        bool isHost = PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(isHost);
        readyButton.gameObject.SetActive(!isHost);
        if (thinkingTimeSlider)
        {
            thinkingTimeSlider.gameObject.SetActive(isHost);
        }

        //Új belépéskor adatok frissítése
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("thinkTime", out var t))
        {
            ApplyThinkingTime(System.Convert.ToSingle(t));
        }

        RefreshPlayerList();
    }

    /// <summary>
    /// Refreshes the player list in the waiting room
    /// </summary>
    private void RefreshPlayerList()
    {
        foreach (var item in playerItems.Values)
        {
            Destroy(item);
        }

        playerItems.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var go = Instantiate(playerListItemPrefab, playerListContainer);
            go.GetComponent<PlayerListItem>()?.Setup(player);
            playerItems[player.ActorNumber] = go;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = AllPlayersReady();
        } 
    }

    /// <summary>
    /// Being ready or not. The text changes with it
    /// </summary>
    private void ToggleReady()
    {
        isReady = !isReady;

        var label = readyButton.GetComponentInChildren<TextMeshProUGUI>();

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        
        if (code == "hu")
        {
            if (label) label.text = isReady ? "Mégsem" : "Kész";
        }
        else
        {
            if (label) label.text = isReady ? "Not ready" : "Ready";
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "ready", isReady } }
        );
    }

    /// <summary>
    /// Triggered when the room properties change
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("thinkTime", out var t))
        {
            ApplyThinkingTime(System.Convert.ToSingle(t));
        }
    }

    /// <summary>
    /// Refreshes when changing a player's data (like the background color)
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="changedProps"></param>
    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshPlayerList();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = AllPlayersReady();
        }            
    }

    /// <summary>
    /// True, when every player in the room is ready to play
    /// </summary>
    /// <returns></returns>
    private bool AllPlayersReady()
    {
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            return false;
        }

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient)
            {
                continue;
            }// host nem kell hogy ready legyen

            if (!p.CustomProperties.TryGetValue("ready", out var r) || !(bool)r)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Triggered when the thinking time is changed
    /// </summary>
    /// <param name="value"></param>
    private void OnThinkingTimeChanged(float value)
    {
        float rounded = Mathf.Round(value / 5f) * 5f; // 5 mp-es lépések

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Gondolkodási idõ: {rounded}s";
            }
        }
        else
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Thinking time: {rounded}s";
            }
        }
            
        NetworkManager.Instance.SetThinkingTime(rounded);
    }

    /// <summary>
    /// Starting the online game
    /// </summary>
    private void OnStartClicked()
    {
        NetworkManager.Instance.StartGame(gameSceneName);
    }

    /// <summary>
    /// Leaving an online room
    /// </summary>
    public override void OnLeftRoom()
    {
        isReady = false;
        ShowPanel(roomListPanel);
    }

    /// <summary>
    /// Showing the correct online ui panel
    /// </summary>
    /// <param name="target"></param>
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

    /// <summary>
    /// Applies the thinking time value to the slider and label
    /// </summary>
    /// <param name="value"></param>
    private void ApplyThinkingTime(float value)
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Gondolkodási idõ: {value}s";
            }
        }
        else
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Thinking time: {value}s";
            }
        }

        if (thinkingTimeSlider && thinkingTimeSlider.gameObject.activeSelf)
        {
            thinkingTimeSlider.SetValueWithoutNotify(value);
        }            
    }

    /// <summary>
    /// Creating the possible backgroung color options
    /// </summary>
    private void SetupColorDropdown()
    {
        if (localColorDropdown == null)
        {
            return;
        }

        localColorDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        for (int j = 0; j < palette.Length; j++)
        {
            options.Add(new TMP_Dropdown.OptionData("", CreateColorSprite(palette[j])));
        }

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

    /// <summary>
    /// Triggers when changing the background color
    /// </summary>
    /// <param name="colorIndex"></param>
    private void OnLocalColorChanged(int colorIndex)
    {
        // Szín szinkronizálása Photonon keresztül
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "colorIndex", colorIndex } });
    }

    /// <summary>
    /// Creating the colored sqaures with the possible colors
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private Sprite CreateColorSprite(Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }
}
