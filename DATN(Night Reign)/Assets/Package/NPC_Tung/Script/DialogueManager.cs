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
    public TextMeshProUGUI npcNameText; // New field for NPC name display
    public bool IsDialogueActive => isDialogueActive;
    private bool hasInterruptedTyping = false;

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip nextDialogueSound;

    [Header("Default Dialogue Fallback (Optional)")]
    [SerializeField] private string[] defaultDialogueKeys;
    [SerializeField] private AudioClip[] defaultVoiceClipsEN;
    [SerializeField] private AudioClip[] defaultVoiceClipsVI;

    private List<string> _originalDialogueKeys;
    private List<string> _originalNPCIds; // Store NPC IDs for each dialogue line
    private List<string> _originalNPCNames; // Store localized NPC names for each dialogue line
    private List<AudioClip> _originalVoiceClipsEN;
    private List<AudioClip> _originalVoiceClipsVI;
    private int _currentDialogueIndex = -1;
    private Queue<string> currentDialogueLines;
    private Queue<AudioClip> currentVoiceClips;
    private Queue<string> currentNPCNames; // Queue for NPC names per line
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Action _onDialogueEndCallback;
    private string currentFullSentence;
    private QuestData currentQuestData;
    public bool IsTyping => isTyping;

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
        currentNPCNames = new Queue<string>(); // Initialize NPC names queue
        _originalDialogueKeys = new List<string>();
        _originalNPCIds = new List<string>();
        _originalNPCNames = new List<string>(); // Initialize NPC names list
        _originalVoiceClipsEN = new List<AudioClip>();
        _originalVoiceClipsVI = new List<AudioClip>();

        if (dialogueUI == null) Debug.LogError("[DialogueManager] Dialogue UI (Panel) is not assigned!");
        if (dialogueText == null) Debug.LogError("[DialogueManager] Dialogue Text (TextMeshProUGUI) is not assigned!");
        if (npcNameText == null) Debug.LogError("[DialogueManager] NPC Name Text (TextMeshProUGUI) is not assigned!");

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

    public async void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType, Action onDialogueEnd = null, string[] keys = null, AudioClip[] voiceClipsEN = null, AudioClip[] voiceClipsVI = null, string[] npcIds = null)
    {
        currentQuestData = quest;
        string[] dialogueKeys = keys ?? (quest != null ? quest.GetDialogueKeys(dialogueType) : defaultDialogueKeys);
        string[] dialogueNPCIds = npcIds ?? (quest != null ? quest.GetDialogueNPCIds(dialogueType) : Array.Empty<string>());
        AudioClip[] clipsEN = voiceClipsEN ?? (quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_EN,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_EN,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_EN,
            QuestDialogueType.InProgress => Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>(),
        } : defaultVoiceClipsEN);
        AudioClip[] clipsVI = voiceClipsVI ?? (quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_VI,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_VI,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_VI,
            QuestDialogueType.InProgress => Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>(),
        } : defaultVoiceClipsVI);

        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            Debug.LogWarning($"[DialogueManager] No dialogue keys provided for {dialogueType} in QuestData: {(quest != null ? quest.questName : "null")}. Using defaultDialogueKeys.");
            dialogueKeys = defaultDialogueKeys;
            dialogueNPCIds = new string[dialogueKeys.Length]; // Use empty NPC IDs for default keys
        }

        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            Debug.LogError($"[DialogueManager] No valid dialogue keys available for {dialogueType}. Ending dialogue.");
            EndDialogue();
            onDialogueEnd?.Invoke();
            return;
        }

        // Validate NPC IDs array length
        if (dialogueNPCIds.Length > 0 && dialogueNPCIds.Length != dialogueKeys.Length)
        {
            Debug.LogWarning($"[DialogueManager] NPC IDs array length ({dialogueNPCIds.Length}) does not match dialogue keys length ({dialogueKeys.Length}). Using giverNPCID as fallback.");
            dialogueNPCIds = new string[dialogueKeys.Length];
            for (int i = 0; i < dialogueNPCIds.Length; i++)
            {
                dialogueNPCIds[i] = quest?.giverNPCID ?? "";
            }
        }

        Debug.Log($"[DialogueManager] Starting dialogue for {dialogueType} with {dialogueKeys.Length} keys: {string.Join(", ", dialogueKeys)}");
        if (dialogueKeys.Length <= 1)
        {
            Debug.LogWarning($"[DialogueManager] Only {dialogueKeys.Length} dialogue key(s) provided. Consider adding more for longer dialogue.");
        }

        StopTypingEffect();
        if (dialogueAudioSource != null) dialogueAudioSource.Stop();

        _originalDialogueKeys.Clear();
        _originalNPCIds.Clear();
        _originalNPCNames.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1;

        _originalDialogueKeys.AddRange(dialogueKeys);
        _originalNPCIds.AddRange(dialogueNPCIds);
        if (clipsEN != null) _originalVoiceClipsEN.AddRange(clipsEN);
        if (clipsVI != null) _originalVoiceClipsVI.AddRange(clipsVI);

        _onDialogueEndCallback = onDialogueEnd;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";
        npcNameText.text = ""; // Clear NPC name text

        var currentLocale = LocalizationManager.Instance.GetCurrentLocale();
        await RePopulateQueuesFromOriginalData(currentLocale);
        if (currentDialogueLines.Count == 0)
        {
            Debug.LogError($"[DialogueManager] Failed to populate dialogue queue for {dialogueType}. Ending dialogue.");
            EndDialogue();
            onDialogueEnd?.Invoke();
            return;
        }

        Debug.Log($"[DialogueManager] Dialogue queue populated with {currentDialogueLines.Count} lines. Starting first sentence.");
        DisplayNextSentence();
    }

    private async Task RePopulateQueuesFromOriginalData(UnityEngine.Localization.Locale targetLocale, int startIndex = 0)
    {
        currentDialogueLines.Clear();
        currentVoiceClips.Clear();
        currentNPCNames.Clear();
        _originalNPCNames.Clear();

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
                    Debug.LogWarning($"[DialogueManager] Localized string for key {key} is empty or null, using fallback: [Missing: {key}]");
                    localizedLine = $"[Missing: {key}]";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Error localizing key {key}: {ex.Message}");
            }

            // Get the NPC ID for this line, fall back to giverNPCID if not specified
            string npcId = i < _originalNPCIds.Count && !string.IsNullOrEmpty(_originalNPCIds[i]) ? _originalNPCIds[i] : (currentQuestData?.giverNPCID ?? "");
            string npcName = currentQuestData != null ? await currentQuestData.GetNPCNameLocalizedAsync(npcId) : "NPC";
            _originalNPCNames.Add(npcName); // Store localized NPC name

            currentDialogueLines.Enqueue(localizedLine); // Only enqueue the dialogue line, no NPC name prefix
            currentNPCNames.Enqueue(npcName); // Enqueue NPC name separately
            Debug.Log($"[DialogueManager] Enqueued line {i}: {localizedLine} (NPC: {npcName})");

            AudioClip voiceClip = GetVoiceClipForIndex(i, isVI);
            currentVoiceClips.Enqueue(voiceClip ?? null);
            Debug.Log($"[DialogueManager] Enqueued voice clip {i}: {(voiceClip != null ? voiceClip.name : "null")}");
        }

        Debug.Log($"[DialogueManager] Repopulated dialogue queue with {currentDialogueLines.Count} lines, voice clips: {currentVoiceClips.Count}, NPC names: {currentNPCNames.Count}");
        if (currentDialogueLines.Count == 0)
        {
            Debug.LogError("[DialogueManager] No lines enqueued! Check QuestData dialogue keys or localization table 'NhiemVu'.");
        }
        else if (currentDialogueLines.Count != _originalDialogueKeys.Count - startIndex)
        {
            Debug.LogError($"[DialogueManager] Mismatch in dialogue lines count. Expected: {_originalDialogueKeys.Count - startIndex}, Got: {currentDialogueLines.Count}");
        }
        if (currentVoiceClips.Count != currentDialogueLines.Count)
        {
            Debug.LogError($"[DialogueManager] Mismatch in voice clips count. Dialogue lines: {currentDialogueLines.Count}, Voice clips: {currentVoiceClips.Count}");
        }
        if (currentNPCNames.Count != currentDialogueLines.Count)
        {
            Debug.LogError($"[DialogueManager] Mismatch in NPC names count. Dialogue lines: {currentDialogueLines.Count}, NPC names: {currentNPCNames.Count}");
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
        Debug.Log($"[DialogueManager] DisplayNextSentence called. isTyping: {isTyping}, currentDialogueLines.Count: {currentDialogueLines.Count}, _currentDialogueIndex: {_currentDialogueIndex}");

        if (isTyping)
        {
            Debug.Log("[DialogueManager] Still typing, stopping typing effect and showing full sentence");
            StopTypingEffect();
            dialogueText.text = currentFullSentence;
            dialogueText.ForceMeshUpdate();
            isTyping = false;
            return;
        }

        if (currentDialogueLines.Count == 0)
        {
            Debug.LogWarning($"[DialogueManager] Dialogue queue is empty (index {_currentDialogueIndex}). Ending dialogue.");
            EndDialogue();
            return;
        }

        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            dialogueAudioSource.Stop();

        if (dialogueAudioSource != null && nextDialogueSound != null && _currentDialogueIndex >= 0)
            dialogueAudioSource.PlayOneShot(nextDialogueSound);

        string lineToDisplay = currentDialogueLines.Dequeue();
        string npcName = currentNPCNames.Dequeue(); // Dequeue NPC name
        AudioClip voiceClip = currentVoiceClips.Count > 0 ? currentVoiceClips.Dequeue() : null;

        currentFullSentence = lineToDisplay;
        _currentDialogueIndex++;
        Debug.Log($"[DialogueManager] Dequeued sentence {_currentDialogueIndex}: '{lineToDisplay}' (NPC: {npcName}, remaining lines: {currentDialogueLines.Count}, remaining clips: {currentVoiceClips.Count})");

        dialogueText.text = "";
        npcNameText.text = npcName; // Update NPC name text
        dialogueText.ForceMeshUpdate();
        npcNameText.ForceMeshUpdate();

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
            if (!isTyping)
            {
                Debug.Log("[DialogueManager] Typing interrupted – showing full sentence");
                dialogueText.text = line;
                dialogueText.ForceMeshUpdate();
                break;
            }

            dialogueText.text += c;
            dialogueText.ForceMeshUpdate();
            yield return new WaitForSeconds(letterDelay);
        }

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
        Debug.Log($"[DialogueManager] EndDialogue called. Current lines remaining: {currentDialogueLines.Count}, _currentDialogueIndex: {_currentDialogueIndex}, isDialogueActive: {isDialogueActive}");

        StopTypingEffect();
        dialogueUI?.SetActive(false);
        dialogueText.text = "";
        npcNameText.text = ""; // Clear NPC name text

        currentDialogueLines.Clear();
        currentVoiceClips.Clear();
        currentNPCNames.Clear();
        _originalDialogueKeys.Clear();
        _originalNPCIds.Clear();
        _originalNPCNames.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1;
        isDialogueActive = false;
        currentQuestData = null;

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        var callback = _onDialogueEndCallback;
        _onDialogueEndCallback = null;
        callback?.Invoke();
        Debug.Log("[DialogueManager] Dialogue ended");
        MouseManager.Instance.HideCursorAndEnableInput();
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
            dialogueAudioSource.Stop();
        }

        isTyping = false;
    }
}