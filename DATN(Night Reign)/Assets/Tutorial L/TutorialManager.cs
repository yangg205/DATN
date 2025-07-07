using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI References")]
    public CanvasGroup popupCanvas;
    public TMP_Text popupText;
    public Image tutorialImage;
    public RawImage tutorialVideoDisplay;

    [Header("Video")]
    public VideoPlayer tutorialVideoPlayer;

    [Header("Icons")]
    public Transform buttonIconsContainer;

    private bool isShowing = false;
    private TutorialTrigger currentTrigger;

    void Awake()
    {
        Instance = this;
        HidePopupImmediate();
    }

    public void ShowMessage(TutorialData data, TutorialTrigger trigger)
    {
        if (data == null) return;

        popupText.text = data.tutorialText;

        // Sprite
        if (tutorialImage != null)
        {
            tutorialImage.sprite = data.tutorialSprite;
            tutorialImage.gameObject.SetActive(data.tutorialSprite != null);
        }

        // Video
        if (tutorialVideoPlayer != null && data.tutorialVideo != null)
        {
            tutorialVideoPlayer.clip = data.tutorialVideo;
            tutorialVideoPlayer.Play();
            tutorialVideoDisplay.gameObject.SetActive(true);
        }
        else
        {
            tutorialVideoDisplay.gameObject.SetActive(false);
        }

        // Icons
        ShowManualIcons(data.manualIconObjects);

        // UI
        popupCanvas.alpha = 1f;
        popupCanvas.interactable = true;
        popupCanvas.blocksRaycasts = true;

        isShowing = true;
        currentTrigger = trigger;

        Time.timeScale = 0f;
    }

    public void CloseCurrentPopup()
    {
        popupCanvas.alpha = 0f;
        popupCanvas.interactable = false;
        popupCanvas.blocksRaycasts = false;

        isShowing = false;
        Time.timeScale = 1f;

        HideAllIcons();

        // Stop video
        if (tutorialVideoPlayer != null)
        {
            tutorialVideoPlayer.Stop();
        }

        if (currentTrigger != null && currentTrigger.ShouldDeactivateAfterShown())
        {
            currentTrigger.Deactivate();
            currentTrigger = null;
        }
    }

    private void ShowManualIcons(List<GameObject> icons)
    {
        HideAllIcons();

        foreach (var icon in icons)
        {
            if (icon != null)
                icon.SetActive(true);
        }
    }

    private void HideAllIcons()
    {
        foreach (Transform child in buttonIconsContainer)
        {
            if (child != null)
                child.gameObject.SetActive(false);
        }
    }

    private void HidePopupImmediate()
    {
        popupCanvas.alpha = 0f;
        popupCanvas.interactable = false;
        popupCanvas.blocksRaycasts = false;
        tutorialVideoDisplay.gameObject.SetActive(false);
        isShowing = false;
    }
}
