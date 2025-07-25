using System;
using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public RankingUIManager uiManager;
    public int characterId = 5; // Gán ID nhân vật cần xem bảng xếp hạng

    private async void OnEnable()
    {
        try
        {
            // Gọi API từ SignalRClient
            var rankings = await FindAnyObjectByType<SignalRClient>().RankingPlayerCharacter(characterId);
            // Hiển thị ra UI
            uiManager.DisplayRanking(rankings);
        }
        catch (Exception ex)
        {
            Debug.LogError("Không thể tải bảng xếp hạng: " + ex.Message);
        }
    }
}
