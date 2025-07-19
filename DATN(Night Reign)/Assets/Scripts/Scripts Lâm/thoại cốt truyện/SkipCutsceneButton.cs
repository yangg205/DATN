using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkipCutscenButton : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;         // Timeline
    public GameObject continueButton;         // Nút Skip
    public Image blackScreen;                 // Ảnh đen toàn màn hình

    [Header("Fade & Scene Settings")]
    public float fadeDuration = 1f;           // Thời gian fade đen
    public string nextSceneName;              // Tên scene sẽ load sau khi skip

    private bool isCutsceneEnded = true;

    private void Start()
    {
        // Ẩn nút và chuẩn bị màn hình đen (trong suốt)
        continueButton.SetActive(true);
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0);

        // Lắng nghe sự kiện khi timeline tự kết thúc
        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        isCutsceneEnded = true;
        continueButton.SetActive(true);  // Hiện nút nếu timeline kết thúc tự nhiên
    }

    // Khi bấm nút Skip
    public void OnContinuePressed()
    {
        continueButton.SetActive(false);

        // Nếu timeline đang chạy, dừng luôn
        if (!isCutsceneEnded && director != null)
        {
            director.Stop();
        }

        // Bắt đầu fade đen và load scene
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Load scene tiếp theo
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Chưa gán tên Scene trong Inspector!");
        }
    }
}
