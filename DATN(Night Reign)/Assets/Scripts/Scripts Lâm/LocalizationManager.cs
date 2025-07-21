using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using System.Collections.Generic;
using UnityEngine.Localization.Tables;
using System;
using System.Threading.Tasks;

public class LocalizationManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private bool isInitialized = false;
    public static LocalizationManager Instance { get; private set; }
    public static event Action OnLanguageChanged; // Sự kiện tùy chỉnh của bạn

    private Dictionary<string, StringTable> _tableCache = new Dictionary<string, StringTable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        languageDropdown.onValueChanged.AddListener(ChangeLanguageImmediate);
        InitializeDropdownOptions();
        LoadSavedLanguage();
        isInitialized = true;
    }

    private void InitializeDropdownOptions()
    {
        languageDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            options.Add(locale.Identifier.CultureInfo.NativeName);
        }
        languageDropdown.AddOptions(options);
    }

    private void LoadSavedLanguage()
    {
        int savedLangIndex = PlayerPrefs.GetInt("Language", 0);
        savedLangIndex = Mathf.Clamp(savedLangIndex, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);

        languageDropdown.SetValueWithoutNotify(savedLangIndex);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLangIndex];

        OnLanguageChanged?.Invoke();
        RefreshAllLocalizedUI();
    }

    public void ChangeLanguageImmediate(int index)
    {
        if (!isInitialized) return;

        index = Mathf.Clamp(index, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);
        PlayerPrefs.SetInt("Language", index);

        var locale = LocalizationSettings.AvailableLocales.Locales[index];

        if (LocalizationSettings.SelectedLocale != locale)
        {
            LocalizationSettings.SelectedLocale = locale;
            _tableCache.Clear();

            OnLanguageChanged?.Invoke();
            RefreshAllLocalizedUI();
        }
    }

    private void RefreshAllLocalizedUI()
    {
        foreach (var locStr in FindObjectsOfType<LocalizeStringEvent>(true))
        {
            locStr.RefreshString();
        }
    }

    // ĐIỂM SỬA LỖI QUAN TRỌNG: Đã thêm hàm GetCurrentLocale() vào class này
    public UnityEngine.Localization.Locale GetCurrentLocale()
    {
        return LocalizationSettings.SelectedLocale;
    }

    public async Task<string> GetLocalizedStringAsync(string tableName, string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key)) return "[EMPTY_KEY]";
        key = key.Trim();

        StringTable table;
        if (!_tableCache.TryGetValue(tableName, out table))
        {
            table = await LocalizationSettings.StringDatabase.GetTableAsync(tableName).Task;
            if (table == null)
            {
                Debug.LogError($"Không tìm thấy bảng: {tableName}");
                return $"[MISSING_TABLE:{tableName}]";
            }
            _tableCache[tableName] = table;
        }

        var entry = table.GetEntry(key);
        return entry?.GetLocalizedString(args) ?? $"[MISSING:{key}]";
    }
}