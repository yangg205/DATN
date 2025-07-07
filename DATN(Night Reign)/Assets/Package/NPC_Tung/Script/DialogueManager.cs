using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton;

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f;

    private string[] currentDialogueKeys;
    private int currentLineIndex;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private Action<string> _onDialogueEndCallback;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(string[] dialogueKeys, Action<string> onDialogueEnd = null)
    {
        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            EndDialogue();
            return;
        }

        currentDialogueKeys = dialogueKeys;
        currentLineIndex = 0;
        _onDialogueEndCallback = onDialogueEnd;

        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";
        ShowLine();

        nextButton?.SetActive(true);
    }

    public void ShowLine()
    {
        if (!isDialogueActive || currentDialogueKeys == null || currentDialogueKeys.Length == 0)
            return;

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, currentDialogueKeys.Length - 1);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        string localizedLine = GetLocalizedDialogueString(currentDialogueKeys[currentLineIndex]);
        typingCoroutine = StartCoroutine(TypeLine(localizedLine));
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(letterDelay);
        }
        typingCoroutine = null;
    }

    public void NextLine()
    {
        if (!isDialogueActive || currentDialogueKeys == null)
            return;

        // Nếu đang gõ, dừng lại trước khi gõ dòng mới
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Nếu còn dòng, tăng chỉ số và gõ dòng mới
        if (currentLineIndex < currentDialogueKeys.Length)
        {
            string localizedLine = GetLocalizedDialogueString(currentDialogueKeys[currentLineIndex]);
            typingCoroutine = StartCoroutine(TypeLine(localizedLine));
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }


    public void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI?.SetActive(false);
        nextButton?.SetActive(false);
        dialogueText.text = "";
        currentDialogueKeys = null;
        currentLineIndex = 0;

        _onDialogueEndCallback?.Invoke("DialogueFinished");
        _onDialogueEndCallback = null;
    }

   private string GetLocalizedDialogueString(string key)
{
    if (string.IsNullOrEmpty(key))
        return "[EMPTY KEY]";

    key = key.Trim(); // Loại bỏ khoảng trắng đầu cuối, dấu xuống dòng

    StringTable stringTable = LocalizationSettings.StringDatabase.GetTable("NhiemVu");
    if (stringTable == null)
    {
        Debug.LogError("Localization StringTable 'NhiemVu' not found! (Check Package Manager & Localization Settings)");
        return $"ERROR: Table Missing - {key}";
    }

    var entry = stringTable.GetEntry(key);
    if (entry == null)
    {
        Debug.LogError($"❌ Key '{key}' không có trong bảng 'NhiemVu'");
        return $"MISSING KEY: {key}";
    }

    return entry.GetLocalizedString();
}


}
