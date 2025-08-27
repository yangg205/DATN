using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineTriggerOnQuestsComplete : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject endGameObject;   // <- đổi thành GameObject cha "end"
    [SerializeField] private QuestManager questManager;
    public TextMeshProUGUI press;

    private bool hasTriggered = false;

    private void Awake()
    {
        // Validate QuestManager
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
            if (questManager == null)
            {
                Debug.LogError("[TimelineTriggerOnQuestsComplete] QuestManager not found in scene!");
                enabled = false;
                return;
            }
        }

        if (endGameObject == null)
        {
            Debug.LogError("[TimelineTriggerOnQuestsComplete] End GameObject not assigned!");
            enabled = false;
            return;
        }

        // Ensure the whole group is inactive at start
        if (endGameObject.activeSelf)
        {
            endGameObject.SetActive(false);
            Debug.Log("[TimelineTriggerOnQuestsComplete] End GameObject set to inactive on initialization.");
        }

        Debug.Log("[TimelineTriggerOnQuestsComplete] Initialized successfully.");
    }

    private void Update()
    {
        if (hasTriggered) return;

        if (questManager.AreAllQuestsCompleted())
        {
            TriggerEndEvent();
            press.gameObject.SetActive(false);
        }
    }

    private void TriggerEndEvent()
    {
        if (endGameObject != null)
        {
            endGameObject.SetActive(true);
            hasTriggered = true;
            Debug.Log("[TimelineTriggerOnQuestsComplete] All quests completed! End GameObject activated.");
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        if (endGameObject != null && endGameObject.activeSelf)
        {
            endGameObject.SetActive(false);
            Debug.Log("[TimelineTriggerOnQuestsComplete] End GameObject deactivated and trigger reset.");
        }
    }
}
