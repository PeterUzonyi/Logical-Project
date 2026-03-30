using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject panel;

    public static InfoPanel Instance { get; private set; }

    [Header("Jelenet neve")]
    [SerializeField] private string menuSceneName = "StartGameScene";

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    // Hívd meg amikor meg akarod jeleníteni
    public void Show(string message)
    {
        infoText.text = message;
        panel.SetActive(true);
    }

    // Az OK gombhoz rendeld hozzá az Inspectorban
    public void OnOkClicked()
    {
        panel.SetActive(false);

        // Csak GameOver után navigálunk vissza
        if (TurnManager.Instance == null || !TurnManager.Instance.isGameOver) return;

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
