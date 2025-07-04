using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public EnemyAIByYang enemyAIByYang;

    public Slider healthSlider;
    public Slider easeHealthSlider;

    public Image fillImage;
    public Color normalColor = Color.red;
    public Color enragedColor = new Color(0.6f, 0f, 0.8f); 



    private float lerpSpeed = 0.05f;


    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != enemyAIByYang.currentHealth)
        {
            healthSlider.value = enemyAIByYang.currentHealth;
        }

        if(healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, enemyAIByYang.currentHealth, lerpSpeed);
        }

        // Đổi màu nếu Enraged
        if (enemyAIByYang != null && fillImage != null)
        {
            if (enemyAIByYang.IsEnraged)
            {
                fillImage.color = enragedColor;
            }
            else
            {
                fillImage.color = normalColor;
            }
        }

    }
}
