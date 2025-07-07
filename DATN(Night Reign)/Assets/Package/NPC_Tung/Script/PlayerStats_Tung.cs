using UnityEngine;
using TMPro;

public class PlayerStats_Tung : MonoBehaviour
{
    public int coin = 0;
    public int experience = 0;

    public TextMeshProUGUI coinText;       // Đây chính là TextGold của bạn
    public TextMeshProUGUI experienceText; // Đây chính là ExpText của bạn

    void Awake()
    {
        // Debug.Log này rất hữu ích, giữ lại
        Debug.Log($"✨ PlayerStats_Tung Awake() trên GameObject: {gameObject.name}. " +
                  $"SoulText gán: {(coinText != null ? coinText.name : "NULL")}, " +
                  $"ExpText gán: {(experienceText != null ? experienceText.name : "NULL")}");
    }

    void Start()
    {
        Debug.Log("▶️ PlayerStats_Tung.Start()");
        UpdateUI(); // Cập nhật UI ngay khi bắt đầu game
    }

    public void AddReward(int soulAmount, int expAmount)
    {
        coin += soulAmount;
        experience += expAmount;
        Debug.Log($"✅ Nhận thưởng: +{soulAmount} Soul, +{expAmount} EXP. Tổng Soul: {coin}, Tổng EXP: {experience}");
        UpdateUI(); // Gọi hàm này để cập nhật Text của PlayerStats_Tung
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = "Soul: " + coin;
            Debug.Log("🟣 Cập nhật soulText: " + coinText.text);
        }
        else
        {
            Debug.LogWarning($"⚠️ soulText chưa được gán trên GameObject: {gameObject.name}! Không thể cập nhật UI.");
        }

        if (experienceText != null)
        {
            experienceText.text = "EXP: " + experience;
            Debug.Log("🟡 Cập nhật experienceText: " + experienceText.text);
        }
        else
        {
            Debug.LogWarning($"⚠️ experienceText chưa được gán trên GameObject: {gameObject.name}! Không thể cập nhật UI.");
        }
    }
}