using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    public Slider progressBar;
    public float loadDuration = 5f; // thời gian đầy thanh slider

    void Start()
    {
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        string sceneToLoad = PlayerPrefs.GetString("SceneToLoad");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (timer < loadDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadDuration);
            if (progressBar != null)
                progressBar.value = progress;

            yield return null;
        }

        // Khi timer đạt 5s, cho phép load scene
        operation.allowSceneActivation = true;
    }
}
