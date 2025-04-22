
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

        gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false); // Đảm bảo panel ẩn lúc đầu
            gameOverText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>(); // Tìm TextMeshPro bên trong Panel
        }
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
