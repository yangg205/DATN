using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;     // Timeline cần skip
    public GameObject continueButton;     // Button "Continue"
    public Image blackScreen;             // UI ảnh đen toàn màn hình (Image)
    public GameObject loadingPrefab;      // Prefab Loading UI
    public string nextSceneName;          // Tên scene cần load

    private bool isCutsceneEnded = false;

    private void Start()
    {
        // Ẩn nút khi bắt đầu, đặt màn hình đen trong suốt
        continueButton.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        if (loadingPrefab != null)
            loadingPrefab.SetActive(false); // Ẩn loading ban đầu

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

        // Sau khi fade xong → hiện Loading UI
        if (loadingPrefab != null)
            loadingPrefab.SetActive(true);

        // Load scene Async để tránh đứng hình
        yield return StartCoroutine(LoadNextSceneAsync());
    }

    private IEnumerator LoadNextSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        // Chờ load xong 90% rồi mới vào scene
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f); // Giữ loading 1s cho đẹp
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
