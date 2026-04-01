using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{

    [SerializeField] private TMP_Text buttonText;


    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Mentett nyelv betöltése
        string saved = PlayerPrefs.GetString("Language", "en");
        var locale = LocalizationSettings.AvailableLocales.GetLocale(saved);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        UpdateButtonText();
    }

    public void ToggleLanguage()
    {
        var current = LocalizationSettings.SelectedLocale;
        string newCode = current.Identifier.Code == "en" ? "hu" : "en";

        var newLocale = LocalizationSettings.AvailableLocales.GetLocale(newCode);
        if (newLocale != null)
        {
            LocalizationSettings.SelectedLocale = newLocale;
            PlayerPrefs.SetString("Language", newCode);
            PlayerPrefs.Save();
        }

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        buttonText.text = code == "en" ? "hu" : "en";
        // Ha angol van, mutasd a magyar zászlót (= erre válts)
        // Ha magyar van, mutasd az angol zászlót (= erre válts)
    }
}
