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
        continueButton.SetActive(false);  // Ẩn nút ngay từ đầu
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Khi timeline kết thúc thì gọi OnCutsceneEnd
        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        // Khi cutscene thoại kết thúc → bắt đầu fade đen
        StartCoroutine(FadeToBlackThenShowContinue());
    }

    private IEnumerator FadeToBlackThenShowContinue()
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

        // Sau khi fade đen xong thì mới hiện nút Continue
        continueButton.SetActive(true);
    }

    // Khi người chơi bấm nút Continue
    public void OnContinuePressed()
    {
        continueButton.SetActive(false);
        StartCoroutine(ShowLoadingAndChangeScene());
    }

    private IEnumerator ShowLoadingAndChangeScene()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Giữ màn hình loading trong 2 giây
        yield return new WaitForSeconds(2f);

        // Load scene Async
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
