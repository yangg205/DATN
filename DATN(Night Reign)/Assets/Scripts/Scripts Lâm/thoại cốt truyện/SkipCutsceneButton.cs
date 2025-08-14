using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkipCutsceneButton : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;
    public CanvasGroup skipButtonCanvas;
    public Image blackScreen;
    // --- THÊM VÀO ---
    // Kéo Prefab UI Loading của bạn vào đây
    public GameObject loadingUIPrefab;

    [Header("Settings")]
    public float skipDelay = 5f;
    public float skipFadeDuration = 0.5f;
    public float fadeDuration = 1f;
    public string nextSceneName;

    private bool cutsceneEnded = false;
    // --- THÊM VÀO ---
    private GameObject loadingInstance; // Để lưu trữ đối tượng loading được tạo ra

    private void Start()
    {
        // Ẩn nút Skip lúc đầu
        if (skipButtonCanvas)
        {
            skipButtonCanvas.alpha = 0f;
            skipButtonCanvas.gameObject.SetActive(false);
        }

        // Chuẩn bị màn hình đen
        if (blackScreen)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0);
        }

        // --- THÊM VÀO ---
        // Đảm bảo prefab loading không hiển thị lúc bắt đầu
        if (loadingUIPrefab)
        {
            loadingUIPrefab.SetActive(false);
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

        if (skipButtonCanvas)
        {
            Destroy(skipButtonCanvas.gameObject);
        }
    }

    public void OnSkipPressed()
    {
        if (skipButtonCanvas)
        {
            Destroy(skipButtonCanvas.gameObject);
        }

        if (!cutsceneEnded && director)
        {
            director.Stop();
        }

        StartCoroutine(FadeAndLoadScene());
    }

    // --- TOÀN BỘ COROUTINE NÀY ĐÃ ĐƯỢC CẬP NHẬT ---
    private IEnumerator FadeAndLoadScene()
    {
        // 1. Fade màn hình ra màu đen
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (blackScreen)
                blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 2. Hiển thị UI Loading
        // Sau khi màn hình đã đen hoàn toàn, bật UI loading lên
        if (loadingUIPrefab != null)
        {
            // Dùng Instantiate nếu prefab không nằm sẵn trong scene,
            // hoặc SetActive(true) nếu nó đã có sẵn và bị tắt.
            // Dùng Instantiate an toàn hơn.
            loadingInstance = Instantiate(loadingUIPrefab);
            loadingInstance.SetActive(true);
        }

        // Thêm một khoảng chờ nhỏ để đảm bảo UI loading có thời gian hiển thị
        yield return new WaitForSeconds(0.5f);

        // 3. Bắt đầu tải Scene mới một cách bất đồng bộ
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);

            // Coroutine sẽ đợi ở đây cho đến khi scene mới được tải xong
            while (!asyncLoad.isDone)
            {
                // Bạn có thể cập nhật thanh tiến trình ở đây nếu muốn
                // float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                // Debug.Log("Loading progress: " + (progress * 100) + "%");
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Chưa gán tên Scene trong Inspector!");
        }
    }
}