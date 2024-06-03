using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    public Locale english;
    private List<Locale> languages;
    public Dropdown switcher;

    private void Awake()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        Debug.Log(english);
        LocalizationSettings.SelectedLocale = english;
        // string currentLanguage = PlayerPrefs.GetString("language", GetDefaultSystemLangcode());
        // languages = LocalizationSettings.AvailableLocales.Locales;
        //
        // switch (currentLanguage)
        // {
        //     case "fr":
        //         switcher.value = 0;
        //         break;
        //     case "nl":
        //         switcher.value = 1;
        //         break;
        //     case "en":
        //         switcher.value = 2;
        //         break;
        //     default:
        //         switcher.value = 2;
        //         break;
        // }
        //
        //
        // //SelectLanguage(currentLanguage);
        // SelectLanguage("en");
    }

    public void DropdownSwitchLanguage(int value)
    {
        string newLanguage;
        Debug.Log(value);

        switch (value)
        {
            case 0:
                newLanguage = "en";
                break;
            case 1:
                newLanguage = "fr";
                break;
            case 2:
                newLanguage = "nl";
                break;
            default:
                newLanguage = "en";
                break;
        }

        PlayerPrefs.SetString("language", newLanguage);
        PlayerPrefs.Save();
        //SelectLanguage(currentLanguage);
        SelectLanguage("en");
    }
    
    public void SelectLanguage(string langcode)
    {
        Debug.Log(langcode);
        foreach (Locale language in languages)
        {
            if (language.Identifier.Code == langcode)
            {
                LocalizationSettings.SelectedLocale = language;
                break;
            }
        }
    }

    private string GetDefaultSystemLangcode()
    {
        SystemLanguage systemLanguage = Application.systemLanguage;

        switch (systemLanguage)
        {
            case SystemLanguage.French: return "fr";
            case SystemLanguage.Dutch: return "nl";
            case SystemLanguage.English: return "en";
            default: return "en";
        }
    }
}