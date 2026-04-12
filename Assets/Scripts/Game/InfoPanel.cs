using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

/// <summary>
/// Pops up to inform the players about the last round, the Final Touches (Végsõ Rendrakás) 
/// and the fianl standings after the game is over
/// </summary>
public class InfoPanel : MonoBehaviour
{
    public static InfoPanel Instance { get; private set; }

    /// <summary>
    /// The massage part
    /// </summary>
    [SerializeField] private TMP_Text infoText;

    /// <summary>
    /// The panel itself
    /// </summary>
    [SerializeField] private GameObject panel;

    [Header("Jelenet neve")]
    [SerializeField] private string menuSceneName = "StartGameScene";

    //Called when the script is loaded
    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    /// <summary>
    /// Shows the panel and the message
    /// </summary>
    /// <param name="message"></param>
    public void Show(string message)
    {
        infoText.text = message;
        panel.SetActive(true);
    }

    /// <summary>
    /// When the panel's Ok button is clicked closes the panel. 
    /// If the game is over, then every player returns to the starting menu
    /// </summary>
    public void OnOkClicked()
    {
        panel.SetActive(false);

        // Csak GameOver után navigálunk vissza
        if (TurnManager.Instance == null || !TurnManager.Instance.isGameOver)
        {
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Online: kilépés a szobából, a callback intézi a visszanavigálást
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // Lokális: egyszerû jelenetváltás
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
