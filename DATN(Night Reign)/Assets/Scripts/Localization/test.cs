using UnityEngine;

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
        localizationManager.ChangeLanguageImmediate(testInt);
    }
}
