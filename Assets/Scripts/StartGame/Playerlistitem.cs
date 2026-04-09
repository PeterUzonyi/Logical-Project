using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// Egy játékos sor a várakozó szobában.
/// 
/// Prefab struktúra:
///   PlayerListItem
///     - PlayerNameLabel (TextMeshProUGUI)
///     - ReadyLabel      (TextMeshProUGUI)   "Kész" / "Vár..."
///     - HostBadge       (GameObject)        csak host-nál látható
/// </summary>
public class PlayerListItem : MonoBehaviour
{
    /// <summary>
    /// Player's name
    /// </summary>
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private TextMeshProUGUI readyLabel;
    [SerializeField] private GameObject hostBadge;
    [SerializeField] private Image colorImage;

    /// <summary>
    /// Background color options
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
    /// Shows the data of every players in the room
    /// </summary>
    /// <param name="player"></param>
    public void Setup(PhotonPlayer player)
    {
        if (playerNameLabel)
        {
            playerNameLabel.text = player.NickName;
        }

        // Ready állapot olvasása
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("ready", out var r))
        {
            isReady = (bool)r;
        }

        // Host automatikusan "kész"-nek számít
        if (player.IsMasterClient)
        {
            isReady = true;
        }

        if (readyLabel)
        {
            readyLabel.text = isReady ? "Kész" : "Vár...";
            readyLabel.color = isReady
                ? new Color(0.2f, 0.8f, 0.4f)   // zöld
                : new Color(0.8f, 0.8f, 0.8f);  // szürke
        }

        if (hostBadge)
        {
            hostBadge.SetActive(player.IsMasterClient);
        }
            

        // Szín megjelenítése (pl. egy Image komponensen)
        if (colorImage != null)
        {
            int colorIndex = 0;
            if (player.CustomProperties.TryGetValue("colorIndex", out var ci))
            {
                colorIndex = (int)ci;
            }

            colorImage.color = palette[colorIndex];
        }
    }
}
