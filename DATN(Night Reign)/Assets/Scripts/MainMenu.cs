using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("Lam");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
