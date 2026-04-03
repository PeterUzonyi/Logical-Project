using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

/// <summary>
/// Controlls the turns, last round, Finishing Touches (Végsõ rendrakás) and Game Over
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// Every players prefab (maximum: 4 players)
    /// </summary>
    [SerializeField] private List<Player> allPlayers; // Inspectorban: mind a 4 Player bekötve

    /// <summary>
    /// Active players (minimum: 2, maximum: 4 players)
    /// </summary>
    public List<Player> players = new List<Player>(); // csak az aktív játékosok

    /// <summary>
    /// The player, whose turn is this turn
    /// </summary>
    public Player currentPlayer { get; private set; } //Soron lévõ játékos

    /// <summary>
    /// Number of active players
    /// </summary>
    public int playerCount => players.Count;

    /// <summary>
    /// Last round
    /// </summary>
    public bool isLastRound = false;
    public Player startedLastRound;
    public bool lastRoundExtra;

    /// <summary>
    /// Final Touches (Végsõ rendrakás)
    /// </summary>
    public bool isVegsoRendrakas;

    /// <summary>
    /// Game Over
    /// </summary>
    public bool isGameOver = false;

    //Called when the script is loaded
    void Awake()
    {
        Instance = this;
        isLastRound = false;
        isGameOver = false;
        lastRoundExtra = false;
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

    //Start is called before the first frame update
    void Start()
    {
        // Online módban a helyi játékos legyen players[0]
        if (PhotonNetwork.IsConnected)
        {
            // ActorNumber beállítása a Photon sorrendnek megfelelõen
            var photonPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.IsMasterClient ? 0 : 1).ToList();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerID = i + 1; // fix 1-4
                players[i].PhotonActorNumber = photonPlayers[i].ActorNumber; // Photon szám
                players[i].PlayerName = photonPlayers[i].NickName;
                players[i].ApplyColor();
            }
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerName = GameConfig.PlayerNames[i];
                players[i].ApplyColor();
            }
        }

        currentPlayer = players[0];
        currentPlayer.OpenCommonReserve();

        if (PhotonNetwork.IsConnected)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].BlockingPanel.SetActive(true);
            }
        }
        else
        {
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
        }

        if (PhotonNetwork.IsConnected && OnlineTurnManager.Instance != null)
        {
            OnlineTurnManager.OnTurnChanged += OnOnlineTurnChanged;
            OnlineTurnManager.OnTimeUp += OnOnlineTimeUp;
        }
    }

    /// <summary>
    /// Unsubscribes from the OnlineTurnManager events
    /// </summary>
    void OnDestroy()
    {
        OnlineTurnManager.OnTurnChanged -= OnOnlineTurnChanged;
        OnlineTurnManager.OnTimeUp -= OnOnlineTimeUp;
    }

    /// <summary>
    /// The next player's turn starts
    /// </summary>
    public void EndTurn()
    {
        // Következõ játékos körbe forgatva
        int idx = players.IndexOf(currentPlayer);
        int nextIdx = (idx + 1) % players.Count;
        Player nextPlayer = players[nextIdx];

        if (isLastRound)
        {
            if (currentPlayer == startedLastRound && lastRoundExtra)
            {
                VegsoRendrakas();
            }
            else if (currentPlayer == startedLastRound && !lastRoundExtra)
            {
                lastRoundExtra = true;
            }
        }
        else if (isVegsoRendrakas && nextPlayer == startedLastRound)
        {
            GameOver();
            return;
        }

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
    }

    /// <summary>
    /// Last round before the Final Touches (Végsõ Rendrakás)
    /// </summary>
    public void LastRound()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Online mód
            OnlineTurnManager.Instance.SyncLastRound(currentPlayer.PlayerID);
        }
        else
        {
            //Lokális mód
            ApplyLastRound(currentPlayer.PlayerID);
        }
    }

    /// <summary>
    /// Last round before the Final Touches (Végsõ Rendrakás)
    /// </summary>
    /// <param name="playerID"></param>
    public void ApplyLastRound(int playerID)
    {
        Player player = players.FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null)
        {
            return;
        }
        isLastRound = true;
        lastRoundExtra = false;
        startedLastRound = player;

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            InfoPanel.Instance.Show(
            "Miután " + player.PlayerName + " befejezte ezt a kört, utána kezdõdik az utolsó kör." +
            "\n\nMindenkire még egyszer kerül sor, addig, amíg " + player.PlayerName +
            " végre nem hajtotta az összes akcióját.\n\nEzután fog következni a Végsõ Rendrakás.");
        }
        else
        {
            InfoPanel.Instance.Show(
            "After " + player.PlayerName + " finished this round, then the last round begins." +
            "\n\nEveryone gets another round, until " + player.PlayerName +
            " has completed all of its actions.\n\nEzután fog következni a Végsõ Rendrakás.");
        }

        
    }

    /// <summary>
    /// Final Touches (Végsõ Rendrakás)
    /// </summary>
    public void VegsoRendrakas()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Online mód
            int nextIdx = (players.IndexOf(currentPlayer) + 1) % players.Count;
            OnlineTurnManager.Instance.SyncVegsoRendrakas(players[nextIdx].PlayerID);
        }
        else
        {
            //Lokális mód
            int nextIdx = (players.IndexOf(currentPlayer) + 1) % players.Count;
            ApplyVegsoRendrakas(players[nextIdx].PlayerID);

            Debug.Log("Végsõ Rendrakás");
        }
    }

    /// <summary>
    /// Final Touches (Végsõ Rendrakás)
    /// </summary>
    /// <param name="startingPlayerID"></param>
    public void ApplyVegsoRendrakas(int startingPlayerID)
    {
        Player startingPlayer = players.FirstOrDefault(p => p.PlayerID == startingPlayerID);
        if (startingPlayer == null)
        {
            return;
        }
        isVegsoRendrakas = true;
        isLastRound = false;
        lastRoundExtra = false;
        startedLastRound = startingPlayer;

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            InfoPanel.Instance.Show(
            "Most kezdõdik a Végsõ Rendrakás, mindenkire még egyszer kerül sor.\n\nEbben a körben " +
            "semmilyen akciót nem lehet végrehajtani, csak az elõtted lévõ feladványokat lehet befejezni. " +
            "Minden egyes elem lerakása egy kártyára 1 pontba kerül, amit a kör befejezése után vonunk le." +
            "\n\nHa végeztél a Végsõ Rendrakás köröddel, azt a megfelelõ gomb megnyomásával jelezd.");
        }
        else
        {
            InfoPanel.Instance.Show(
            "Now it is time for the Final Touches, everyone gets a last round.\n\nDuring this round " +
            "noone can do any actions, except placing down elements onto the puzzles in front of you. " +
            "For each element placed down you lose 1 point, which is deducted after you finished this round." +
            "\n\nIf you are finished with the Final Touches, indicate this by clicking the appropriate button.");
        }        
    }

    /// <summary>
    /// The Game is Over
    /// </summary>
    public void GameOver()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Online mód
            OnlineTurnManager.Instance.SyncGameOver();
        }
        else
        {
            //Lokális mód
            ApplyGameOver();
        }
    }

    /// <summary>
    /// The Game is Over and showing the final standings
    /// </summary>
    public void ApplyGameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over");

        var sortedPlayers = players
            .OrderByDescending(p => p.PlayerScore)
            .ThenByDescending(p => p.CompletedPuzzles)
            .ThenByDescending(p => p.RemainingElements)
            .ToList();

        string result = "";
        int rank = 1;
        int i = 0;

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        while (i < sortedPlayers.Count)
        {
            int j = i;
            while (j < sortedPlayers.Count
                && sortedPlayers[j].PlayerScore == sortedPlayers[i].PlayerScore
                && sortedPlayers[j].CompletedPuzzles == sortedPlayers[i].CompletedPuzzles
                && sortedPlayers[j].RemainingElements == sortedPlayers[i].RemainingElements)
            {
                j++;
            }

            bool isTie = (j - i) > 1;
            for (int k = i; k < j; k++)
            {
                Player p = sortedPlayers[k];

                if (code == "hu")
                {
                    string rankStr = isTie ? rank + ". (döntetlen)" : rank + ".";
                    result += $"{rankStr} {p.PlayerName}: {p.PlayerScore} pont" +
                              $" | Feladványok: {p.CompletedPuzzles}" +
                              $" | Alkatrészek: {p.RemainingElements}\n";
                }
                else
                {
                    string rankStr = isTie ? rank + ". (tie)" : rank + ".";
                    result += $"{rankStr} {p.PlayerName}: {p.PlayerScore} score" +
                              $" | puzzles: {p.CompletedPuzzles}" +
                              $" | elements: {p.RemainingElements}\n";
                }

                
            }
            if (isTie)
            {
                if (code == "hu")
                {
                    result += "Az érintettek osztoznak a gyõzelemben. Gratulálunk mindenkinek!\n";
                }
                else
                {
                    result += "All tied players share the victory. You are all awesome!\n";
                }
            }

            rank += (j - i);
            i = j;
        }

        if (code == "hu")
        {
            InfoPanel.Instance.Show("A játék véget ért, íme a végsõ állás:\n\n" + result);
        }
        else
        {
            InfoPanel.Instance.Show("The game is over, here are the final standings:\n\n" + result);
        }
    }

    /// <summary>
    /// Online mode. Called when the online turn changes, the UI updates automatically via OnTimerTick
    /// </summary>
    /// <param name="actorNumber"></param>
    private void OnOnlineTurnChanged(int actorNumber)
    {
        // Az actorNumber alapján döntjük el ki a currentPlayer
        var next = players.FirstOrDefault(p => p.PhotonActorNumber == actorNumber);
        if (next == null)
        {
            Debug.LogWarning("Nem található játékos ezzel az ActorNumber-rel: " + actorNumber);
            return;
        }

        // Az állapotgép logikát csak a MasterClient futtatja
        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoundState(next);
        }

        if (isGameOver)
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
    }

    /// <summary>
    /// Online mode. Runs only on the Master Client. Checks the conditions before starting the next player's turn
    /// </summary>
    private void CheckRoundState(Player nextPlayer)
    {
        if (isLastRound)
        {
            if (currentPlayer == startedLastRound && lastRoundExtra)
            {
                OnlineTurnManager.Instance.SyncVegsoRendrakas(nextPlayer.PlayerID);
                return;
            }
            else if (currentPlayer == startedLastRound && !lastRoundExtra)
            {
                OnlineTurnManager.Instance.SyncLastRoundExtra();
            }
        }
        else if (isVegsoRendrakas && nextPlayer == startedLastRound)
        {
            OnlineTurnManager.Instance.SyncGameOver();
            return;
        }
    }

    /// <summary>
    /// Online mode. When the timer hits 0
    /// </summary>
    private void OnOnlineTimeUp()
    {
        // Ha lejárt az idõ, a játékos körét befejezettnek tekintjük
        // (az OnlineTurnManager MasterClient-en már léptette a kört)
        Debug.Log("Idõ lejárt, kör kényszerített vége.");
    }
}
