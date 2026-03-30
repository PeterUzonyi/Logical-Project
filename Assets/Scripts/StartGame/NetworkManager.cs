using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PhotonPlayer = Photon.Realtime.Player;
using System.Linq;

/// <summary>
/// Photon kapcsolat és szoba kezelõ (Singleton, DontDestroyOnLoad).
/// Csatold egy üres GameObject-hez a menü jelenetben.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Beállítások")]
    public string gameVersion = "1.0";
    public byte maxPlayersPerRoom = 4;

    // Események a LobbyUI feliratkozik rájuk
    public event System.Action<string> OnStatusChanged;
    public event System.Action OnLobbyJoined;
    public event System.Action OnRoomJoined;
    public event System.Action OnRoomLeft;
    public event System.Action<PhotonPlayer> OnOtherPlayerEntered;
    public event System.Action<PhotonPlayer> OnOtherPlayerLeft;

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

    // Csatlakozás 
    public void ConnectToPhoton(string playerName)
    {
        if (PhotonNetwork.IsConnected)
        {
            OnStatusChanged?.Invoke("Már csatlakozva.");
            PhotonNetwork.JoinLobby();
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerName;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        OnStatusChanged?.Invoke("Csatlakozás...");
    }

    // Szoba mûveletek
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
        OnStatusChanged?.Invoke("Szoba létrehozása...");
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        OnStatusChanged?.Invoke("Csatlakozás a szobához...");
    }

    //Ez nem kell, nem is használom
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        OnStatusChanged?.Invoke("Véletlenszerû szoba keresése...");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetThinkingTime(float seconds)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "thinkTime", seconds } }
        );
    }

    // Játék indítása (csak Host/MasterClient hívhatja) 
    public void StartGame(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;  // ne tudjon senki csatlakozni
        PhotonNetwork.LoadLevel(sceneName);         // mindenkinél betölti a jelenetet
    }

    // Photon visszahívások 
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        OnStatusChanged?.Invoke("Csatlakozva!");
    }

    public override void OnJoinedLobby()
    {
        OnStatusChanged?.Invoke("Lobby kész.");
        OnLobbyJoined?.Invoke();
    }

    [PunRPC]
    public void RPC_PlaceElementOnGrid(int playerID, int slotIndex, int[] squareIndexes, int itemID, float r, float g, float b, int totalSquares)
    {
        Player player = TurnManager.Instance.players
            .FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null) return;

        CardLoader cardLoader = player.GetCardLoaderBySlot(slotIndex);
        if (cardLoader == null) return;

        MyGrid grid = cardLoader.gridScript;
        if (grid == null) return;

        Color elementColor = new Color(r, g, b);

        foreach (int idx in squareIndexes)
        {
            GridSquare sq = grid.GetGridSquare(idx);
            if (sq != null)
                sq.ActivateSquareSync(elementColor, itemID, totalSquares);
        }

        // Mennyiség csökkentése MINDEN kliensen a játékos saját inventoryján
        InventoryItem item = player.inventoryManager.GetItemById(itemID);
        if (item != null)
        {
            item.quantity--;
            item.RefreshCount();
        }
    }

    [PunRPC]
    public void RPC_CardCompleted(int playerID, int slotIndex, int[] elements, int score, int rewardElement)
    {
        Player ownerPlayer = TurnManager.Instance.players
            .FirstOrDefault(p => p.PlayerID == playerID);
        if (ownerPlayer == null) return;

        InventoryManager ownerInventory = ownerPlayer.inventoryManager;

        //A teljesítésért járó elem
        elements[rewardElement]++;
        CommonReserve.Instance.TakeFromInventory(rewardElement, 1);

        for (int i = 0; i < elements.Length; i++)
        {
            InventoryItem item = ownerInventory.GetItemById(i);
            if (item != null)
            {
                item.quantity += elements[i];
                item.RefreshCount();
            }
        }

        ownerPlayer.RefreshScore(score);
        ownerPlayer.RemoveCard(slotIndex);
    }

    public override void OnJoinedRoom() => OnRoomJoined?.Invoke();
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
        OnStatusChanged?.Invoke("Nincs szabad szoba, új létrehozása...");
        CreateRoom("Szoba_" + Random.Range(1000, 9999));
    }

    public override void OnDisconnected(DisconnectCause cause)
        => OnStatusChanged?.Invoke($"Kapcsolat megszakadt: {cause}");
}
