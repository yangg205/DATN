using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LocalizationCusScene : MonoBehaviour
{
    public static LocalizationCusScene Instance;

    public TextAsset localizationCSV;
    public string currentLanguage = "Vietnamese"; // hoặc "English"

    private Dictionary<string, string> localizedTexts = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocalization();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadLocalization()
    {
        localizedTexts.Clear();
        var lines = localizationCSV.text.Split('\n');
        var headers = lines[0].Split(',');

        int langIndex = System.Array.IndexOf(headers, currentLanguage);
        if (langIndex == -1)
        {
            Debug.LogError("Language not found in CSV: " + currentLanguage);
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split(',');
            if (columns.Length <= langIndex) continue;

            string key = columns[0].Trim();
            string value = columns[langIndex].Trim();
            localizedTexts[key] = value;
        }
    }

    public string GetText(string key)
    {
        if (localizedTexts.TryGetValue(key, out var value))
            return value;
        return $"[{key}]";
    }

    public void ChangeLanguage(string language)
    {
        currentLanguage = language;
        LoadLocalization();
    }
}
