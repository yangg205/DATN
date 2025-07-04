using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Quest Reward UI")]
    public GameObject rewardPopup;
    public TextMeshProUGUI soulText;
    public TextMeshProUGUI expText;

    [Header("Quest Progress UI")]
    public TextMeshProUGUI questProgressText;

    [Header("Quest Notice Text (hiện trong Box)")]
    public TextMeshProUGUI questNoticeText; // <-- Gán Text UI trong Box vào đây

    private Coroutine noticeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ UIManager Instance đã sẵn sàng.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Có nhiều hơn 1 UIManager trong scene, đã hủy bản sao.");
        }
    }

    // 🟢 Hiển thị thông báo tạm trong Box (Text UI)
    public void ShowNotice(string message, float duration = 2.5f)
    {
        if (questNoticeText == null)
        {
            Debug.LogWarning("⚠️ Chưa gán questNoticeText trong UIManager!");
            return;
        }

        if (noticeCoroutine != null)
            StopCoroutine(noticeCoroutine);

        noticeCoroutine = StartCoroutine(ShowNoticeCoroutine(message, duration));
    }

    private IEnumerator ShowNoticeCoroutine(string message, float duration)
    {
        questNoticeText.text = message;
        yield return new WaitForSeconds(duration);
        questNoticeText.text = "";
    }

    // 🟢 Popup phần thưởng
    public void ShowRewardPopup(int soul, int exp)
    {
        if (rewardPopup == null || soulText == null || expText == null)
        {
            Debug.LogWarning("⚠️ Reward UI chưa được gán đầy đủ trong Inspector.");
            return;
        }

        soulText.text = $"Soul: {soul}";
        expText.text = $"EXP: {exp}";
        rewardPopup.SetActive(true);
        StartCoroutine(HideRewardPopupAfterDelay(2f));
    }

    private IEnumerator HideRewardPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rewardPopup.SetActive(false);
    }

    public void UpdateQuestProgress(int current, int total)
    {
        if (questProgressText != null)
            questProgressText.text = $"Tiến độ nhiệm vụ:\n{current}/{total}";
    }

    public void UpdateQuestProgressText(string progressMessage)
    {
        if (questProgressText != null)
            questProgressText.text = progressMessage;
    }

    public void HideQuestProgress()
    {
        if (questProgressText != null)
            questProgressText.text = "";
    }
}
