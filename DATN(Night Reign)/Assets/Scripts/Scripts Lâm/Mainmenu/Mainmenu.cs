using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class Mainmenu : MonoBehaviour
{
    private LoadingController loadingController;
    public void Awake()
    {
        loadingController = FindAnyObjectByType<LoadingController>();
    }
    public void Play()
    {
        loadingController.LoadScene("Selectioncharacter");
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Player Has Exit The Game");
    }
}
