using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public float Health { get; set; }
    public float MaxHealth { get; private set; }
    public Slider personalHealthSlider; 
    public Slider publicHealthSlider;   
    private void OnHealthChanged()
    {
        Debug.Log("Health changed to: " + Health);
        UpdateHealthUI();
    }

    private void Start()
    {
        MaxHealth = 100f;
        Health = MaxHealth;
        personalHealthSlider = GameObject.Find("Health").GetComponent<Slider>();
        if (personalHealthSlider != null)
        {
            personalHealthSlider.maxValue = MaxHealth;
            personalHealthSlider.value = MaxHealth;
        }

        if (publicHealthSlider != null)
        {
            publicHealthSlider.maxValue = MaxHealth;
            publicHealthSlider.value = MaxHealth;
        }
    }

    private void UpdateHealthUI()
    {
        //// Cập nhật slider cá nhân
        if (personalHealthSlider != null)
        {
            personalHealthSlider.value = Health;
        }

        // Cập nhật slider công khai
        if (publicHealthSlider != null)
        {
            publicHealthSlider.value = Health;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Bullet"))
    //    {
    //        Health -= 20;

    //        // Giới hạn giá trị Health để tránh âm
    //        Health = Mathf.Clamp(Health, 0, MaxHealth);
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Health -= 20;

            // Giới hạn giá trị Health để tránh âm
            Health = Mathf.Clamp(Health, 0, MaxHealth);

            Debug.Log("Bullet hit detected! Health: " + Health);
        }
    }
}
