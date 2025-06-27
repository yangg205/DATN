using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Quest Reward UI")]
    public GameObject rewardPopup;
    public TextMeshProUGUI soulText;
    public TextMeshProUGUI expText;

    [Header("Quest Progress UI")]
    public TextMeshProUGUI questProgressText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowRewardPopup(int soulAmount, int expAmount)
    {
        soulText.text = $"+{soulAmount} Soul";
        expText.text = $"+{expAmount} EXP";

        rewardPopup.SetActive(true);
        CancelInvoke(nameof(HideRewardPopup));
        Invoke(nameof(HideRewardPopup), 2.5f);
    }

    private void HideRewardPopup()
    {
        rewardPopup.SetActive(false);
    }

    public void UpdateQuestProgress(int current, int total)
    {
        if (questProgressText != null)
            questProgressText.text = $"Tiến độ nhiệm vụ: {current}/{total}";
    }

    public void UpdateQuestProgressText(string customText)
    {
        if (questProgressText != null)
            questProgressText.text = customText;
    }

    public void HideQuestProgress()
    {
        if (questProgressText != null)
            questProgressText.text = "";
    }
}
