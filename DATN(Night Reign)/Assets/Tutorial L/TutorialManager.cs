using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using ND;

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

    [Header("Canvas Reference")]
    public Canvas tutorialCanvas; // Thêm tham chiếu đến Canvas chứa popup

    private bool isShowing = false;
    private TutorialTrigger currentTrigger;

    void Awake()
    {
        Instance = this;
        HidePopupImmediate();
    }

    void Update()
    {
        if (isShowing)
        {
            // Hỗ trợ đóng bằng Escape hoặc Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Phím Escape hoặc Space được nhấn: Đóng popup.");
                CloseCurrentPopup();
            }
        }
    }

    public void ShowMessage(TutorialData data, TutorialTrigger trigger)
    {
        if (data == null) return;

        // Parse tutorialText để chèn icon TMP
        popupText.text = ParseTextWithInlineIcons(data.tutorialText, data.inlineIcons);

        // Sprite bên cạnh text
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

        // Icon UI thủ công (GameObject trong Scene)
        ShowManualIcons(data.manualIconObjects);

        // Hiển thị UI và đảm bảo tương tác
        popupCanvas.alpha = 1f;
        popupCanvas.interactable = true;
        popupCanvas.blocksRaycasts = true;

        // Đẩy Canvas lên layer cao nhất
        if (tutorialCanvas != null)
        {
            tutorialCanvas.sortingOrder = 1000; // Đặt giá trị cao để vượt qua các UI khác
            Debug.Log("Canvas sorting order được đặt thành: " + tutorialCanvas.sortingOrder);
        }

        isShowing = true;
        currentTrigger = trigger;

        // Pause game và unlock chuột
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (CameraHandler.singleton != null)
            CameraHandler.singleton.canRotate = false;

        Debug.Log("Popup hiển thị: Kiểm tra tương tác.");
    }

    public void CloseCurrentPopup()
    {
        Debug.Log("CloseCurrentPopup được gọi! (từ click hoặc phím)");
        popupCanvas.alpha = 0f;
        popupCanvas.interactable = false;
        popupCanvas.blocksRaycasts = false;

        // Khôi phục sorting order về mặc định (nếu cần)
        if (tutorialCanvas != null)
        {
            tutorialCanvas.sortingOrder = 0; // Hoặc giá trị mặc định ban đầu
        }

        isShowing = false;

        HideAllIcons();

        if (tutorialVideoPlayer != null)
        {
            tutorialVideoPlayer.Stop();
        }

        if (currentTrigger != null && currentTrigger.ShouldDeactivateAfterShown())
        {
            currentTrigger.Deactivate();
            currentTrigger = null;
        }

        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (CameraHandler.singleton != null)
            CameraHandler.singleton.canRotate = true;
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

        // Khôi phục sorting order về mặc định khi ẩn
        if (tutorialCanvas != null)
        {
            tutorialCanvas.sortingOrder = 0;
        }
    }

    private string ParseTextWithInlineIcons(string rawText, List<TutorialData.TextIconMapping> iconMappings)
    {
        string processed = rawText;

        if (iconMappings == null) return processed;

        foreach (var icon in iconMappings)
        {
            if (!string.IsNullOrEmpty(icon.key) && icon.sprite != null)
            {
                string spriteName = icon.sprite.name;
                processed = processed.Replace("{" + icon.key + "}", $"<sprite name=\"{spriteName}\">");
            }
        }

        return processed;
    }
}