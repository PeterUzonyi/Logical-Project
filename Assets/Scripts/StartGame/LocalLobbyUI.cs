using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public class LocalLobbyUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject localLobbyPanel;

    [Header("Játékosok száma")]
    [SerializeField] private Button btn2Players;
    [SerializeField] private Button btn3Players;
    [SerializeField] private Button btn4Players;

    [Header("Játékos sorok (PlayerRow_1..4)")]
    [SerializeField] private GameObject[] playerRows = new GameObject[4];
    [SerializeField] private TMP_InputField[] playerNameInputs = new TMP_InputField[4];
        
    [Header("Szín választók")]
    [SerializeField] private TMP_Dropdown[] colorDropdowns = new TMP_Dropdown[4];

    [Header("Gondolkodási idõ")]
    [SerializeField] private Slider thinkingTimeSlider;
    [SerializeField] private TextMeshProUGUI thinkingTimeLabel;

    [Header("Gombok")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;

    [Header("Jelenet")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Elõre megadott színpaletta
    private readonly Color[] palette = new Color[]
    {
        new Color(0.85f, 0.22f, 0.22f), // piros
        new Color(0.22f, 0.45f, 0.85f), // kék
        new Color(0.22f, 0.72f, 0.33f), // zöld
        new Color(0.95f, 0.75f, 0.10f), // sárga
        new Color(0.70f, 0.25f, 0.80f), // lila
        new Color(0.95f, 0.50f, 0.10f), // narancs
    };

    private int selectedPlayerCount = 2;

    void Start()
    {
        btn2Players.onClick.AddListener(() => SetPlayerCount(2));
        btn3Players.onClick.AddListener(() => SetPlayerCount(3));
        btn4Players.onClick.AddListener(() => SetPlayerCount(4));

        thinkingTimeSlider.minValue = 30;
        thinkingTimeSlider.maxValue = 120;
        thinkingTimeSlider.value = 30;
        thinkingTimeSlider.onValueChanged.AddListener(OnThinkingTimeChanged);

        startButton.onClick.AddListener(OnStartClicked);
        backButton.onClick.AddListener(OnBackClicked);

        for (int i = 0; i < colorDropdowns.Length; i++)
        {
            int playerIndex = i;
            TMP_Dropdown dropdown = colorDropdowns[i];

            dropdown.ClearOptions();

            // Opciók feltöltése sprite-okkal
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (int j = 0; j < palette.Length; j++)
                options.Add(new TMP_Dropdown.OptionData("", CreateColorSprite(palette[j])));

            dropdown.AddOptions(options);

            // Listener ELÕTT állítjuk be az alapértéket, hogy ne süljön el
            dropdown.SetValueWithoutNotify(i % palette.Length);
            dropdown.RefreshShownValue();

            dropdown.onValueChanged.AddListener((val) => OnColorDropdownChanged(playerIndex, val));

            GameConfig.PlayerColors[i] = palette[i % palette.Length];
        }

        SetPlayerCount(2);
    }

    private void OnColorDropdownChanged(int playerIndex, int colorIndex)
    {
        GameConfig.PlayerColors[playerIndex] = palette[colorIndex];
    }

    private Sprite CreateColorSprite(Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }

    private void SetPlayerCount(int count)
    {
        selectedPlayerCount = count;

        for (int i = 0; i < playerRows.Length; i++)
            playerRows[i].SetActive(i < count);

        btn2Players.interactable = count != 2;
        btn3Players.interactable = count != 3;
        btn4Players.interactable = count != 4;
    }

    private void OnThinkingTimeChanged(float value)
    {
        float rounded = Mathf.Round(value / 5f) * 5f;
        thinkingTimeSlider.value = rounded;
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "hu")
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Gondolkodási idõ: {rounded}s";
            }
        }
        else
        {
            if (thinkingTimeLabel)
            {
                thinkingTimeLabel.text = $"Thinking time: {rounded}s";
            }
        }
    }

    private void OnStartClicked()
    {
        for (int i = 0; i < selectedPlayerCount; i++)
        {
            string name = playerNameInputs[i].text.Trim();
            if (string.IsNullOrEmpty(name))
                name = $"Játékos {i + 1}";
            GameConfig.PlayerNames[i] = name;
        }

        GameConfig.PlayerCount = selectedPlayerCount;
        GameConfig.ThinkingTime = Mathf.Round(thinkingTimeSlider.value / 5f) * 5f;

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnBackClicked()
    {
        localLobbyPanel.SetActive(false);
    }

    public void Open()
    {
        localLobbyPanel.SetActive(true);
    }
}