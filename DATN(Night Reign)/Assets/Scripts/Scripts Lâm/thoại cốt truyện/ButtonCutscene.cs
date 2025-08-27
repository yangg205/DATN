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

        // Khi timeline kết thúc thì gọi OnCutsceneEnd
        if (director != null)
            director.stopped += OnCutsceneEnd;
    }

    /// <summary>
    /// Khi timeline chạy hết thoại → chỉ hiện nút Continue (không fade)
    /// </summary>
    private void OnCutsceneEnd(PlayableDirector obj)
    {
        continueButton.SetActive(true);
    }

    /// <summary>
    /// Khi người chơi bấm nút Continue → fade đen + loading + chuyển scene
    /// </summary>
    public void OnContinuePressed()
    {
        continueButton.SetActive(false);
        StartCoroutine(FadeToBlackAndLoad());
    }

    private IEnumerator FadeToBlackAndLoad()
    {
        // Bước 1: Fade đen
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Bước 2: Hiện panel loading
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Giữ loading trong 2 giây
        yield return new WaitForSeconds(2f);

        // Bước 3: Load scene bất đồng bộ
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
