using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//SceneManager.LoadScene("");

public class DialogueQueueManager : MonoBehaviour
{
    
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject blackScreenPanel;        // 🎯 Là panel chứa background đen
    private Image blackScreenImage;            // Image nằm trên chính panel đó

    [Header("Settings")]
    public float displayTimePerDialogue = 5f;
    public float finalDialogueHoldTime = 3f;
    public float typewriterSpeed = 0.03f;
    public float fadeDuration = 1f;

    [Header("Dialogue Data")]
    public DialogueData dialogueData;

    [Header("Timeline")]
    public PlayableDirector cutsceneTimeline;

    private Queue<LocalizedString> dialogueQueue = new Queue<LocalizedString>();
    private bool hasPlayed = false;

    private const string TableName = "CutsceneNew";

    void Start()
    {
        dialoguePanel.SetActive(false);

        if (blackScreenPanel != null)
        {
            blackScreenImage = blackScreenPanel.GetComponent<Image>();
            if (blackScreenImage == null)
            {
                Debug.LogError("⚠️ Panel phải có component Image để fade alpha.");
            }
            else
            {
                Color c = blackScreenImage.color;
                c.a = 0f;
                blackScreenImage.color = c;
                blackScreenPanel.SetActive(false);
            }
        }
    }

    public void TriggerCutscene()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        if (cutsceneTimeline != null)
        {
            cutsceneTimeline.Play();
        }

        if (dialogueData != null)
        {
            PrepareDialogue(dialogueData);
            StartCoroutine(ProcessQueueThenFadeOut());
        }
    }

    private void PrepareDialogue(DialogueData data)
    {
        dialogueQueue.Clear();

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
    }

    private IEnumerator ProcessQueueThenFadeOut()
    {
        yield return StartCoroutine(ProcessQueue(holdLastDialogue: true));

        yield return new WaitUntil(() =>
            cutsceneTimeline == null || cutsceneTimeline.state != PlayState.Playing
        );

        yield return StartCoroutine(FadeBlackScreen());
    }

    private IEnumerator ProcessQueue(bool holdLastDialogue = false)
    {
        while (dialogueQueue.Count > 0)
        {
            var locString = dialogueQueue.Dequeue();
            bool isLoaded = false;
            string fullText = "";

            locString.StringChanged += OnStringReady;
            locString.RefreshString();

            void OnStringReady(string value)
            {
                fullText = value;
                isLoaded = true;
                locString.StringChanged -= OnStringReady;
            }

            yield return new WaitUntil(() => isLoaded);

            dialoguePanel.SetActive(true);
            dialogueText.text = "";

            foreach (char c in fullText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            if (dialogueQueue.Count == 0 && holdLastDialogue)
            {
                yield return new WaitForSeconds(finalDialogueHoldTime);
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(displayTimePerDialogue);
                dialoguePanel.SetActive(false);
            }
        }
    }

    private IEnumerator FadeBlackScreen()
    {
        Debug.Log("✅ Bắt đầu fade Panel đen (BlackScreenPanel)");

        if (blackScreenImage == null) yield break;

        blackScreenPanel.SetActive(true);

        Color fromColor = blackScreenImage.color;
        fromColor.a = 0f;
        blackScreenImage.color = fromColor;

        Color toColor = fromColor;
        toColor.a = 1f;

        yield return blackScreenImage
            .DOColor(toColor, fadeDuration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        Debug.Log("🎬 Fade Panel hoàn tất. Load scene tiếp theo nếu muốn.");
    }

}
