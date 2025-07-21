using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings; // Vẫn cần UnityEngine.Localization.Locale cho kiểu dữ liệu
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public bool IsDialogueActive => isDialogueActive;

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip nextDialogueSound;

    // Lưu trữ các key gốc và voice clips để có thể tái tạo hàng đợi khi chuyển ngữ
    private List<string> _originalDialogueKeys;
    private List<AudioClip> _originalVoiceClipsEN;
    private List<AudioClip> _originalVoiceClipsVI;

    // Theo dõi chỉ số của câu thoại hiện tại trong danh sách gốc
    private int _currentDialogueIndex = -1;

    // Hàng đợi cho nội dung thoại và voice clips ĐÃ ĐƯỢC DỊCH
    private Queue<string> currentDialogueLines;
    private Queue<AudioClip> currentVoiceClips;

    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Action _onDialogueEndCallback;
    private string currentFullSentence; // Lưu trữ câu đầy đủ (đã localize) đang được gõ/hiển thị

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Nếu bạn muốn DialogueManager tồn tại giữa các cảnh, hãy uncomment dòng dưới
            // DontDestroyOnLoad(gameObject); 
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

        // Kiểm tra các thành phần UI đã được gán chưa
        if (dialogueUI == null) Debug.LogError("Dialogue UI (Panel) is not assigned in DialogueManager!");
        if (dialogueText == null) Debug.LogError("Dialogue Text (TextMeshProUGUI) is not assigned in DialogueManager!");

        // Đảm bảo có AudioSource
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("Dialogue Audio Source was not assigned. Automatically added one to DialogueManager GameObject.");
        }

        dialogueUI?.SetActive(false);
    }

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện thay đổi ngôn ngữ từ LocalizationManager của bạn
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChangedHandler;
        }
        else
        {
            Debug.LogWarning("LocalizationManager.Instance not found. Language change events will not be handled correctly.");
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi đối tượng bị vô hiệu hóa để tránh lỗi
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChangedHandler;
        }
    }

    // Phương thức này được gọi khi ngôn ngữ thay đổi
    // Sửa lại OnLanguageChangedHandler để thay đổi ngay lập tức
    private async void OnLanguageChangedHandler()
    {
        if (!isDialogueActive || _originalDialogueKeys.Count == 0 || _currentDialogueIndex < 0) return;

        StopTypingEffect();
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Stop();
        }

        // Lấy locale mới
        UnityEngine.Localization.Locale currentLocale = LocalizationManager.Instance.GetCurrentLocale();
        bool isVI = currentLocale.Identifier.Code.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

        // Dịch lại câu hiện tại ngay lập tức
        string currentKey = _originalDialogueKeys[_currentDialogueIndex];
        string localizedLine = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", currentKey);

        // Lấy voice clip mới
        AudioClip voiceClip = isVI ?
            (_currentDialogueIndex < _originalVoiceClipsVI.Count ? _originalVoiceClipsVI[_currentDialogueIndex] : null) :
            (_currentDialogueIndex < _originalVoiceClipsEN.Count ? _originalVoiceClipsEN[_currentDialogueIndex] : null);

        // Cập nhật ngay lập tức
        currentFullSentence = localizedLine;
        dialogueText.text = localizedLine;

        // Phát voice clip mới nếu có
        if (voiceClip != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.Play();
        }
    }
    // Bắt đầu một đoạn hội thoại mới
    public async void StartDialogue(string[] dialogueKeys, AudioClip[] voiceClipsEN, AudioClip[] voiceClipsVI, Action onDialogueEnd = null)
    {
        if (dialogueKeys == null || dialogueKeys.Length == 0)
        {
            EndDialogue();
            return;
        }

        StopTypingEffect();
        if (dialogueAudioSource != null) dialogueAudioSource.Stop();

        _originalDialogueKeys.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1; // Reset index khi bắt đầu thoại mới

        _originalDialogueKeys.AddRange(dialogueKeys);
        if (voiceClipsEN != null) _originalVoiceClipsEN.AddRange(voiceClipsEN);
        if (voiceClipsVI != null) _originalVoiceClipsVI.AddRange(voiceClipsVI);

        _onDialogueEndCallback = onDialogueEnd;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = "";

        // Lấy locale hiện tại thông qua phương thức GetCurrentLocale() của LocalizationManager của bạn
        UnityEngine.Localization.Locale currentLocale = LocalizationManager.Instance.GetCurrentLocale();
        // Điền hàng đợi lần đầu tiên với ngôn ngữ hiện tại
        await RePopulateQueuesFromOriginalData(currentLocale);

        DisplayNextSentence();
    }

    // Tái tạo hàng đợi thoại và voice clips từ dữ liệu gốc, sử dụng ngôn ngữ đích
    private async Task RePopulateQueuesFromOriginalData(UnityEngine.Localization.Locale targetLocale)
    {
        currentDialogueLines.Clear();
        currentVoiceClips.Clear();

        // Xác định xem ngôn ngữ đích có phải là tiếng Việt không
        bool isVI = targetLocale.Identifier.Code.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

        for (int i = 0; i < _originalDialogueKeys.Count; i++)
        {
            // GỌI HÀM CỦA BẠN: Sử dụng LocalizationManager.Instance.GetLocalizedStringAsync
            // để lấy chuỗi đã được dịch từ String Table
            string localizedLine = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", _originalDialogueKeys[i]);
            currentDialogueLines.Enqueue(localizedLine);

            AudioClip voiceClip = GetVoiceClipForIndex(i, isVI);
            currentVoiceClips.Enqueue(voiceClip);
        }
    }

    // Lấy voice clip tương ứng với ngôn ngữ và chỉ số
    private AudioClip GetVoiceClipForIndex(int index, bool isVI)
    {
        List<AudioClip> targetVoiceList = isVI ? _originalVoiceClipsVI : _originalVoiceClipsEN;

        if (index >= 0 && index < targetVoiceList.Count)
        {
            return targetVoiceList[index];
        }
        return null;
    }

    // Hiển thị câu thoại tiếp theo
    public void DisplayNextSentence()
    {
        if (isTyping) // Nếu đang gõ, hiển thị toàn bộ câu ngay lập tức
        {
            StopTypingEffect();
            dialogueText.text = currentFullSentence;

            if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            {
                dialogueAudioSource.Stop();
            }

            isTyping = false;
            return;
        }

        if (currentDialogueLines.Count == 0) // Hết thoại
        {
            EndDialogue();
            return;
        }

        // Dừng bất kỳ voice clip nào đang phát trước khi chuyển câu mới
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Stop();
        }

        // Phát âm thanh chuyển câu nếu không phải câu đầu tiên
        if (dialogueAudioSource != null && nextDialogueSound != null && _currentDialogueIndex != -1)
        {
            dialogueAudioSource.PlayOneShot(nextDialogueSound);
        }

        // Lấy câu thoại và voice clip tiếp theo từ hàng đợi (loại bỏ khỏi hàng đợi)
        string lineToDisplay = currentDialogueLines.Dequeue();
        AudioClip voiceClip = currentVoiceClips.Dequeue();

        currentFullSentence = lineToDisplay; // Cập nhật câu đầy đủ hiện tại
        _currentDialogueIndex++; // Tăng chỉ số sau khi dequeue

        // Bắt đầu hiệu ứng gõ chữ cho câu thoại mới
        typingCoroutine = StartCoroutine(TypeLine(lineToDisplay, voiceClip));
    }

    // Coroutine để gõ từng chữ một và phát âm thanh
    private IEnumerator TypeLine(string line, AudioClip voiceClip)
    {
        isTyping = true;
        dialogueText.text = ""; // Xóa text hiện tại trước khi gõ lại

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop(); // Đảm bảo không có âm thanh nào đang chạy
        }

        if (dialogueAudioSource != null && voiceClip != null)
        {
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.loop = false;
            dialogueAudioSource.Play();

            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }
        }
        else if (dialogueAudioSource != null && typewriterSound != null)
        {
            dialogueAudioSource.clip = typewriterSound;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();

            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }

            // Dừng âm thanh máy đánh chữ khi hoàn tất gõ (nếu không có voice clip)
            if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            {
                dialogueAudioSource.Stop();
            }
        }
        else // Trường hợp không có cả voice clip và typewriter sound
        {
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(letterDelay);
            }
        }

        isTyping = false;
    }

    // Dừng hiệu ứng gõ chữ
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    // Kết thúc đoạn hội thoại
    public void EndDialogue()
    {
        StopTypingEffect();

        dialogueUI?.SetActive(false);
        dialogueText.text = "";

        // Xóa tất cả dữ liệu thoại và reset trạng thái
        currentDialogueLines.Clear();
        currentVoiceClips.Clear();
        _originalDialogueKeys.Clear();
        _originalVoiceClipsEN.Clear();
        _originalVoiceClipsVI.Clear();
        _currentDialogueIndex = -1; // Reset index
        isDialogueActive = false;

        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }

        // Kích hoạt callback nếu có và xóa nó
        _onDialogueEndCallback?.Invoke();
        _onDialogueEndCallback = null;
    }
}