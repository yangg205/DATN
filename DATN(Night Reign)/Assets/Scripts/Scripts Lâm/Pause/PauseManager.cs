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
        // Ẩn chuột và khóa khi bắt đầu game
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
                Resume(); // Bấm ESC lần 2 cũng sẽ ẩn chuột ở đây
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

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

        pauseMenuUI.SetActive(false);

        // Ẩn chuột bất kể gọi từ ESC hay Resume button
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
