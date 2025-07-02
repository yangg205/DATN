using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text loadingPercentText;

    [Header("Loading Settings")]
    public float loadDuration = 5f; // Tổng thời gian loading giả lập

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

            if (loadingPercentText != null)
                loadingPercentText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }
        // Khi timer kết thúc, cho phép chuyển cảnh
        operation.allowSceneActivation = true;

        // Chờ scene load xong
        yield return new WaitUntil(() => operation.isDone);

        // Tắt instance sau khi load
        GameObject loadingInstance = GameObject.FindWithTag("LoadingUI"); // hoặc Find("LoadingUI(Clone)")
        if (loadingInstance != null)
        {
            loadingInstance.SetActive(false); // hoặc Destroy(loadingInstance) nếu không cần nữa
        }
    }
}
