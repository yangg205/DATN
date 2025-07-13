using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class DialogueQueueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject blackScreenPanel;

    [Header("Settings")]
    public float displayTimePerDialogue = 5f;
    public float finalDialogueHoldTime = 3f;
    public float typewriterSpeed = 0.03f;
    public float fadeDuration = 1f;

    [Header("Dialogue Data")]
    public DialogueData dialogueData;

    [Header("Timeline")]
    public PlayableDirector cutsceneTimeline;

    [Header("Scene To Load After Cutscene")]
    public string nextSceneName = "Map_SaMac";

    [Header("Voice Settings")]
    public AudioSource voiceSource;

    private Queue<LocalizedString> dialogueQueue = new Queue<LocalizedString>();
    private bool hasPlayed = false;
    private CanvasGroup canvasGroup;

    private const string TableName = "CutsceneNew";

    void Start()
    {
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (blackScreenPanel != null)
        {
            var img = blackScreenPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = new Color(0, 0, 0, 0);
        }

        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();
    }

    public void TriggerCutscene()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        if (cutsceneTimeline != null)
            cutsceneTimeline.Play();

        if (dialogueData != null)
        {
            PrepareDialogue(dialogueData);
            StartCoroutine(ProcessQueueThenCheckTimeline());
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

    private IEnumerator ProcessQueueThenCheckTimeline()
    {
        yield return StartCoroutine(ProcessQueue(holdLastDialogue: true));

        yield return new WaitUntil(() =>
            cutsceneTimeline == null || cutsceneTimeline.state != PlayState.Playing
        );

        EndCutscene();
    }

    private IEnumerator ProcessQueue(bool holdLastDialogue = false)
    {
        int index = 0;

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
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
            dialogueText.text = "";

            PlayVoiceByIndex(index); // 🔊 phát voice theo ngôn ngữ

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
                canvasGroup.DOFade(0f, fadeDuration);
                yield return new WaitForSeconds(fadeDuration);
                dialoguePanel.SetActive(false);
            }

            index++;
        }
    }

    private void PlayVoiceByIndex(int index)
    {
        if (dialogueData == null || voiceSource == null) return;

        string langCode = LocalizationSettings.SelectedLocale.Identifier.Code.ToLower();
        AudioClip clip = null;

        if (langCode == "vi" && index < dialogueData.voiceVi.Count)
            clip = dialogueData.voiceVi[index];
        else if (langCode == "en" && index < dialogueData.voiceEn.Count)
            clip = dialogueData.voiceEn[index];

        if (clip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }

    private void EndCutscene()
    {
        Debug.Log("✅ Thoại và Timeline kết thúc. Fade màn hình đen.");

        if (blackScreenPanel != null)
        {
            var img = blackScreenPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                blackScreenPanel.SetActive(true);
                img.DOFade(1f, 1f).OnComplete(() =>
                {
                    Debug.Log("🎬 Fade hoàn tất. Chuyển scene...");
                    SceneManager.LoadScene(nextSceneName);
                });
            }
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
