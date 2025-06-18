using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using NUnit.Framework;

public class SceneTransition : MonoBehaviour
{
    public Image fadeImage;
    SignalRClient signalRClient;
    int playerId;
    int[] characterId = { 5, 1005, 1006 };
    NotificationManager notificationManager;

    void Start()
    {
        playerId = PlayerPrefs.GetInt("PlayerId", 0);
        signalRClient = FindAnyObjectByType<SignalRClient>();
        notificationManager = FindAnyObjectByType<NotificationManager>();
        // Fade in: từ đen -> trong suốt
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0, 1f).SetEase(Ease.InOutQuad);
    }

    public async void FadeToScene(string sceneName)
    {
        int count = 0;
        for(int i = 0; i < 3; i++)
        {
            var result = await signalRClient.SelectedCharacter(playerId, characterId[i]);
            if (result.status)
            {
                count++;
            }
        }
        if(count > 0)
        {
            fadeImage.DOFade(1, 1f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
        }
    }
}
