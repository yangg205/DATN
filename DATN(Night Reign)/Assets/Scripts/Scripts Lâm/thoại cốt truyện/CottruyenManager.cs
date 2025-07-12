using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class DialogueQueueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Settings")]
    public float displayTimePerDialogue = 5f;
    public float typewriterSpeed = 0.03f; // Tốc độ gõ từng ký tự

    [Header("Dialogue Data")]
    public DialogueData dialogueData;

    private Queue<LocalizedString> dialogueQueue = new Queue<LocalizedString>();
    private bool isDisplaying = false;

    private const string TableName = "CutsceneNew"; // Tên bảng localization

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (dialogueData != null)
        {
            PlayDialogueData(dialogueData);
        }
    }

    public void PlayDialogueData(DialogueData data)
    {
        if (data == null || data.keys.Count == 0)
        {
            Debug.LogWarning("⚠️ DialogueData null hoặc rỗng.");
            return;
        }

        foreach (string rawKey in data.keys)
        {
            string key = rawKey.Trim();
            var locString = new LocalizedString
            {
                TableReference = TableName,
                TableEntryReference = key
            };

            dialogueQueue.Enqueue(locString);
        }

        if (!isDisplaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isDisplaying = true;

        while (dialogueQueue.Count > 0)
        {
            var locString = dialogueQueue.Dequeue();
            bool isLoaded = false;
            string fullText = "";

            locString.StringChanged += OnStringReady;
            locString.RefreshString();

            void OnStringReady(string value)
            {
                string localeCode = LocalizationSettings.SelectedLocale?.Identifier.Code;

                if (value.Contains("Not Found"))
                {
                    string message = $"❌ Không tìm thấy key: {locString.TableReference}/{locString.TableEntryReference} (locale: {localeCode})";

                    if (localeCode == "vi")
                        Debug.LogError(message);
                    else
                        Debug.LogWarning(message);
                }

                fullText = value;
                isLoaded = true;
                locString.StringChanged -= OnStringReady;
            }

            yield return new WaitUntil(() => isLoaded);

            dialoguePanel.SetActive(true);
            dialogueText.text = "";

            // Bắt đầu gõ từng ký tự
            foreach (char c in fullText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            // Giữ nguyên panel một lúc sau khi gõ xong
            yield return new WaitForSeconds(displayTimePerDialogue);
            dialoguePanel.SetActive(false);
        }

        isDisplaying = false;
    }
}
