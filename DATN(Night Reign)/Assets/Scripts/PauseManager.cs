using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused = false;
    [SerializeField] private GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}











//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PauseManager : MonoBehaviour
//{
//    public static bool IsPaused = false;

//    [SerializeField] private GameObject pauseMenuUI;

//    private PlayerInputActions inputActions;

//    private void Awake()
//    {
//        inputActions = new PlayerInputActions();
//        inputActions.UI.Pause.performed += ctx => TogglePause();
//    }

//    private void OnEnable()
//    {
//        inputActions.UI.Enable();
//    }

//    private void OnDisable()
//    {
//        inputActions.UI.Disable();
//    }

//    private void TogglePause()
//    {
//        if (IsPaused)
//            Resume();
//        else
//            Pause();
//    }

//    public void Resume()
//    {
//        pauseMenuUI.SetActive(false);
//        Time.timeScale = 1f;
//        IsPaused = false;
//    }

//    public void Pause()
//    {
//        pauseMenuUI.SetActive(true);
//        Time.timeScale = 0f;
//        IsPaused = true;
//    }

//    public void QuitGame()
//    {
//        Debug.Log("Quit game");
//        Application.Quit();
//    }
//}
