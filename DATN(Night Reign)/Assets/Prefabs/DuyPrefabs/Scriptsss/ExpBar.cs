using UnityEngine;
using UnityEngine.UI;

namespace AG
{
    public class ExpBar : MonoBehaviour
    {
        public Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
        }

        public void SetMaxEXP(int maxEXP)
        {
            slider.maxValue = maxEXP;
            slider.value = 0;
        }

        public void SetCurrentEXP(int currentEXP)
        {
            slider.value = currentEXP;
        }
    }
}

