using UnityEngine;
using UnityEngine.EventSystems;


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
        pauseMenuUI.SetActive(true);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveConfirmPanel != null) saveConfirmPanel.SetActive(false);

        // Reset selected UI element (fix hover bug)
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }
    // ✅ Quit Game (gọi từ nút Quit)
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
