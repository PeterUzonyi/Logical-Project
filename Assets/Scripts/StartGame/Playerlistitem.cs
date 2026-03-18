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
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private TextMeshProUGUI readyLabel;
    [SerializeField] private GameObject hostBadge;

    public void Setup(PhotonPlayer player)
    {
        if (playerNameLabel)
            playerNameLabel.text = player.NickName;

        // Ready állapot olvasása
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("ready", out var r))
            isReady = (bool)r;

        // Host automatikusan "kész"-nek számít
        if (player.IsMasterClient) isReady = true;

        if (readyLabel)
        {
            readyLabel.text = isReady ? "Kész" : "Vár...";
            readyLabel.color = isReady
                ? new Color(0.2f, 0.8f, 0.4f)   // zöld
                : new Color(0.8f, 0.8f, 0.8f);  // szürke
        }

        if (hostBadge)
            hostBadge.SetActive(player.IsMasterClient);
    }
}
