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

        // Đảm bảo panel luôn nằm trên cùng
        BringToFront(pauseMenuUI);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveConfirmPanel != null) saveConfirmPanel.SetActive(false);

        // Reset selected UI element (fix hover bug)
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        MouseManager.Instance.HideCursorAndEnableInput();
        isPaused = false;
        pauseMenuUI.SetActive(false);
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
        // Đưa transform lên cuối để luôn render trên cùng
        uiElement.transform.SetAsLastSibling();

        // Nếu có Canvas riêng, tăng Sorting Order
        Canvas canvas = uiElement.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999; // số lớn để luôn trên cùng
        }
    }
}
