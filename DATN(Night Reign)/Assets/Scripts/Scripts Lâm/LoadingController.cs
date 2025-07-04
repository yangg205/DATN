using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public GameObject loadingPrefab;
    private GameObject loadingInstance;

    public void LoadScene(string sceneName)
    {
        PlayerPrefs.SetString("SceneToLoad", sceneName);
        PlayerPrefs.Save();

        if (loadingInstance == null)
        {
            loadingInstance = Instantiate(loadingPrefab);
            DontDestroyOnLoad(loadingInstance);
        }

        loadingInstance.SetActive(true);

        // Chuyển tới scene trung gian Loading
        SceneManager.LoadScene("Scene_Loading");
    }
}
