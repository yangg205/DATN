using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class Mainmenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Selectioncharacter");
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Player Has Exit The Game");
    }
}
