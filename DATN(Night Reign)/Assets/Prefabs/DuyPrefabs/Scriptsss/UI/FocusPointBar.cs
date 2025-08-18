using UnityEngine;
using UnityEngine.UI;

namespace AG
{
    public class FocusPointBar : MonoBehaviour
    {
        public Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
        }
        public void SetMaxFocusPoints(float maxFocusPoints)
        {
            slider.maxValue = maxFocusPoints;
            slider.value = maxFocusPoints;
        }

        public void SetCurrentFocusPoint(float currentFocusPoints)
        {
            slider.value = currentFocusPoints;
        }
    }
}

