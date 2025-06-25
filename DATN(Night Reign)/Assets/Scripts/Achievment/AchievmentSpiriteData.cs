using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAchievementSpriteData", menuName = "Achievement/Achievement Sprite Data")]
public class AchievementSpriteData : ScriptableObject
{
    public Sprite[] sequentialAchievementSprites;
    public Sprite defaultAchievementSprite;


    public Sprite GetSpriteById(int achievementId)
    {
        int arrayIndex = achievementId - 1;

        if (sequentialAchievementSprites != null &&
            arrayIndex >= 0 && arrayIndex < sequentialAchievementSprites.Length)
        {
            if (sequentialAchievementSprites[arrayIndex] != null)
            {
                return sequentialAchievementSprites[arrayIndex];
            }
            else
            {
                Debug.LogWarning($"Sprite at index {arrayIndex} for Achievement ID {achievementId} is null. Using default.");
                return defaultAchievementSprite;
            }
        }

        Debug.LogWarning($"Sprite for Achievement ID {achievementId} not found in array. Using default.");
        return defaultAchievementSprite; // Luôn trả về default nếu không tìm thấy
    }
}