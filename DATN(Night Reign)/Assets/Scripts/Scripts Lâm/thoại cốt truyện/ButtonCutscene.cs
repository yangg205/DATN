using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class CutsceneEndHandler : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject continueButton; // Gán Button trong Inspector
    public Image blackScreen;         // Gán ảnh đen (Image UI) toàn màn hình

    private void Start()
    {
        continueButton.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0); // Bắt đầu trong suốt

        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        continueButton.SetActive(true); // Hiện nút sau khi timeline kết thúc
    }

    public void OnContinuePressed()
    {
        continueButton.SetActive(false);
        StartCoroutine(FadeToBlack());
    }

    private System.Collections.IEnumerator FadeToBlack()
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // TODO: Load scene tiếp theo hoặc hành động khác tại đây
    }
}
