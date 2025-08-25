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
    private bool isChangingLanguage = false;
    private float lastChangeTime = -1f;
    private const float debounceTime = 1f;

    public static LocalizationManager Instance { get; private set; }
    public static event Action OnLanguageChanged;

    private Dictionary<string, StringTable> _tableCache = new Dictionary<string, StringTable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ LocalizationManager Instance đã sẵn sàng.");

            // Lắng nghe sự kiện của Unity khi locale đổi xong
            LocalizationSettings.SelectedLocaleChanged += OnUnityLocaleChanged;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Có nhiều hơn 1 LocalizationManager trong scene, đã hủy bản sao.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            LocalizationSettings.SelectedLocaleChanged -= OnUnityLocaleChanged;
        }
    }

    private async void Start()
    {
        await LocalizationSettings.InitializationOperation.Task;
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            Debug.LogError("❌ Không thể khởi tạo LocalizationSettings!");
            return;
        }

        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        InitializeDropdownOptions();
        LoadSavedLanguage();
        isInitialized = true;
        Debug.Log("✅ LocalizationManager đã khởi tạo thành công.");
    }

    private void OnDropdownValueChanged(int index)
    {
        if (!isChangingLanguage && Time.time - lastChangeTime >= debounceTime)
        {
            ChangeLanguageImmediate(index);
        }
    }

    private void InitializeDropdownOptions()
    {
        if (languageDropdown == null)
        {
            Debug.LogWarning("⚠️ Chưa gán languageDropdown trong LocalizationManager!");
            return;
        }

        languageDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            options.Add(locale.Identifier.CultureInfo.NativeName);
        }
        languageDropdown.AddOptions(options);
        Debug.Log($"[LocalizationManager] Đã thêm {options.Count} tùy chọn ngôn ngữ vào dropdown.");
    }

    private void LoadSavedLanguage()
    {
        int savedLangIndex = PlayerPrefs.GetInt("Language", 0);
        savedLangIndex = Mathf.Clamp(savedLangIndex, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);

        isChangingLanguage = true;
        languageDropdown.value = savedLangIndex;
        isChangingLanguage = false;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLangIndex];

        Debug.Log($"[LocalizationManager] Đã tải ngôn ngữ lưu trữ: {LocalizationSettings.SelectedLocale.Identifier.Code}");
    }

    public async void ChangeLanguageImmediate(int index)
    {
        if (!isInitialized || isChangingLanguage || Time.time - lastChangeTime < debounceTime)
        {
            Debug.LogWarning($"⚠️ Không thể đổi ngôn ngữ ngay bây giờ! (index: {index}, isInitialized: {isInitialized}, isChangingLanguage: {isChangingLanguage})");
            return;
        }

        Debug.Log($"[LocalizationManager] Bắt đầu thay đổi ngôn ngữ với index: {index}");
        isChangingLanguage = true;
        lastChangeTime = Time.time;

        index = Mathf.Clamp(index, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);
        PlayerPrefs.SetInt("Language", index);

        var locale = LocalizationSettings.AvailableLocales.Locales[index];
        if (LocalizationSettings.SelectedLocale != locale)
        {
            await LocalizationSettings.InitializationOperation.Task;
            LocalizationSettings.SelectedLocale = locale;

            // Lưu ý: KHÔNG gọi OnLanguageChanged ở đây nữa, chỉ chờ OnUnityLocaleChanged
        }

        isChangingLanguage = false;
    }

    private void OnUnityLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        Debug.Log($"[LocalizationManager] Locale thực sự đã đổi sang: {newLocale.Identifier.Code}");
        _tableCache.Clear();
        UILocalizeExtensions.ClearCache();

        // Bắn sự kiện custom khi đổi xong
        OnLanguageChanged?.Invoke();
        RefreshAllLocalizedUI();
    }

    private void RefreshAllLocalizedUI()
    {
        foreach (var locStr in FindObjectsOfType<LocalizeStringEvent>(true))
        {
            locStr.RefreshString();
        }
        Debug.Log("[LocalizationManager] Đã làm mới tất cả các thành phần LocalizeStringEvent.");
    }

    public UnityEngine.Localization.Locale GetCurrentLocale()
    {
        return LocalizationSettings.SelectedLocale;
    }

    public async Task<string> GetLocalizedStringAsync(string tableName, string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key)) return "[EMPTY_KEY]";
        key = key.Trim();

        await LocalizationSettings.InitializationOperation.Task;

        StringTable table;
        if (!_tableCache.TryGetValue(tableName, out table) || table.LocaleIdentifier.Code != LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            var tableHandle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await tableHandle.Task;
            table = tableHandle.Result;
            if (table == null)
            {
                Debug.LogError($"[LocalizationManager] Không tìm thấy bảng: {tableName} cho locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
                return $"[MISSING_TABLE:{tableName}]";
            }
            _tableCache[tableName] = table;
            Debug.Log($"[LocalizationManager] Đã tải bảng: {tableName} cho locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogWarning($"[LocalizationManager] Key '{key}' không tìm thấy trong bảng '{tableName}' cho locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
            return key;
        }

        try
        {
            return entry.GetLocalizedString(args);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalizationManager] Lỗi khi lấy chuỗi cho key '{key}' trong bảng '{tableName}': {ex.Message}");
            return key;
        }
    }
}
