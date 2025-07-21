using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;

public class LanguageDropdown : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown languageDropdown;

    [Header("Buttons to Localize")]
    public TextMeshProUGUI newGameText;
    public TextMeshProUGUI continueText;
    public TextMeshProUGUI settingsText;
    public TextMeshProUGUI exitText;
    public TextMeshProUGUI mouseText;

    private bool isInitialized = false;
    private StringTable buttonTable;

    private void Start()
    {
        StartCoroutine(InitLanguages());
    }

    IEnumerator InitLanguages()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Gán danh sách ngôn ngữ vào dropdown
        languageDropdown.options.Clear();
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            string displayName = LocalizationSettings.AvailableLocales.Locales[i].Identifier.CultureInfo.NativeName;
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
        }

        int savedLang = PlayerPrefs.GetInt("Language", 0);
        languageDropdown.value = savedLang;
        languageDropdown.onValueChanged.AddListener(ChangeLanguage);

        // Đặt ngôn ngữ đã lưu
        yield return SetLanguage(savedLang);

        isInitialized = true;
    }

    public void ChangeLanguage(int langIndex)
    {
        if (!isInitialized) return;

        PlayerPrefs.SetInt("Language", langIndex);
        StartCoroutine(SetLanguage(langIndex));
    }

    IEnumerator SetLanguage(int index)
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;

        // Load String Table "Button"
        var tableHandle = LocalizationSettings.StringDatabase.GetTableAsync("Button");
        yield return tableHandle;
        if (tableHandle.Status == AsyncOperationStatus.Succeeded)
        {
            buttonTable = tableHandle.Result;
            UpdateButtonTexts();
        }
        else
        {
            Debug.LogError("Failed to load String Table: Button");
        }

        yield return null;
        Debug.Log($"🌐 Language switched to: {selectedLocale.Identifier.CultureInfo.EnglishName}");
    }

    private void UpdateButtonTexts()
    {
        if (buttonTable == null) return;

        // Gán text dựa theo key trong bảng "Button"
        newGameText.text = buttonTable.GetEntry("btn_NewGame")?.GetLocalizedString() ?? "[MISSING]";
        continueText.text = buttonTable.GetEntry("btn_Continue")?.GetLocalizedString() ?? "[MISSING]";
        settingsText.text = buttonTable.GetEntry("btn_Settings")?.GetLocalizedString() ?? "[MISSING]";
        exitText.text = buttonTable.GetEntry("btn_Exit")?.GetLocalizedString() ?? "[MISSING]";
        mouseText.text = buttonTable.GetEntry("btn_Mouse")?.GetLocalizedString() ?? "[MISSING]";
    }
}
