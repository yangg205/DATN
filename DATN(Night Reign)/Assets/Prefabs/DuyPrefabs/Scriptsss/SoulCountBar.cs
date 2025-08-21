using TMPro;
using UnityEngine;

namespace AG
{
    public class SoulCountBar : MonoBehaviour
    {
        public TextMeshProUGUI soulCountText;

        public void SetSoulCountText(int soulCount)
        {
            soulCountText.text = soulCount.ToString();
        }
    }
}

