using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkipCutsceneButton : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;         // Timeline
    public CanvasGroup skipButtonCanvas;      // CanvasGroup của nút Skip (để làm fade)
    public Image blackScreen;                 // Ảnh đen full màn hình

    [Header("Settings")]
    public float skipDelay = 5f;              // Thời gian chờ trước khi hiện nút Skip
    public float skipFadeDuration = 0.5f;     // Thời gian nút Skip mờ dần xuất hiện
    public float fadeDuration = 1f;           // Thời gian fade màn hình đen
    public string nextSceneName;              // Tên Scene sẽ load

    private bool cutsceneEnded = false;

    private void Start()
    {
        // Ẩn nút Skip lúc đầu (alpha = 0)
        if (skipButtonCanvas)
        {
            skipButtonCanvas.alpha = 0f;
            skipButtonCanvas.gameObject.SetActive(false);
        }

        if (blackScreen)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0);
        }

        StartCoroutine(ShowSkipButtonAfterDelay());

        if (director) director.stopped += OnCutsceneEnded;
    }

    private IEnumerator ShowSkipButtonAfterDelay()
    {
        yield return new WaitForSeconds(skipDelay);

        if (!cutsceneEnded && skipButtonCanvas)
        {
            skipButtonCanvas.gameObject.SetActive(true);

            // Hiệu ứng mờ dần xuất hiện
            float elapsed = 0f;
            while (elapsed < skipFadeDuration)
            {
                elapsed += Time.deltaTime;
                skipButtonCanvas.alpha = Mathf.Clamp01(elapsed / skipFadeDuration);
                yield return null;
            }
            skipButtonCanvas.alpha = 1f;
        }
    }

    private void OnCutsceneEnded(PlayableDirector obj)
    {
        cutsceneEnded = true;

        // Destroy luôn nút Skip khi Timeline kết thúc
        if (skipButtonCanvas)
        {
            Destroy(skipButtonCanvas.gameObject);
        }
    }

    // Gọi khi nhấn Skip
    public void OnSkipPressed()
    {
        // Destroy nút Skip ngay khi bấm
        if (skipButtonCanvas)
        {
            Destroy(skipButtonCanvas.gameObject);
        }

        // Dừng timeline nếu còn chạy
        if (!cutsceneEnded && director)
        {
            director.Stop();
        }

        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            if (blackScreen)
                blackScreen.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("Chưa gán tên Scene trong Inspector!");
    }
}
