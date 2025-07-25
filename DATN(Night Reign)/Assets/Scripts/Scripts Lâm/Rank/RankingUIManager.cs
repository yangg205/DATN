using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using server.model;

public class RankingUIManager : MonoBehaviour
{
    public Transform contentParent;          // Content (Vertical Layout Group)
    public GameObject rankingItemPrefab;     // Prefab cho mỗi dòng
    public ScrollRect scrollRect;            // Để cuộn về đầu

    public void DisplayRanking(List<Ranking> rankings)
    {
        // Xoá item cũ
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;  // Scroll về top

        StartCoroutine(SpawnItemsWithAnimation(rankings));
    }

    private System.Collections.IEnumerator SpawnItemsWithAnimation(List<Ranking> rankings)
    {
        for (int i = 0; i < rankings.Count; i++)
        {
            var rank = rankings[i];
            GameObject item = Instantiate(rankingItemPrefab, contentParent);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = rank.rank.ToString();
            texts[1].text = rank.playerName.ToString();
            texts[2].text = rank.total_point.ToString();

            // Ẩn item ban đầu
            var rect = item.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;

            // Chờ layout cập nhật
            Canvas.ForceUpdateCanvases();

            // Hiệu ứng hiện dần
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.1f); // Delay giữa các item
        }
    }
}
