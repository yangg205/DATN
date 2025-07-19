using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarForEnemies : MonoBehaviour
{
    public NightMare NightMare;

    public Slider healthSlider;
    public Slider easeHealthSlider;

    public TextMeshProUGUI hpText;

    private float lerpSpeed = 0.05f;


    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != NightMare.HP)
        {
            healthSlider.value = NightMare.HP;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, NightMare.HP, lerpSpeed);
        }

        hpText.text = $"{NightMare.HP}";

    }
}