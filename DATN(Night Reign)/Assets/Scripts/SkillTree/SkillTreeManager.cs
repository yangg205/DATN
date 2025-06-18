using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class SkillTreeManager : MonoBehaviour
{
    public RawImage videoDisplay;                 // UI hiển thị video
    public VideoPlayer videoPlayer;              // VideoPlayer
    public VideoClip[] videoClips;               // Danh sách video
    public string[] skillNames;                  // Danh sách tên kỹ năng
    public string[] skillDescriptions;           // Danh sách mô tả kỹ năng
    public TextMeshProUGUI skillNameText;        // Text UI để hiện tên kỹ năng
    public TextMeshProUGUI skillDescriptionText; // Text UI để hiện mô tả kỹ năng
    private SignalRClient signalRClient;
    private int currentSkillId = -1;             // ID kỹ năng hiện tại được chọn
    private ButtonHoverEffect ButtonHoverEffect;
    int skillpoint;

    private void Awake()
    {
        // Tìm SignalRClient trong scene
        signalRClient = FindAnyObjectByType<SignalRClient>();
        if (signalRClient == null)
        {
            Debug.LogError("Không tìm thấy SignalRClient trong scene! Hãy thêm component SignalRClient vào một GameObject.");
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

        if (skillNameText != null)
        {
            skillNameText.text = skillNames[index];
        }
        if (skillDescriptionText != null)
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

    public void SelectSkill(int skillId)
    {
        currentSkillId = skillId; // Lưu lại skillId hiện tại
        Debug.Log($"Kỹ năng được chọn: {skillId}");
    }

    public async void UpgradeCurrentSkill()
    {
        var playerCharacterId = PlayerPrefs.GetInt("PlayerCharacterId", 0);
        if (currentSkillId == -1)
        {
            Debug.LogError("Chưa có kỹ năng nào được chọn!");
            return;
        }

        if (signalRClient != null)
        {
            var result = await signalRClient.SendUpdateSkill(playerCharacterId, currentSkillId);
            if (result.status)
            {
                var buttons = FindObjectsOfType<ButtonHoverEffect>();
                foreach (var button in buttons)
                {
                    if (button.skillId == currentSkillId)
                    {
                        button.SetOwned(true); // Đặt trạng thái sở hữu
                        break;
                    }
                }

            }
            else
            {
                Debug.LogError($"Không thể nâng cấp kỹ năng {currentSkillId}!");
            }
        }
        else
        {
            Debug.LogError("SignalRClient is not initialized!");
        }
    }
}
