using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject saveConfirmPanel;

    private bool isPaused = false;

    void Start()
    {
        // Ẩn chuột và khóa khi bắt đầu
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

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

        // Dừng logic điều khiển khác
        PlayerPause.IsPaused = true;
        EnemyPause.IsPaused = true;

        pauseMenuUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        BringToFront(pauseMenuUI);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveConfirmPanel != null) saveConfirmPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        PlayerPause.IsPaused = false;
        EnemyPause.IsPaused = false;

        pauseMenuUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BringToFront(GameObject uiElement)
    {
        uiElement.transform.SetAsLastSibling();

        Canvas canvas = uiElement.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;
        }
    }
}
