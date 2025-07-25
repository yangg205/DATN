using TMPro;
using UnityEngine;

public class SoulCountBar : MonoBehaviour
{
    public TextMeshProUGUI soulCountText;

    public void SetSoulCountText(int soulCount)
    {
        soulCountText.text = soulCount.ToString();
    }
        
}
