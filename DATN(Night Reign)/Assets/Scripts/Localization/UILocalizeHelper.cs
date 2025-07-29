using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class UILocalizeExtensions
{
    private static readonly Dictionary<string, StringTable> cachedTables = new();

    public static void ClearCache()
    {
        cachedTables.Clear();
        Debug.Log("[UILocalizeExtensions] Đã xóa cache bảng localization.");
    }

    public static async Task SetLocalizationKey(this GameObject obj, string key, string tableName = "UI_Texts")
    {
        if (obj == null)
        {
            Debug.LogWarning("GameObject is null.");
            return;
        }

        var tmpText = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogWarning($"No TextMeshProUGUI found inside GameObject '{obj.name}'.");
            return;
        }

        key = key.Trim();
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("Localization key is empty.");
            return;
        }

        if (!LocalizationSettings.InitializationOperation.IsDone)
            await LocalizationSettings.InitializationOperation.Task;

        if (!cachedTables.TryGetValue(tableName, out var table))
        {
            var handle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await handle.Task;

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                table = handle.Result;
                cachedTables[tableName] = table;
            }
            else
            {
                Debug.LogError($"Failed to load localization table '{tableName}'.");
                tmpText.text = "[TABLE LOAD ERROR]";
                return;
            }
        }

        await ApplyLocalization(tmpText, table, key);
    }

    private static async Task ApplyLocalization(TextMeshProUGUI tmpText, StringTable table, string key)
    {
        string localized = await LocalizationManager.Instance.GetLocalizedStringAsync(table.TableCollectionName, key);
        Debug.Log($"[UILocalizeExtensions] Applying localized text for key '{key}': '{localized}' to '{tmpText.gameObject.name}'");
        tmpText.text = localized;
    }
}
