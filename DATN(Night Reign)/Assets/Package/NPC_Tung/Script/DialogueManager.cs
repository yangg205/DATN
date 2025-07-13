using UnityEngine;
using TMPro;
using System.Collections;
using System; // For Action
using System.Collections.Generic; // Thêm để dùng Queue
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI; // Để sử dụng Button

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueUI; // Panel chứa toàn bộ UI thoại
    public TextMeshProUGUI dialogueText;
    //public TextMeshProUGUI speakerNameText; // Trường cho tên người nói (vẫn giữ để hiển thị giá trị cố định)
    public GameObject nextButton; // Nút để chuyển câu thoại

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f; // Tốc độ gõ chữ

    [Header("Audio Settings")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip nextDialogueSound;

    private Queue<string> currentDialogueLines; // Hàng đợi cho nội dung thoại đã localize
    private Queue<AudioClip> currentVoiceClips; // Hàng đợi cho voice clips

    private bool isDialogueActive = false;
    private bool isTyping = false; // Cờ để kiểm tra đang gõ chữ
    private Coroutine typingCoroutine;
    private Action _onDialogueEndCallback;
    private string currentFullSentence; // Lưu trữ câu đầy đủ (vẫn giữ nhưng không dùng để skip)

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
        currentVoiceClips = new Queue<AudioClip>();

        // Kiểm tra các tham chiếu UI
        if (dialogueUI == null) Debug.LogError("Dialogue UI (Panel) is not assigned in DialogueManager!");
        if (dialogueText == null) Debug.LogError("Dialogue Text (TextMeshProUGUI) is not assigned in DialogueManager!");
        if (nextButton == null) Debug.LogError("Next Button (GameObject) is not assigned!");

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

    // Hàm StartDialogue mới, KHÔNG NHẬN speakerNameKeys
    public void StartDialogue(string[] dialogueKeys, AudioClip[] voiceClips, Action onDialogueEnd = null)
    {
        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            EndDialogue();
            return;
        }

        currentDialogueLines.Clear();
        currentVoiceClips.Clear();

        // Đổ dữ liệu vào hàng đợi
        for (int i = 0; i < dialogueKeys.Length; i++)
        {
            string localizedLine = GetLocalizedDialogueString(dialogueKeys[i]);
            AudioClip voiceClip = (voiceClips != null && i < voiceClips.Length) ? voiceClips[i] : null;

            currentDialogueLines.Enqueue(localizedLine);
            currentVoiceClips.Enqueue(voiceClip);
        }

        _onDialogueEndCallback = onDialogueEnd;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";
        nextButton?.SetActive(true);

        DisplayNextSentence(); // Bắt đầu hiển thị câu thoại đầu tiên
    }

    // Hàm DisplayNextSentence đã sửa đổi: Bất cứ khi nào chuyển câu, đều gõ chữ
    public void DisplayNextSentence()
    {
        // 1. Nếu đang gõ chữ (typingCoroutine đang chạy), KHÔNG LÀM GÌ CẢ.
        // Người dùng phải đợi hiệu ứng gõ chữ hoàn tất tự nhiên.
        if (isTyping)
        {
            return; // Thoát khỏi hàm, bỏ qua input này
        }

        // 2. Nếu đã hết câu thoại trong hàng đợi (và không đang gõ chữ)
        if (currentDialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // 3. Nếu không đang gõ chữ và còn câu thoại, chuyển sang câu tiếp theo và BẮT ĐẦU GÕ CHỮ

        // Phát âm thanh chuyển câu (chỉ khi có câu thoại trước đó, tức là không phải câu đầu tiên)
        // Kiểm tra !string.IsNullOrEmpty(dialogueText.text) để tránh phát âm thanh khi bắt đầu dialogue trống rỗng
        if (dialogueAudioSource != null && nextDialogueSound != null && !string.IsNullOrEmpty(dialogueText.text))
        {
            dialogueAudioSource.PlayOneShot(nextDialogueSound);
        }

        // Lấy câu thoại và voice clip tiếp theo
        string lineToDisplay = currentDialogueLines.Dequeue();
        AudioClip voiceClip = currentVoiceClips.Dequeue();

        currentFullSentence = lineToDisplay; // Vẫn lưu trữ nhưng không dùng để skip

        // BẮT ĐẦU HIỆU ỨNG GÕ CHỮ CHO CÂU THOẠI MỚI
        typingCoroutine = StartCoroutine(TypeLine(lineToDisplay, voiceClip));
    }


    private IEnumerator TypeLine(string line, AudioClip voiceClip)
    {
        isTyping = true;
        currentFullSentence = line; // Lưu trữ câu đầy đủ
        dialogueText.text = ""; // Xóa text hiện tại để bắt đầu gõ lại

        // Dừng mọi âm thanh đang phát trên AudioSource để tránh chồng chéo
        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        // Ưu tiên phát lồng tiếng (nếu có)
        if (dialogueAudioSource != null && voiceClip != null)
        {
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.loop = false; // Lồng tiếng không lặp
            dialogueAudioSource.Play();

            // Chạy hiệu ứng gõ chữ ngay cả khi có lồng tiếng
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }
        }
        // Nếu không có lồng tiếng, phát âm thanh gõ máy chữ (nếu có)
        else if (dialogueAudioSource != null && typewriterSound != null)
        {
            dialogueAudioSource.clip = typewriterSound;
            dialogueAudioSource.loop = true; // Âm thanh gõ máy chữ lặp lại
            dialogueAudioSource.Play();

            // Hiệu ứng gõ chữ
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }

            // Dừng âm thanh gõ máy chữ khi gõ xong
            if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            {
                dialogueAudioSource.Stop();
            }
        }
        else // Không có cả voice clip lẫn typewriter sound, vẫn gõ chữ (nhưng không có âm thanh)
        {
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }
        }

        isTyping = false; // Đảm bảo cờ isTyping là false sau khi gõ xong
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
        currentDialogueLines.Clear(); // Xóa hàng đợi
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

        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable("NhiemVu");
        if (stringTable == null)
        {
            Debug.LogError($"Localization StringTable 'NhiemVu' not found! (Check Package Manager & Localization Settings for key: {key})");
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