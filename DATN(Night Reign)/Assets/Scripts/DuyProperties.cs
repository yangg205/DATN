
using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DuyProperties : NetworkBehaviour
{
    public string name;
    public TextMeshProUGUI nameText;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    private float Health { get; set; }
    private float MaxHealth { get; set; }
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    // Các biến XP
    [Networked, OnChangedRender(nameof(OnXPChanged))]
    private float XP { get; set; }
    private float MaxXP { get; set; }
    public Slider xpSlider;
    public TextMeshProUGUI xpText;

    [Networked, OnChangedRender(nameof(OnLevelChanged))]
    private int Level { get; set; }

    public TextMeshProUGUI levelText;

    public NetworkObject networkObject;
    public NetworkRunner networkRunner;

    public Animator animator;
    public GameObject gameOverPanel;
    private TextMeshProUGUI gameOverText;

    private void OnHealthChanged()
    {
        float healthRatio = Health / MaxHealth;
        healthSlider.value = healthRatio;

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(Health)} / {Mathf.CeilToInt(MaxHealth)}";
        }
    }

    private void OnXPChanged()
    {
        float xpRatio = XP / MaxXP;
        xpSlider.value = xpRatio;

        if (xpText != null)
        {
            xpText.text = $"{Mathf.CeilToInt(XP)} / {Mathf.CeilToInt(MaxXP)}";
        }
    }

    private void OnLevelChanged()
    {
        if (levelText != null)
        {
            levelText.text = $"Lv. {Level}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Health -= 10;
            if (Health <= 0)
            {
                StartCoroutine(HandleDeath());
            }
        }
    }

    public void GainXP(float amount)
    {
        XP += amount;
        if (XP >= MaxXP)
        {
            XP = 0;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        MaxXP += 50; // Cứ mỗi cấp, tăng độ khó lên
    }

    private IEnumerator HandleDeath()
    {
        animator.SetTrigger("Die"); // Kích hoạt animation chết
        yield return new WaitForSeconds(2f); // Đợi animation kết thúc

        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true); // Hiện Panel Game Over
            if (gameOverText)
            {
                gameOverText.text = "Game Over"; // Cập nhật nội dung chữ
            }
        }

        networkRunner.Despawn(networkObject); // Xóa nhân vật
    }

    private void Start()
    {
        MaxHealth = 100;
        Health = MaxHealth;
        healthSlider.value = Health / MaxHealth;

        Level = 1;
        MaxXP = 100;
        XP = 0;

        xpSlider.value = XP / MaxXP;

        gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
            gameOverText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        OnLevelChanged(); // Khởi tạo hiển thị level
        OnXPChanged();    // Khởi tạo hiển thị XP
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            name = PlayerPrefs.GetString("PlayerName");
            nameText.text = name;
        }
    }
}
