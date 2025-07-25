using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public bool IsDialogueActive => isDialogueActive;
    private bool hasInterruptedTyping = false;


    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip nextDialogueSound;

    [Header("Default Dialogue Fallback (Optional)")]
    [SerializeField] private string[] defaultDialogueKeys; // Thêm mảng khóa hội thoại mặc định trong Inspector
    [SerializeField] private AudioClip[] defaultVoiceClipsEN;
    [SerializeField] private AudioClip[] defaultVoiceClipsVI;

    private List<string> _originalDialogueKeys;
    private List<AudioClip> _originalVoiceClipsEN;
    private List<AudioClip> _originalVoiceClipsVI;
    private int _currentDialogueIndex = -1;
    private Queue<string> currentDialogueLines;
    private Queue<AudioClip> currentVoiceClips;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Action _onDialogueEndCallback;
    private string currentFullSentence;
    public bool IsTyping => isTyping; // hoặc tương tự


    // Getter công khai để truy cập số lượng câu thoại
    public int DialogueLinesCount => currentDialogueLines?.Count ?? 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment nếu cần
        }
        else
        {
            Destroy(gameObject);
        }

        currentDialogueLines = new Queue<string>();
        currentVoiceClips = new Queue<AudioClip>();
        _originalDialogueKeys = new List<string>();
        _originalVoiceClipsEN = new List<AudioClip>();
        _originalVoiceClipsVI = new List<AudioClip>();

        if (dialogueUI == null) Debug.LogError("[DialogueManager] Dialogue UI (Panel) is not assigned!");
        if (dialogueText == null) Debug.LogError("[DialogueManager] Dialogue Text (TextMeshProUGUI) is not assigned!");

        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("[DialogueManager] Dialogue Audio Source was not assigned. Added one.");
        }

        dialogueUI?.SetActive(false);
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChangedHandler;
        }
        else
        {
            Debug.LogWarning("[DialogueManager] LocalizationManager.Instance not found.");
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChangedHandler;
        }
    }

    private async void OnLanguageChangedHandler()
    {
        if (!isDialogueActive || _originalDialogueKeys.Count == 0) return;

        Debug.Log("[DialogueManager] Language changed, repopulating dialogue queue");
        StopTypingEffect();
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Stop();
        }

        var currentLocale = LocalizationManager.Instance.GetCurrentLocale();
        int resumeIndex = _currentDialogueIndex < 0 ? 0 : _currentDialogueIndex;
        await RePopulateQueuesFromOriginalData(currentLocale, resumeIndex);

        _currentDialogueIndex = resumeIndex - 1;
        DisplayNextSentence();
    }

    public async void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType, Action onDialogueEnd = null, string[] keys = null, AudioClip[] voiceClipsEN = null, AudioClip[] voiceClipsVI = null)
    {
        string[] dialogueKeys = keys ?? (quest != null ? quest.GetDialogueKeys(dialogueType) : Array.Empty<string>());
        AudioClip[] clipsEN = voiceClipsEN ?? (quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_EN,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_EN,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_EN,
            QuestDialogueType.InProgress => Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>(),
        } : Array.Empty<AudioClip>());
        AudioClip[] clipsVI = voiceClipsVI ?? (quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_VI,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_VI,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_VI,
            QuestDialogueType.InProgress => Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>(),
        } : Array.Empty<AudioClip>());

        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            Debug.LogWarning($"[DialogueManager] No dialogue keys provided for {dialogueType} in QuestData: {(quest != null ? quest.questName : "null")}");
            dialogueKeys = defaultDialogueKeys; // Sử dụng hội thoại mặc định nếu không có khóa
        }

        Debug.Log($"[DialogueManager] Starting dialogue for {dialogueType} with {dialogueKeys.Length} keys: {string.Join(", ", dialogueKeys)}");
        if (dialogueKeys.Length <= 2)
        {
            Debug.LogWarning($"[DialogueManager] Only {dialogueKeys.Length} dialogue keys provided. Consider adding more for longer dialogue.");
        }

        StopTypingEffect();
        if (dialogueAudioSource != null) dialogueAudioSource.Stop();

        _originalDialogueKeys.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1;

        _originalDialogueKeys.AddRange(dialogueKeys);
        if (clipsEN != null) _originalVoiceClipsEN.AddRange(clipsEN);
        if (clipsVI != null) _originalVoiceClipsVI.AddRange(clipsVI);

        _onDialogueEndCallback = onDialogueEnd;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";

        var currentLocale = LocalizationManager.Instance.GetCurrentLocale();
        await RePopulateQueuesFromOriginalData(currentLocale);
        DisplayNextSentence();
    }

    private async Task RePopulateQueuesFromOriginalData(UnityEngine.Localization.Locale targetLocale, int startIndex = 0)
    {
        currentDialogueLines.Clear();
        currentVoiceClips.Clear();

        bool isVI = targetLocale.Identifier.Code.StartsWith("vi", StringComparison.OrdinalIgnoreCase);
        Debug.Log($"[DialogueManager] Repopulating queue for language {targetLocale.Identifier.Code}, starting from index {startIndex}, original keys: {_originalDialogueKeys.Count} ({string.Join(", ", _originalDialogueKeys)})");

        for (int i = startIndex; i < _originalDialogueKeys.Count; i++)
        {
            string key = _originalDialogueKeys[i];
            string localizedLine = $"[Missing: {key}]";
            try
            {
                localizedLine = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", key);
                if (string.IsNullOrEmpty(localizedLine))
                {
                    Debug.LogWarning($"[DialogueManager] Localized string for key {key} is empty or null!");
                    localizedLine = $"[Missing: {key}]";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Error localizing key {key}: {ex.Message}");
            }

            currentDialogueLines.Enqueue(localizedLine);
            Debug.Log($"[DialogueManager] Enqueued line {i}: {localizedLine}");

            AudioClip voiceClip = GetVoiceClipForIndex(i, isVI);
            currentVoiceClips.Enqueue(voiceClip);
            Debug.Log($"[DialogueManager] Enqueued voice clip {i}: {(voiceClip != null ? voiceClip.name : "null")}");
        }

        Debug.Log($"[DialogueManager] Repopulated dialogue queue with {currentDialogueLines.Count} lines, voice clips: {currentVoiceClips.Count}");
        if (currentDialogueLines.Count == 0)
        {
            Debug.LogError("[DialogueManager] No lines enqueued! Check QuestData dialogue keys or localization table 'NhiemVu'.");
        }
    }


    private AudioClip GetVoiceClipForIndex(int index, bool isVI)
    {
        List<AudioClip> targetVoiceList = isVI ? _originalVoiceClipsVI : _originalVoiceClipsEN;
        if (index >= 0 && index < targetVoiceList.Count)
        {
            return targetVoiceList[index];
        }
        return null;
    }

    public void DisplayNextSentence()
    {
        Debug.Log($"[DialogueManager] DisplayNextSentence called. isTyping: {isTyping}, currentDialogueLines.Count: {currentDialogueLines.Count}");

        if (isTyping)
        {
            //DisplayFullCurrentSentence(); // Gọi hàm riêng đã có
            StopTypingEffect(); // Đảm bảo coroutine cũ được dừng
            dialogueText.text = currentFullSentence; // Hiển thị toàn bộ
            //Debug.Log($"{currentFullSentence}");
            isTyping = false;
            return;
        }


        if (currentDialogueLines.Count == 0)
        {
            Debug.LogWarning($"[DialogueManager] Dialogue queue is empty (index {_currentDialogueIndex}). Ending dialogue.");
            EndDialogue();
            MouseManager.Instance?.HideCursorAndEnableInput();
            return;

        }

        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            dialogueAudioSource.Stop();

        if (dialogueAudioSource != null && nextDialogueSound != null && _currentDialogueIndex != -1)
            dialogueAudioSource.PlayOneShot(nextDialogueSound);

        string lineToDisplay = currentDialogueLines.Dequeue();
        currentFullSentence = lineToDisplay;
        AudioClip voiceClip = currentVoiceClips.Dequeue();

        currentFullSentence = lineToDisplay;
        _currentDialogueIndex++;
        Debug.Log($"[DialogueManager] Dequeued sentence {_currentDialogueIndex}: '{lineToDisplay}' (remaining lines: {currentDialogueLines.Count})");

        dialogueText.text = "";
        dialogueText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeLine(lineToDisplay, voiceClip));
    }





    private IEnumerator TypeLine(string line, AudioClip voiceClip)
    {
        isTyping = true;
        currentFullSentence = line;

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        if (dialogueAudioSource != null && voiceClip != null)
        {
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.loop = false;
            dialogueAudioSource.Play();
            Debug.Log($"[DialogueManager] Playing voice clip: {voiceClip.name}");
        }
        else if (dialogueAudioSource != null && typewriterSound != null)
        {
            dialogueAudioSource.clip = typewriterSound;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();
        }

        dialogueText.text = "";

        Debug.Log($"[DialogueManager] Starting to type: {line}");

        foreach (char c in line)
        {
            dialogueText.text += c;
            dialogueText.ForceMeshUpdate();

            // Nếu user vừa nhấn tiếp, ta sẽ hiển thị toàn bộ luôn
            if (!isTyping)
            {
                Debug.Log("[DialogueManager] Typing interrupted – showing full sentence");
                dialogueText.text = line;
                break;
            }

            yield return new WaitForSeconds(letterDelay);
        }

        // Dừng âm thanh typewriter
        if (dialogueAudioSource != null && dialogueAudioSource.loop && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Stop();
        }

        isTyping = false;
        typingCoroutine = null;

        Debug.Log($"[DialogueManager] Finished typing sentence {_currentDialogueIndex}");
    }






    public void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    public void EndDialogue()
    {
        StopTypingEffect();
        MouseManager.Instance?.HideCursorAndEnableInput();
        dialogueUI?.SetActive(false);
        dialogueText.text = "";

        currentDialogueLines.Clear();
        currentVoiceClips.Clear();
        _originalDialogueKeys.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1;
        isDialogueActive = false;

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        var callback = _onDialogueEndCallback;
        _onDialogueEndCallback = null; // ✨ NGẮT callback trước khi gọi
        callback?.Invoke();            // 🛡️ Không còn gây vòng lặp
        Debug.Log("[DialogueManager] Dialogue ended");
    }

    public void DisplayFullCurrentSentence()
{
    if (typingCoroutine != null)
    {
        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    dialogueText.text = currentFullSentence;

    if (dialogueAudioSource != null && dialogueAudioSource.loop && dialogueAudioSource.isPlaying)
    {
        dialogueAudioSource.Stop(); // Dừng loop typewriter
    }

    isTyping = false;
}

}