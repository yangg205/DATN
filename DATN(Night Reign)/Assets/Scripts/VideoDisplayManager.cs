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
    public TextMeshProUGUI skillNameText;                  // Text UI để hiện tên kỹ năng
    public TextMeshProUGUI skillDescriptionText;          // Text UI để hiện mô tả kỹ năng

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

        // Đặt tên kỹ năng tương ứng
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
}
