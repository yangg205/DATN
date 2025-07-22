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
    private bool isChangingLanguage = false; // Cờ để ngăn chặn lặp vô hạn
    public static LocalizationManager Instance { get; private set; }
    public static event Action OnLanguageChanged; // Sự kiện tùy chỉnh

    private Dictionary<string, StringTable> _tableCache = new Dictionary<string, StringTable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ LocalizationManager Instance đã sẵn sàng.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Có nhiều hơn 1 LocalizationManager trong scene, đã hủy bản sao.");
        }
    }

    private async void Start()
    {
        // Đảm bảo LocalizationSettings được khởi tạo
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
        if (!isChangingLanguage) // Chỉ xử lý nếu không đang thay đổi ngôn ngữ
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

        isChangingLanguage = true; // Ngăn chặn sự kiện khi đặt giá trị ban đầu
        languageDropdown.value = savedLangIndex; // Sử dụng value thay vì SetValueWithoutNotify
        isChangingLanguage = false;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLangIndex];

        Debug.Log($"[LocalizationManager] Đã tải ngôn ngữ lưu trữ: {LocalizationSettings.SelectedLocale.Identifier.Code}");
        OnLanguageChanged?.Invoke();
        RefreshAllLocalizedUI();
    }

    public async void ChangeLanguageImmediate(int index)
    {
        if (!isInitialized || isChangingLanguage)
        {
            Debug.LogWarning($"⚠️ LocalizationManager chưa được khởi tạo hoặc đang thay đổi ngôn ngữ! (index: {index}, isInitialized: {isInitialized}, isChangingLanguage: {isChangingLanguage})");
            return;
        }

        Debug.Log($"[LocalizationManager] Bắt đầu thay đổi ngôn ngữ với index: {index}");
        isChangingLanguage = true; // Đặt cờ để ngăn chặn lặp

        index = Mathf.Clamp(index, 0, LocalizationSettings.AvailableLocales.Locales.Count - 1);
        PlayerPrefs.SetInt("Language", index);

        var locale = LocalizationSettings.AvailableLocales.Locales[index];
        if (LocalizationSettings.SelectedLocale != locale)
        {
            await LocalizationSettings.InitializationOperation.Task; // Đảm bảo khởi tạo
            LocalizationSettings.SelectedLocale = locale;
            _tableCache.Clear(); // Xóa cache để tải lại bảng với locale mới
            Debug.Log($"[LocalizationManager] Đã thay đổi ngôn ngữ thành: {locale.Identifier.Code}");

            // Đồng bộ giá trị dropdown với locale mới
            isChangingLanguage = true; // Ngăn chặn sự kiện khi thay đổi dropdown
            languageDropdown.value = index;
            isChangingLanguage = false;

            OnLanguageChanged?.Invoke();
            RefreshAllLocalizedUI();
        }
        else
        {
            Debug.Log($"[LocalizationManager] Không thay đổi ngôn ngữ vì locale hiện tại ({LocalizationSettings.SelectedLocale.Identifier.Code}) trùng với locale mới ({locale.Identifier.Code})");
        }

        isChangingLanguage = false; // Đặt lại cờ sau khi hoàn tất
        Debug.Log($"[LocalizationManager] Hoàn tất thay đổi ngôn ngữ với index: {index}");
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

        // Đảm bảo hệ thống localization đã sẵn sàng
        await LocalizationSettings.InitializationOperation.Task;

        StringTable table;
        if (!_tableCache.TryGetValue(tableName, out table) || table.LocaleIdentifier.Code != LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            var tableHandle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await tableHandle.Task;
            table = tableHandle.Result;
            if (table == null)
            {
                Debug.LogError($"Không tìm thấy bảng: {tableName}");
                return $"[MISSING_TABLE:{tableName}]";
            }
            _tableCache[tableName] = table; // Cập nhật cache với bảng mới
            Debug.Log($"[LocalizationManager] Đã tải bảng: {tableName} cho locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogWarning($"Key '{key}' không tìm thấy trong bảng '{tableName}' cho locale: {LocalizationSettings.SelectedLocale.Identifier.Code}");
            return key; // Trả về key thay vì [MISSING:<key>] để kiểm tra
        }

        try
        {
            return entry.GetLocalizedString(args);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Lỗi khi lấy chuỗi cho key '{key}' trong bảng '{tableName}': {ex.Message}");
            return key; // Trả về key làm fallback
        }
    }
}