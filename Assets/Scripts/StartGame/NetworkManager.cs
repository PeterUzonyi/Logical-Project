using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonPlayer = Photon.Realtime.Player;
using System.Linq;
using UnityEngine.Localization.Settings;

/// <summary>
/// Connects the players and the rooms to the photon server. This provides the online mode
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Beállítások")]
    public string gameVersion = "1.0";
    public byte maxPlayersPerRoom = 4;

    /// <summary>
    /// Events
    /// </summary>
    public event System.Action<string> OnStatusChanged;
    public event System.Action OnLobbyJoined;
    public event System.Action OnRoomJoined;
    public event System.Action OnRoomLeft;
    public event System.Action<PhotonPlayer> OnOtherPlayerEntered;
    public event System.Action<PhotonPlayer> OnOtherPlayerLeft;

    //Called when the script is loaded
    void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // MasterClient tölti be a játék jelenetet mindenkinél
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// Connecting the player to the photon server
    /// </summary>
    /// <param name="playerName"></param>
    public void ConnectToPhoton(string playerName)
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        if (PhotonNetwork.IsConnected)
        {
            if (code == "hu")
            {
                OnStatusChanged?.Invoke("Már csatlakozva.");
            }
            else
            {
                OnStatusChanged?.Invoke("Already connected.");
            }
            
            PhotonNetwork.JoinLobby();
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerName;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Csatlakozás...");
        }
        else
        {
            OnStatusChanged?.Invoke("Connecting...");
        }
        
    }

    /// <summary>
    /// Creating an online room
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="maxPlayers"></param>
    public void CreateRoom(string roomName, byte maxPlayers = 0)
    {
        if (maxPlayers == 0) maxPlayers = maxPlayersPerRoom;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "thinkTime", 30f }
            },
            CustomRoomPropertiesForLobby = new[] { "thinkTime" }
        };

        PhotonNetwork.CreateRoom(roomName, options);
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Szoba létrehozása...");
        }
        else
        {
            OnStatusChanged?.Invoke("Creating room...");
        }        
    }

    /// <summary>
    /// Joining to an online room
    /// </summary>
    /// <param name="roomName"></param>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Csatlakozás a szobához...");
        }
        else
        {
            OnStatusChanged?.Invoke("Connecting to the room...");
        }        
    }

    /// <summary>
    /// Leaving an online room
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Setting the thinking time
    /// </summary>
    /// <param name="seconds"></param>
    public void SetThinkingTime(float seconds)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "thinkTime", seconds } });
    }

    /// <summary>
    /// The host can start the game
    /// </summary>
    /// <param name="sceneName"></param>
    public void StartGame(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;  // ne tudjon senki csatlakozni
        PhotonNetwork.LoadLevel(sceneName);         // mindenkinél betölti a jelenetet
    }

    /// <summary>
    /// Event overrides
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Csatlakozva!");
        }
        else
        {
            OnStatusChanged?.Invoke("Connected!");
        }
    }


    public override void OnJoinedLobby()
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Lobby kész.");
        }
        else
        {
            OnStatusChanged?.Invoke("Lobby is ready.");
        }
        
        OnLobbyJoined?.Invoke();
    }

    public override void OnJoinedRoom() => OnRoomJoined?.Invoke();

    /// <summary>
    /// If the player lest the room becouse of the previous game ended, the they get back to the main menu
    /// </summary>
    public override void OnLeftRoom()
    {
        OnRoomLeft?.Invoke();

        // Ha GameOver után léptünk ki, töltsük be a menü jelenetet
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "StartGameScene")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartGameScene");
        }
    } 

    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        => OnOtherPlayerEntered?.Invoke(newPlayer);

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        => OnOtherPlayerLeft?.Invoke(otherPlayer);

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke("Nincs szabad szoba, új létrehozása...");
        }
        else
        {
            OnStatusChanged?.Invoke("There are no empty rooms, making a new room...");
        }
        
        CreateRoom("Szoba_" + Random.Range(1000, 9999));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            OnStatusChanged?.Invoke($"Kapcsolat megszakadt: {cause}");
        }
        else
        {
            OnStatusChanged?.Invoke($"Connection lost: {cause}");
        }
    }
}
