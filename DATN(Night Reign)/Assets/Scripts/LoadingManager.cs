using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public TMP_Text loadingText;
    public Slider loadingBar;

    private string targetScene;

    void Start()
    {
        targetScene = PlayerPrefs.GetString("NextScene", "Prototype Map");
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (!operation.isDone)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Tăng fakeProgress để slider chạy mượt đến realProgress
            if (fakeProgress < realProgress)
            {
                fakeProgress += Time.deltaTime;
                fakeProgress = Mathf.Min(fakeProgress, realProgress);
            }

            loadingBar.value = fakeProgress;
            loadingText.text = $"Loading... {Mathf.RoundToInt(fakeProgress * 100)}%";

            // Nếu cả fakeProgress và realProgress đều đạt 100%, thì chuyển scene
            if (fakeProgress >= 1f && realProgress >= 1f)
            {
                yield return new WaitForSeconds(0.3f); // Để người chơi thấy 100% một chút
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
