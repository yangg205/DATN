using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public GameObject loadingPrefab; // Gán trong Inspector hoặc Resources.Load
    private GameObject loadingInstance;

    public void LoadScene(string sceneName)
    {
        if (loadingInstance == null)
        {
            loadingInstance = Instantiate(loadingPrefab);
            DontDestroyOnLoad(loadingInstance);
        }

        loadingInstance.SetActive(true);
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // Gán tiến trình vào thanh Slider nếu có
            yield return null;
        }

        // Sau khi load xong, tắt loading panel (hoặc destroy nếu không cần nữa)
        loadingInstance.SetActive(false);
    }
}
