using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class UILocalizeExtensions_lam
{
    


    // Cache bảng localization
    private static readonly Dictionary<string, StringTable> cachedTables = new();

    /// <summary>
    /// Gán khóa localization cho GameObject có TMP_Text (hoặc con của nó).
    /// </summary>
    public static async void SetLocalizationKey(this GameObject obj, string key, string tableName = "Button")
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

        // Ensure Localization system is initialized
        if (!LocalizationSettings.InitializationOperation.IsDone)
            await LocalizationSettings.InitializationOperation.Task;

        // Use cached table if available
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

        ApplyLocalization(tmpText, table, key);
    }

    /// <summary>
    /// Gán văn bản localized vào TextMeshProUGUI.
    /// </summary>
    private static void ApplyLocalization(TextMeshProUGUI tmpText, StringTable table, string key)
    {
        var entry = table.GetEntry(key);
        if (entry != null)
        {
            tmpText.text = entry.GetLocalizedString();
        }
        else
        {
            Debug.LogWarning($"Key '{key}' not found in table '{table.TableCollectionName}'.");
            tmpText.text = $"[MISSING:{key}]";
        }
    }
}