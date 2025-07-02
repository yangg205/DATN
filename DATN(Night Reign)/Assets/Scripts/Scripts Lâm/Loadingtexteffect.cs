using UnityEngine;
using TMPro;

public class LoadingTextEffect : MonoBehaviour
{
    public TMP_Text loadingText;
    public Color baseColor = new Color(1f, 0.9f, 0.6f); // #e0c891

    void Update()
    {
        float alpha = Mathf.PingPong(Time.time, 1f); // dao động từ 0 → 1
        loadingText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
