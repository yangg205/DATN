using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyDesQuest : MonoBehaviour
{
    private QuestManager quest;

    void Start()
    {
        quest = FindAnyObjectByType<QuestManager>();
        if (quest == null)
        {
            Debug.LogError("QuestManager not found in the scene.");
            return;
        }
    }
    private void OnDestroy()
    {
        if (quest != null)
        {
            quest.ReportKill();
        }

    }
}
