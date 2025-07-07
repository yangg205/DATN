using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI; // Để sử dụng Button

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueUI; // Panel chứa toàn bộ UI thoại
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText; // Trường cho tên người nói
    public GameObject nextButton; // Nút để chuyển câu thoại

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f; // Tốc độ gõ chữ

    [Header("Audio Settings")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip nextDialogueSound;

    private Queue<string> currentDialogueLines; // Hàng đợi cho nội dung thoại đã localize
    private Queue<string> currentSpeakerNames; // Hàng đợi cho tên người nói đã localize
    private Queue<AudioClip> currentVoiceClips; // Hàng đợi cho voice clips

    private bool isDialogueActive = false;
    private bool isTyping = false; // Cờ để kiểm tra đang gõ chữ
    private Coroutine typingCoroutine;
    private Action _onDialogueEndCallback;
    private string currentFullSentence; // Lưu trữ câu đầy đủ để hiển thị ngay lập tức

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Giữ nếu bạn muốn nó tồn tại qua các cảnh
        }
        else
        {
            Destroy(gameObject);
        }

        // Khởi tạo các hàng đợi
        currentDialogueLines = new Queue<string>();
        currentSpeakerNames = new Queue<string>();
        currentVoiceClips = new Queue<AudioClip>();

        // Kiểm tra các tham chiếu UI
        if (dialogueUI == null) Debug.LogError("Dialogue UI (Panel) is not assigned in DialogueManager!");
        if (dialogueText == null) Debug.LogError("Dialogue Text (TextMeshProUGUI) is not assigned in DialogueManager!");
        if (speakerNameText == null) Debug.LogError("Speaker Name Text (TextMeshProUGUI) is not assigned in DialogueManager!");
        if (nextButton == null) Debug.LogError("Next Button (GameObject) is not assigned in DialogueManager!");

        // Kiểm tra và tự động thêm AudioSource nếu chưa có
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("Dialogue Audio Source was not assigned. Automatically added one to DialogueManager GameObject.");
        }

        // Gán sự kiện click cho nút Next/Continue
        Button nextBtnComponent = nextButton?.GetComponent<Button>();
        if (nextBtnComponent != null)
        {
            nextBtnComponent.onClick.AddListener(DisplayNextSentence);
        }
        else
        {
            Debug.LogError("Next Button does not have a Button component!");
        }

        dialogueUI?.SetActive(false);
        nextButton?.SetActive(false);
    }

    // Hàm StartDialogue mới, nhận cả keys, speaker names và voice clips
    public void StartDialogue(string[] dialogueKeys, string[] speakerNameKeys, AudioClip[] voiceClips, Action onDialogueEnd = null)
    {
        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            EndDialogue();
            return;
        }

        currentDialogueLines.Clear();
        currentSpeakerNames.Clear();
        currentVoiceClips.Clear();

        // Đổ dữ liệu vào hàng đợi
        for (int i = 0; i < dialogueKeys.Length; i++)
        {
            string localizedLine = GetLocalizedDialogueString(dialogueKeys[i]);
            string localizedSpeaker = (speakerNameKeys != null && i < speakerNameKeys.Length) ? GetLocalizedDialogueString(speakerNameKeys[i]) : "NPC"; // Mặc định là "NPC" nếu không có tên người nói
            AudioClip voiceClip = (voiceClips != null && i < voiceClips.Length) ? voiceClips[i] : null;

            currentDialogueLines.Enqueue(localizedLine);
            currentSpeakerNames.Enqueue(localizedSpeaker);
            currentVoiceClips.Enqueue(voiceClip);
        }

        _onDialogueEndCallback = onDialogueEnd;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";
        speakerNameText.text = ""; // Xóa tên người nói cũ
        nextButton?.SetActive(true);

        DisplayNextSentence(); // Bắt đầu hiển thị câu thoại đầu tiên
    }

    // Hàm thay thế cho NextLine() và ShowLine() của bạn
    public void DisplayNextSentence()
    {
        // Nếu đang gõ chữ, dừng gõ và hiển thị toàn bộ câu ngay lập tức
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            dialogueText.text = currentFullSentence;
            isTyping = false;
            // Dừng âm thanh gõ máy chữ nếu nó đang phát
            if (dialogueAudioSource != null && dialogueAudioSource.isPlaying && dialogueAudioSource.clip == typewriterSound)
            {
                dialogueAudioSource.Stop();
            }
            return; // Thoát để không chuyển sang câu tiếp theo ngay lập tức
        }

        // Nếu hết câu thoại
        if (currentDialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Phát âm thanh chuyển câu (nếu không phải câu đầu tiên)
        if (dialogueAudioSource != null && nextDialogueSound != null && !string.IsNullOrEmpty(dialogueText.text))
        {
            dialogueAudioSource.PlayOneShot(nextDialogueSound);
        }

        // Lấy câu thoại, tên người nói và voice clip tiếp theo
        string lineToDisplay = currentDialogueLines.Dequeue();
        string speakerName = currentSpeakerNames.Dequeue();
        AudioClip voiceClip = currentVoiceClips.Dequeue();

        speakerNameText.text = speakerName; // Cập nhật tên người nói
        typingCoroutine = StartCoroutine(TypeLine(lineToDisplay, voiceClip)); // Bắt đầu gõ chữ và phát voice
    }

    private IEnumerator TypeLine(string line, AudioClip voiceClip)
    {
        isTyping = true;
        currentFullSentence = line; // Lưu trữ câu đầy đủ
        dialogueText.text = ""; // Xóa text hiện tại

        // Dừng mọi âm thanh đang phát trên AudioSource
        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        // Phát lồng tiếng (nếu có)
        if (dialogueAudioSource != null && voiceClip != null)
        {
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.loop = false;
            dialogueAudioSource.Play();
        }
        // Nếu không có lồng tiếng, phát âm thanh gõ máy chữ
        else if (dialogueAudioSource != null && typewriterSound != null)
        {
            dialogueAudioSource.clip = typewriterSound;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();
        }

        // Hiệu ứng gõ chữ
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
        // Dừng âm thanh gõ máy chữ khi gõ xong (nếu nó đang phát)
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying && dialogueAudioSource.clip == typewriterSound)
        {
            dialogueAudioSource.Stop();
        }
    }

    public void EndDialogue()
    {
        isTyping = false;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueUI?.SetActive(false);
        nextButton?.SetActive(false);
        dialogueText.text = "";
        speakerNameText.text = ""; // Xóa tên người nói khi kết thúc
        currentDialogueLines.Clear(); // Xóa hàng đợi
        currentSpeakerNames.Clear();
        currentVoiceClips.Clear();

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop(); // Đảm bảo dừng mọi âm thanh
        }

        _onDialogueEndCallback?.Invoke(); // Gọi callback
        _onDialogueEndCallback = null;
    }

    private string GetLocalizedDialogueString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "[EMPTY KEY]";

        key = key.Trim();

        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable("NhiemVu"); // Đã sửa thành "NhiemVu"
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