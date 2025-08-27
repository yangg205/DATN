using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    [Header("Tham Chiếu")]
    [SerializeField] private PlayableDirector director;     // Timeline thoại
    [SerializeField] private GameObject continueButton;     // Nút "Tiếp tục"
    [SerializeField] private GameObject loadingPanel;       // Panel giao diện Loading
    [SerializeField] private Image blackScreen;            // Hình ảnh đen toàn màn hình
    [SerializeField] private string nextSceneName;         // Tên scene cần chuyển đến
    [SerializeField] private AudioSource audioSource;      // Âm thanh nền hoặc thoại

    [Header("Cài Đặt")]
    [SerializeField] private float fadeDuration = 2f;      // Thời gian fade

    private bool isTransitioning = false;                   // Ngăn bấm nút nhiều lần

    private void Start()
    {
        // Kiểm tra các tham chiếu
        if (!ValidateReferences()) return;

        continueButton.SetActive(false);  // Ẩn nút từ đầu
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Đăng ký sự kiện khi timeline kết thúc
        director.stopped += OnCutsceneEnd;
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (director == null) { Debug.LogError("PlayableDirector chưa được gán!", this); isValid = false; }
        if (continueButton == null) { Debug.LogError("Nút Continue chưa được gán!", this); isValid = false; }
        if (blackScreen == null) { Debug.LogError("BlackScreen chưa được gán!", this); isValid = false; }
        if (string.IsNullOrEmpty(nextSceneName)) { Debug.LogError("Tên scene tiếp theo bị trống!", this); isValid = false; }
        if (audioSource == null) { Debug.LogWarning("AudioSource chưa được gán, âm thanh sẽ không được điều khiển.", this); }
        return isValid;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        StartCoroutine(FadeToBlackThenShowContinue());
    }

    private IEnumerator FadeToBlackThenShowContinue()
    {
        // Tắt âm thanh và thoại
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Đảm bảo timeline dừng hoàn toàn
        if (director != null)
            director.Stop();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        continueButton.SetActive(true);
    }

    public void OnContinuePressed()
    {
        if (isTransitioning) return; // Ngăn bấm nút nhiều lần
        isTransitioning = true;

        continueButton.SetActive(false);
        StartCoroutine(ShowLoadingAndChangeScene());
    }

    private IEnumerator ShowLoadingAndChangeScene()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        else
            Debug.LogWarning("Panel Loading chưa được gán, tiếp tục mà không có giao diện loading.", this);

        // Đợi 2 giây để hiển thị màn hình loading
        yield return new WaitForSeconds(2f);

        // Kiểm tra tên scene hợp lệ
        if (!SceneExists(nextSceneName))
        {
            Debug.LogError($"Scene '{nextSceneName}' không tồn tại trong Build Settings!", this);
            isTransitioning = false;
            yield break;
        }

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

        isTransitioning = false;
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (scene == sceneName) return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh rò rỉ bộ nhớ
        if (director != null)
            director.stopped -= OnCutsceneEnd;
    }
}