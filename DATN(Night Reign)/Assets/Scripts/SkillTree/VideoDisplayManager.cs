using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoDisplayManager : MonoBehaviour
{
    public RawImage videoDisplay;               // UI hiển thị video
    public VideoPlayer videoPlayer;             // VideoPlayer
    public VideoClip[] videoClips;              // Danh sách video
    public string[] skillNames;                 // Danh sách tên kỹ năng
    public string[] skillDescriptions;
    public TextMeshProUGUI skillNameText;       // Text UI để hiện tên kỹ năng
    public TextMeshProUGUI skillDescriptionText; // Text UI để hiện mô tả kỹ năng
    private SignalRClient signalRClient;

    private void Awake()
    {
        // Tìm instance của SignalRClient trong scene
        signalRClient = FindObjectOfType<SignalRClient>();
        if (signalRClient == null)
        {
            Debug.LogError("Không tìm thấy SignalRClient trong scene! Hãy thêm component SignalRClient vào một GameObject.");
        }
        else
        {
            Debug.Log("Đã tìm thấy SignalRClient: " + signalRClient.name);
        }
    }

    public void PlayVideo(int index)
    {
        if (index < 0 || index >= videoClips.Length) return;

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[index];
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += (vp) =>
        {
            videoDisplay.texture = videoPlayer.texture;
            videoPlayer.Play();
        };
    }

    public void ShowText(int index)
    {
        if (index < 0 || index >= skillNames.Length) return;

        if (skillNameText != null && index < skillNames.Length)
        {
            skillNameText.text = skillNames[index];
        }
        if (skillDescriptionText != null && index < skillDescriptions.Length)
        {
            skillDescriptionText.text = skillDescriptions[index];
        }
    }

    public void HideVideo()
    {
        videoPlayer.Stop();
        videoDisplay.texture = null;
        if (skillNameText != null)
        {
            skillNameText.text = "";
        }
        if (skillDescriptionText != null)
        {
            skillDescriptionText.text = "";
        }
    }

    public void OnClickUpdateSkill()
    {
        if (signalRClient != null)
        {
            _ = signalRClient.SendUpdateSkill(1, 11);
            Debug.Log("Gọi SendUpdateSkill thành công.");
        }
        else
        {
            Debug.LogError("signalRClient chưa được gán! Hãy kiểm tra lại scene hoặc gán thủ công.");
        }
    }
}