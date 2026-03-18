using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System;

/// <summary>
/// Egy szoba sor a szoba listában.
/// 
/// Prefab struktúra:
///   RoomListItem
///     - RoomNameLabel    (TextMeshProUGUI)
///     - PlayerCountLabel (TextMeshProUGUI)  pl. "2/4"
///     - JoinButton       (Button)
/// </summary>
public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameLabel;
    [SerializeField] private TextMeshProUGUI playerCountLabel;
    [SerializeField] private Button joinButton;

    public void Setup(RoomInfo info, Action onJoin)
    {
        if (roomNameLabel) roomNameLabel.text = info.Name;
        if (playerCountLabel) playerCountLabel.text = $"{info.PlayerCount}/{info.MaxPlayers}";

        joinButton?.onClick.RemoveAllListeners();
        joinButton?.onClick.AddListener(() => onJoin?.Invoke());

        // Ha tele van a szoba, tiltsuk le a gombot
        if (joinButton) joinButton.interactable = info.PlayerCount < info.MaxPlayers;
    }
}
