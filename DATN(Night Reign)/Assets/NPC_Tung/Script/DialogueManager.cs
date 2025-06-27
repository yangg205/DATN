using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton;

    private string[] dialogueLines;
    private int currentLineIndex;
    private bool isDialogueActive = false;

    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("⚠️ Không có đoạn thoại nào để hiển thị.");
            return;
        }

        dialogueLines = lines;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        ShowLine();

        if (nextButton != null)
            nextButton.SetActive(true);
    }

    public void ShowLine()
    {
        if (!isDialogueActive || dialogueLines == null || dialogueLines.Length == 0)
            return;

        // Chặn index bị vượt quá
        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, dialogueLines.Length - 1);

        dialogueText.text = dialogueLines[currentLineIndex];
        Debug.Log(dialogueLines[currentLineIndex]);

        if (nextButton != null)
            nextButton.SetActive(true);
    }

    public void NextLine()
    {
        if (!isDialogueActive || dialogueLines == null || dialogueLines.Length == 0)
            return;

        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            ShowLine();
        }
        else
        {
            // ✅ Đã đến dòng cuối → vẫn giữ nguyên giao diện
            Debug.Log("📜 Hết đoạn thoại rồi, không chuyển tiếp nữa.");
            // Không làm gì cả — vẫn giữ nguyên dialogueUI và nút tiếp tục
        }
    }

    public void EndDialogue()
    {
        // Gọi thủ công từ bên ngoài nếu muốn đóng
        isDialogueActive = false;
        dialogueUI.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);
    }
}
