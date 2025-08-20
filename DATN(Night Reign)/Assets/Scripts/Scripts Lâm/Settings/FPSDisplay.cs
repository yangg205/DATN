using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;

    void Update()
    {
        // Cập nhật deltaTime để làm mượt giá trị FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // Hiển thị FPS
        if (fpsText != null)
            fpsText.text = $"{Mathf.RoundToInt(fps)} FPS";
    }
}
