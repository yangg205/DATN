using UnityEngine;
using UnityEngine.UI;

namespace AG
{
    public class UIEnemyHealthBar : MonoBehaviour
    {
        Slider slider;
        float timeUntilHealthBarIsHidden = 0;

        private void Awake()
        {
            slider = GetComponentInChildren<Slider>();
        }

        public void SetHealth(int health)
        {
            slider.value = health;
            timeUntilHealthBarIsHidden = 3;
        }

        public void SetMaxHealth(int maxHealth)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
        }

        private void Update()
        {
            timeUntilHealthBarIsHidden = timeUntilHealthBarIsHidden - Time.deltaTime;

            if(slider != null)
            {
                if (timeUntilHealthBarIsHidden <= 0)
                {
                    timeUntilHealthBarIsHidden = 0;
                    slider.gameObject.SetActive(false);
                }
                else
                {
                    if (!slider.gameObject.activeInHierarchy)
                    {
                        slider.gameObject.SetActive(true);
                    }
                }

                if (slider.value <= 0)
                {
                    Destroy(slider.gameObject);
                }
            }
        }
    }
}

