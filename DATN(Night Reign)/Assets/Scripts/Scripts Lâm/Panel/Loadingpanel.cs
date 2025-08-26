using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Loadingpanel : MonoBehaviour
{
    [Header("Loading Panel Settings")]
    public GameObject loadingPanel;    // Panel Loading có sẵn trong Canvas
    public float loadingTime = 3f;     // Thời gian loading (chỉnh trong Inspector)
    public string sceneToLoad = "GameScene"; // Scene cần load

    private void Start()
    {
        // Khi vào game thì ẩn Panel Loading đi
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void OnSelectCharacter()
    {
        StartCoroutine(ShowLoadingAndLoadScene());
    }

    private IEnumerator ShowLoadingAndLoadScene()
    {
        // Hiện Panel Loading
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Chờ đúng thời gian loading
        yield return new WaitForSeconds(loadingTime);

        // Load scene mới
        SceneManager.LoadScene(sceneToLoad);
    }
}
