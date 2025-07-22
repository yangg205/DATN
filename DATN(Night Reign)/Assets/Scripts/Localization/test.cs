using UnityEngine;
using UnityEngine.Localization.Settings;

public class test : MonoBehaviour
{
    public int testInt = 0;
    private LocalizationManager localizationManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localizationManager = FindAnyObjectByType<LocalizationManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) testInt = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) testInt = 1;
        if (localizationManager != null && testInt >= 0 && testInt < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            localizationManager.ChangeLanguageImmediate(testInt);
        }
    }
}
