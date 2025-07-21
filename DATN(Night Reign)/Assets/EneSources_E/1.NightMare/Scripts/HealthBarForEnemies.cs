using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarForEnemies : MonoBehaviour
{
    //public NightMare NightMare;

    public MonoBehaviour enemy;

    public Slider healthSlider;
    public Slider easeHealthSlider;

    public TextMeshProUGUI hpText;

    private float lerpSpeed = 0.05f;


    // Update is called once per frame
    void Update()
    {
        float currentHP = GetHP();

        if (healthSlider.value != currentHP)
        {
            healthSlider.value = currentHP;
        }

        if (easeHealthSlider.value != currentHP)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHP, lerpSpeed);
        }

        hpText.text = $"{currentHP}";

    }

    private float GetHP()
    {
        if (enemy is NightMare nm) return nm.HP;
        if (enemy is DragonSoulEater se) return se.HP;
        return 0;
    }

}