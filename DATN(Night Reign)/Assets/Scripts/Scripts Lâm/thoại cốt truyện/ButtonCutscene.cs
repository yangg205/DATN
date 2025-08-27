using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;     // Timeline thoại
    public GameObject continueButton;     // Button "Continue"
    public GameObject loadingPanel;       // Panel Loading UI
    public Image blackScreen;             // UI ảnh đen toàn màn hình
    public string nextSceneName;          // Tên scene cần load

    private void Start()
    {
        // Ẩn UI từ đầu
        continueButton.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Gắn sự kiện khi Timeline kết thúc
        if (director != null)
            director.stopped += OnCutsceneEnd;
    }

    /// <summary>
    /// Khi timeline chạy hết toàn bộ thoại
    /// </summary>
    private void OnCutsceneEnd(PlayableDirector obj)
    {
        StartCoroutine(FadeToBlackThenShowContinue());
    }

    /// <summary>
    /// Fade màn hình đen rồi mới hiện nút Continue
    /// </summary>
    private IEnumerator FadeToBlackThenShowContinue()
    {
        float duration = 2f; // thời gian fade
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Fade xong mới hiện nút Continue
        continueButton.SetActive(true);
    }

    /// <summary>
    /// Khi người chơi bấm nút Continue
    /// </summary>
    public void OnContinuePressed()
    {
        continueButton.SetActive(false);
        StartCoroutine(ShowLoadingAndChangeScene());
    }

    /// <summary>
    /// Hiện panel loading rồi chuyển scene
    /// </summary>
    private IEnumerator ShowLoadingAndChangeScene()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Giữ panel loading trong 2 giây
        yield return new WaitForSeconds(2f);

        // Load scene bất đồng bộ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
