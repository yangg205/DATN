using UnityEngine;
using UnityEngine.UI;

namespace ND
{
    public class FocusPointBar : MonoBehaviour
    {
        public Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
        }
        public void SetMaxFocusPoint(float maxFocusPoint)
        {
            slider.maxValue = maxFocusPoint;
            slider.value = maxFocusPoint;
        }

        public void SetCurrentFocusPoint(float currentFocusPoint)
        {
            slider.value = currentFocusPoint;
        }
    }
}

