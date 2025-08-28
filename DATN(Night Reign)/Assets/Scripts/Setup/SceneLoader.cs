using UnityEngine;
namespace yang
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.Collections;

    public class SceneLoader : MonoBehaviour
    {
        public void LoadAfterDelay(string sceneName, float delay)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, delay));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneName);
            Destroy(gameObject); // Destroy loader sau khi load
        }
    }

}

