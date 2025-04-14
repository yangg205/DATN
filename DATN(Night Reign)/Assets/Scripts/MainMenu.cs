using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("Lam");
    }
    public void LoadSettings()
    {
        SceneManager.LoadScene("Settings"); // thay "Settings" bằng tên scene thực tế nếu khác
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
