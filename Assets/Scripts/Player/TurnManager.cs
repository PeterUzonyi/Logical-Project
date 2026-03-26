using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<Player> allPlayers; // Inspectorban: mind a 4 Player bekötve
    public List<Player> players = new List<Player>(); // csak az aktív játékosok
    public Player currentPlayer { get; private set; } //Soron lévõ játékos
    public int playerCount => players.Count;

    public bool isLastRound = false;
    public Player startedLastRound;
    private bool lastRoundExtra;

    public bool isVegsoRendrakas;

    public bool isGameOver = false;


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
            }
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerName = GameConfig.PlayerNames[i];
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

    void OnDestroy()
    {
        OnlineTurnManager.OnTurnChanged -= OnOnlineTurnChanged;
        OnlineTurnManager.OnTimeUp -= OnOnlineTimeUp;
    }

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

    public void LastRound()
    {
        isLastRound = true;
        lastRoundExtra = false;
        startedLastRound = currentPlayer;
        InfoPanel.Instance.Show("Miután " + currentPlayer.name + " befejezte ezt a kört, utána kezdõdik az utolsó " +
            "kör. \n\nMindenkire még egyszer kerül sor, addig, amíg " + currentPlayer.name + " végre nem hajtotta " +
            "az összes akcióját. \n\nEzután fog következi a Végsõ Rendrakás.");
    }

    public void VegsoRendrakas()
    {
        Debug.Log("Végsõ Rendrakás");
        isVegsoRendrakas = true;
        isLastRound = false;
        lastRoundExtra = false;
        int idx = players.IndexOf(currentPlayer);
        int nextIdx = (idx + 1) % players.Count;
        startedLastRound = players[nextIdx];
        InfoPanel.Instance.Show("Most kezdõdik a Végsõ Rendrakás, mindenkire még egyszer kerül sor. \n\nEbben a körben" +
            " semmilyen akciót nem lehet végrehajtani, csak az elõtted lévõ feladványokat lehet befejezni. Minden " +
            "egyes elem lerakása egy kártyára 1 pontba kerül, amit a kör befejezése után vonunk le. \n\nHa végeztél a " +
            "végsõ rendrakás köröddel, akkor ezt a megfelelõ gomb megnyomásával jelezheted.");
    }

    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over");

        var sortedPlayers = players.OrderByDescending(p => p.PlayerScore)
            .ThenByDescending(p=>p.CompletedPuzzles)
            .ThenByDescending(p=>p.RemainingElements).ToList();
        string result = "";
        int rank = 1;

        /*
        foreach ( Player player in sortedPlayers )
        {
            result += rank + ". " + player.name + ": " + player.PlayerScore + " pont\n";
            rank++;
        }

        InfoPanel.Instance.Show("A játék véget ért, íme a végsõ állás: \n\n" + result);
        //SceneManager.LoadScene("StartGameScene");
        */

        int i = 0;

        while (i < sortedPlayers.Count)
        {
            // Megkeressük meddig tart az egyforma csoport
            int j = i;
            while (j < sortedPlayers.Count
                && sortedPlayers[j].PlayerScore == sortedPlayers[i].PlayerScore
                && sortedPlayers[j].CompletedPuzzles == sortedPlayers[i].CompletedPuzzles
                && sortedPlayers[j].RemainingElements == sortedPlayers[i].RemainingElements)
            {
                j++;
            }

            // i..j-1 indexig mindenki azonos helyezésen van
            bool isTie = (j - i) > 1;
            for (int k = i; k < j; k++)
            {
                Player p = sortedPlayers[k];
                string rankStr = isTie ? rank + ". (döntetlen)" : rank + ".";
                result += $"{rankStr} {p.name}: {p.PlayerScore} pont" +
                          $" | Feladványok: {p.CompletedPuzzles}" +
                          $" | Alkatrészek: {p.RemainingElements}\n";
            }

            if (isTie)
            {
                result += "Az érintettek osztoznak a gyõzelemben. Gratulálunk!\n";
            }

            rank += (j - i); // pl. ha 2 játékos osztja az 1. helyet, a következõ a 3.
            i = j;
        }

        InfoPanel.Instance.Show("A játék véget ért, íme a végsõ állás: \n\n" + result);
    }

    private void OnOnlineTurnChanged(int actorNumber)
    {
        // Az actorNumber alapján döntjük el ki a currentPlayer
        var next=players.FirstOrDefault(p => p.PhotonActorNumber == actorNumber);
        if (next == null)
        {
            Debug.LogWarning("Nem található játékos ezzel az ActorNumber-rel: " + actorNumber);
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
    private void OnOnlineTimeUp()
    {
        // Ha lejárt az idõ, a játékos körét befejezettnek tekintjük
        // (az OnlineTurnManager MasterClient-en már léptette a kört)
        Debug.Log("Idõ lejárt, kör kényszerített vége.");
    }
}
