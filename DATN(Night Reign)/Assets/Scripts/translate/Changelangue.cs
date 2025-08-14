using TMPro;
using UnityEngine;

public class Changelangue : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LocalizationManager.Instance.ChangeLanguageImmediate(languageDropdown.value);
        Debug.Log("Language changed to: " + languageDropdown.value);
    }
}
