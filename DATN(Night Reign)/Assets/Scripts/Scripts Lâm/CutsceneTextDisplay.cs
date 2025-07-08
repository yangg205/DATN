using UnityEngine;
using TMPro;

public class CutsceneTextDisplay : MonoBehaviour
{
    public TMP_Text dialogueText;
    public string key; // ví dụ: "Intro_1"

    void Start()
    {
        if (LocalizationCusScene.Instance != null)
        {
            string text = LocalizationCusScene.Instance.GetText(key);
            dialogueText.text = text;
        }
    }
}
