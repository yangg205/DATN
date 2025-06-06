using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;

    public List<TMP_Text> textsToLocalize;
    public List<string> vietnameseTexts;
    public List<string> englishTexts;

    private void Start()
    {
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);
        LoadSavedLanguage();
    }

    void LoadSavedLanguage()
    {
        int savedLang = PlayerPrefs.GetInt("Language", 0);
        languageDropdown.value = savedLang;
        ChangeLanguage(savedLang);
    }

    public void ChangeLanguage(int langIndex)
    {
        PlayerPrefs.SetInt("Language", langIndex);
        for (int i = 0; i < textsToLocalize.Count; i++)
        {
            if (langIndex == 0)
                textsToLocalize[i].text = vietnameseTexts[i];
            else
                textsToLocalize[i].text = englishTexts[i];
        }
    }
}
