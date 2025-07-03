using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using System;
using System.Collections;
using server.model;

public class AchievementManager : MonoBehaviour
{
    public GameObject achievementItemPrefab;
    public Transform contentParent;
    //[SerializeField]
    //private AchievementSpriteData achievementSpriteData;
    private SignalRClient signalRClient;
    private List<PlayerAchievementDto> allAchievements = new List<PlayerAchievementDto>();

    void Start()
    {
        signalRClient = FindAnyObjectByType<SignalRClient>();
        if (signalRClient == null)
        {
            Debug.LogError("SignalRClient not found in the scene! Please make sure it's present and active.");
            return;
        }
        //if (achievementSpriteData == null)
        //{
        //    Debug.LogError("Achievement Sprite Data is not assigned in the Inspector! Please assign the ScriptableObject asset.");
        //    return;
        //}
        LoadAndDisplayAchievements();
    }
    private async void LoadAndDisplayAchievements()
    {
        var dbAchievements = await signalRClient.GetAllAchievements(PlayerPrefs.GetInt("PlayerCharacterId"));

        if (dbAchievements == null || dbAchievements.Count == 0)
        {
            Debug.Log("Không có thành tựu nào được trả về từ DB.");
            return;
        }

        allAchievements.Clear();
        allAchievements = new List<PlayerAchievementDto>(dbAchievements);

        DisplayAchievements();
    }

    void DisplayAchievements()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerAchievementDto achievement in allAchievements)
        {
            GameObject achievementGO = Instantiate(achievementItemPrefab, contentParent);

            // Truy cập các thành phần UI dựa trên cấu trúc Prefab:
            // logo nằm trong Panel -> Canvas
            // name nằm trong Panel -> Canvas
            // status (là một Panel/Image) nằm trong Panel -> Canvas
            // text_status nằm trong status -> Panel -> Canvas

            Image logoImage = achievementGO.transform.Find("Canvas/Panel/logo")?.GetComponent<Image>();
            TextMeshProUGUI nameText = achievementGO.transform.Find("Canvas/Panel/name")?.GetComponent<TextMeshProUGUI>();

            // Lấy Image component từ Panel "status" để đổi màu nền
            Image statusPanelImage = achievementGO.transform.Find("Canvas/Panel/status")?.GetComponent<Image>();
            TextMeshProUGUI statusDescriptionText = achievementGO.transform.Find("Canvas/Panel/status/text_status")?.GetComponent<TextMeshProUGUI>();

            if (logoImage != null)
            {
                // ------------- PHẦN THAY ĐỔI CHỦ YẾU Ở ĐÂY -------------
                if (!string.IsNullOrEmpty(achievement.IconPath))
                {
                    Debug.Log(achievement.IconPath);
                    // Chuyển đổi đường dẫn từ Assets/ path sang Resources/ path format
                    // Ví dụ: "Assets/Img/Icon Lam/1. Bước chân đầu tiên.png"
                    // Cần chuyển thành "Img/Icon Lam/1. Bước chân đầu tiên"
                    string resourcePath = achievement.IconPath;

                    // Xóa tiền tố "Assets/" nếu có
                    if (resourcePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        resourcePath = resourcePath.Substring("Assets/".Length);
                    }
                    // Xóa phần mở rộng file (ví dụ: ".png", ".jpg")
                    int dotIndex = resourcePath.LastIndexOf('.');
                    if (dotIndex != -1)
                    {
                        resourcePath = resourcePath.Substring(0, dotIndex);
                    }

                    // Tải Sprite từ thư mục Resources
                    Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);

                    if (loadedSprite != null)
                    {
                        logoImage.sprite = loadedSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"Không thể tải Sprite từ đường dẫn Resources: {resourcePath} cho AchievementId: {achievement.AchievementId}. Hãy đảm bảo file nằm trong thư mục 'Resources' và đường dẫn đúng.");
                    }
                }
                else
                {
                    Debug.LogWarning($"AchievementId: {achievement.AchievementId} không có đường dẫn IconPath trong dữ liệu DB.");
                }
                // ------------- KẾT THÚC PHẦN THAY ĐỔI -------------
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy Image component cho logo trong prefab {achievementItemPrefab.name}");
            }

            if (nameText != null)
            {
                nameText.text = achievement.Name;
            }

            if (statusDescriptionText != null)
            {
                // Hiển thị Description của thành tựu trong text_status
                statusDescriptionText.text = achievement.Description;
                // Hoặc nếu bạn muốn hiển thị trực tiếp Status ("Completed", "InProgress")
                // statusDescriptionText.text = achievement.Status;
            }

            // Cập nhật màu sắc của Panel "status" (Image component)
            if (statusPanelImage != null)
            {
                if (achievement.Status == "Completed")
                {
                    statusPanelImage.color = Color.green;
                }
                else if (achievement.Status == "InProgress")
                {
                    statusPanelImage.color = Color.yellow;
                }
                else // "Locked" hoặc trạng thái khác
                {
                    statusPanelImage.color = Color.grey;
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }
    public async void UpdateAchievment(int playercharacterid, int achievmentid)
    {
        var result = await signalRClient.UpdateAchievement(playercharacterid, achievmentid);
        if (result != null)
        {
            Debug.Log($"Thành tựu {result.AchievementId} đã được cập nhật thành công!");
            // Cập nhật lại danh sách thành tựu và hiển thị
            LoadAndDisplayAchievements();
        }
        else
        {
            Debug.LogError($"Không thể cập nhật thành tựu {result.AchievementId}");
        }
    }
}
