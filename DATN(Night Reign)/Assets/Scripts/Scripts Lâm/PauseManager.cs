using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject saveConfirmPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                Pause();
            else
                Resume();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

        // Hiện pause menu chính
        pauseMenuUI.SetActive(true);

        // Ẩn các panel phụ nếu có
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveConfirmPanel != null) saveConfirmPanel.SetActive(false);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }
}
