using UnityEngine;
using TMPro;

public class PlayerStats_Tung : MonoBehaviour
{
    public int soul = 0;
    public int experience = 0;

    public TextMeshProUGUI soulText;
    public TextMeshProUGUI experienceText;

    void Start()
    {
        Debug.Log("▶️ PlayerStats_Tung.Start()");
        UpdateUI();
    }

    public void AddReward(int soulAmount, int expAmount)
    {
        soul += soulAmount;
        experience += expAmount;
        Debug.Log($"✅ Nhận thưởng: +{soulAmount} Soul, +{expAmount} EXP");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (soulText != null)
        {
            soulText.text = "Soul: " + soul;
            Debug.Log("🟣 Cập nhật soulText: " + soulText.text);
        }
        else
        {
            Debug.LogWarning("⚠️ soulText chưa được gán!");
        }

        if (experienceText != null)
        {
            experienceText.text = "EXP: " + experience;
            Debug.Log("🟡 Cập nhật experienceText: " + experienceText.text);
        }
        else
        {
            Debug.LogWarning("⚠️ experienceText chưa được gán!");
        }
    }
}
