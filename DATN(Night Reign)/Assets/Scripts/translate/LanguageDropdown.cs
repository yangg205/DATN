using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class LanguageDropdownTMP : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // Gắn sự kiện vào TMP Dropdown khi thay đổi giá trị
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        // Đặt giá trị ban đầu cho Dropdown theo ngôn ngữ hiện tại
        SetCurrentLanguage();
    }

    private void OnLanguageChanged(int index)
    {
        string selectedLanguage = languageDropdown.options[index].text; // Lấy tên ngôn ngữ từ Dropdown
        string localeCode = GetLocaleCode(selectedLanguage); // Chuyển đổi tên ngôn ngữ thành mã locale
        SetLanguage(localeCode);
    }

    private void SetLanguage(string localeCode)
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales.Find(locale => locale.Identifier.Code == localeCode);
        if (selectedLocale != null)
        {
            LocalizationSettings.SelectedLocale = selectedLocale;
        }
        else
        {
            Debug.LogWarning("Locale không tìm thấy: " + localeCode);
        }
    }

    private string GetLocaleCode(string languageName)
    {
        // Map tên ngôn ngữ sang mã locale
        switch (languageName)
        {
            case "English":
                Debug.Log("Đang là tiếng anh");
                return "en";
            case "Vietnamese":
                Debug.Log("Đang là tiếng việt");
                return "vi";
            default:
                return "en"; // Mặc định là tiếng Anh
        }
    }

    private void SetCurrentLanguage()
    {
        // Lấy mã locale hiện tại
        string currentLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        // Đồng bộ TMP Dropdown với ngôn ngữ hiện tại
        for (int i = 0; i < languageDropdown.options.Count; i++)
        {
            if (GetLocaleCode(languageDropdown.options[i].text) == currentLocaleCode)
            {
                languageDropdown.value = i;
                break;
            }
        }
    }
}
