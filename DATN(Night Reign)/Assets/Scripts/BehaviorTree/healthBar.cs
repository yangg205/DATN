using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public EnemyAIByYang enemyAIByYang;

    public Slider healthSlider;
    public Slider easeHealthSlider;

    private float lerpSpeed = 0.05f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

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
    }
}
