using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void LoadWithLoadingScene(string targetSceneName)
    {
        PlayerPrefs.SetString("SceneToLoad", targetSceneName);
        SceneManager.LoadScene("SceneMerge 1");
    }
}
