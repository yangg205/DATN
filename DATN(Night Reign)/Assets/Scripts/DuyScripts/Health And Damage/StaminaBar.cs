using UnityEngine;
using UnityEngine.UI;

namespace ND
{
    public class StaminaBar : MonoBehaviour
    {
        public Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
        }
        public void SetMaxStamina(int maxStamina)
        {
            slider.maxValue = maxStamina;
            slider.value = maxStamina;
        }

        public void SetCurrenStamina(int currentStamina)
        {
            slider.value = currentStamina;
        }
    }
}
