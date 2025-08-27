using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("UI References")]
    public Image blackScreen;          // UI ảnh đen full màn hình (alpha = 0 ban đầu)
    public GameObject loadingPanel;    // Panel loading (ẩn ban đầu)

    [Header("Settings")]
    public float fadeDuration = 1.5f;  // Thời gian fade
    public float loadingDelay = 2f;    // Thời gian chờ ở panel loading trước khi chuyển scene
    public string nextSceneName = "Selection2"; // Tên scene sẽ load

    private bool isDead = false;

    void Start()
    {
        // Luôn tắt panel loading khi mới vào game
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Đảm bảo màn hình đen trong suốt ban đầu
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
        }
    }

    void Update()
    {
        // Bấm phím K để chết
        if (!isDead && Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        isDead = true;

        // Fade màn hình sang đen
        float elapsed = 0f;
        Color color = blackScreen.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            blackScreen.color = color;
            yield return null;
        }

        // Hiện panel loading
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Chờ một lúc
        yield return new WaitForSeconds(loadingDelay);

        // Chuyển scene
        SceneManager.LoadScene(nextSceneName);
    }
}
