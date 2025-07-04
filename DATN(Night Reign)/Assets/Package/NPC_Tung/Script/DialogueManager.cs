using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton;

    [Header("Typewriter Settings")]
    public float letterDelay = 0.03f;

    private string[] currentDialogueLines;
    private int currentLineIndex;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    // 🟢 Gọi hàm này để bắt đầu đoạn hội thoại
    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("⚠️ Không có đoạn thoại nào để hiển thị.");
            return;
        }

        currentDialogueLines = lines;
        currentLineIndex = 0;

        // Hiển thị UI
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = ""; // Reset text

        ShowLine();

        if (nextButton != null)
            nextButton.SetActive(true);
    }

    // 🟢 Hiển thị dòng thoại hiện tại
    public void ShowLine()
    {
        if (!isDialogueActive || currentDialogueLines == null || currentDialogueLines.Length == 0)
            return;

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, currentDialogueLines.Length - 1);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        string lineToDisplay = currentDialogueLines[currentLineIndex];
        typingCoroutine = StartCoroutine(TypeLine(lineToDisplay));
    }

    // 🟢 Hiệu ứng gõ từng chữ
    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(letterDelay);
        }

        typingCoroutine = null; // Đánh dấu là đã gõ xong
    }

    // 🟢 Gọi từ nút "Tiếp" để chuyển thoại
    public void NextLine()
    {
        if (!isDialogueActive || currentDialogueLines == null || currentDialogueLines.Length == 0)
            return;

        // Nếu đang gõ chữ thì hiển thị toàn bộ ngay lập tức
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;

            dialogueText.text = currentDialogueLines[currentLineIndex];
            return;
        }

        // Nếu còn thoại thì chuyển dòng
        if (currentLineIndex < currentDialogueLines.Length - 1)
        {
            currentLineIndex++;
            ShowLine();
        }
        else
        {
            Debug.Log("📜 Hết đoạn thoại rồi, đóng hộp thoại.");
            EndDialogue();
        }
    }

    // 🟢 Kết thúc đoạn thoại và ẩn UI
    public void EndDialogue()
    {
        isDialogueActive = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);

        dialogueText.text = "";
        currentDialogueLines = null;
        currentLineIndex = 0;
    }
}
