using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;     // Timeline cần skip
    public GameObject continueButton;     // Button "Continue" trong Inspector
    public Image blackScreen;             // UI ảnh đen toàn màn hình (Image)

    private bool isCutsceneEnded = false;

    private void Start()
    {
        // Ẩn nút khi bắt đầu, đặt màn hình đen trong suốt
        continueButton.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        // Bắt sự kiện khi timeline kết thúc
        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        isCutsceneEnded = true;
        continueButton.SetActive(true); // Hiện nút sau khi timeline tự chạy hết
    }

    // Gọi khi bấm nút "Continue"
    public void OnContinuePressed()
    {
        // Nếu timeline chưa kết thúc, skip luôn về cuối
        if (!isCutsceneEnded && director != null)
        {
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }

        continueButton.SetActive(false);
        StartCoroutine(FadeToBlack());
    }

    // Hiệu ứng fade màn hình đen
    private IEnumerator FadeToBlack()
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // TODO: Tại đây bạn có thể load scene mới hoặc bật gameplay
        // Ví dụ:
        // SceneManager.LoadScene("NextSceneName");
    }
}
