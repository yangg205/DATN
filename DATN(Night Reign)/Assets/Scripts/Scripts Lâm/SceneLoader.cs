using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Prefab & UI")]
    public GameObject loadPanelPrefab;
    public Transform uiCanvas;

    [Header("Settings")]
    public float delayBeforeLoad = 2f; // Thời gian chờ trước khi load scene
    public float fadeDuration = 1f;

    private GameObject currentPanel;
    private CanvasGroup canvasGroup;

    // HÀM GỌI KHI BẤM NÚT BUTTON
    public void OnButtonClickLoadScene(string sceneName)
    {
        if (currentPanel == null)
        {
            currentPanel = Instantiate(loadPanelPrefab, uiCanvas);
            canvasGroup = currentPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = 0;
        }

        StartCoroutine(HandleLoad(sceneName));
    }

    IEnumerator HandleLoad(string sceneName)
    {
        yield return FadeIn();

        yield return new WaitForSeconds(delayBeforeLoad);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return FadeOut();

        operation.allowSceneActivation = true;
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
    }
}
