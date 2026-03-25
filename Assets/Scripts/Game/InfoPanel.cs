using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject panel;

    public static InfoPanel Instance { get; private set; }

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
    }
}
