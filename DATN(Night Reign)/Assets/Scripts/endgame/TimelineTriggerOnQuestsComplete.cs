using TMPro;
using UnityEngine;
using UnityEngine.Playables;
public class TimelineTriggerOnQuestsComplete : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject timelineGameObject;
    [SerializeField] private QuestManager questManager;
    public TextMeshProUGUI press;

    private bool hasTimelineTriggered = false;

    private void Awake()
    {
        // Validate references
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

        if (timelineGameObject == null)
        {
            Debug.LogError("[TimelineTriggerOnQuestsComplete] Timeline GameObject not assigned!");
            enabled = false;
            return;
        }

        // Ensure the timeline GameObject is initially inactive
        if (timelineGameObject.activeSelf)
        {
            timelineGameObject.SetActive(false);
            Debug.Log("[TimelineTriggerOnQuestsComplete] Timeline GameObject set to inactive on initialization.");
        }

        // Verify PlayableDirector exists and has a Timeline asset
        var playableDirector = timelineGameObject.GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            Debug.LogError("[TimelineTriggerOnQuestsComplete] No PlayableDirector found on Timeline GameObject!");
            enabled = false;
            return;
        }
        if (playableDirector.playableAsset == null)
        {
            Debug.LogError("[TimelineTriggerOnQuestsComplete] No Timeline asset assigned to PlayableDirector on Timeline GameObject!");
            enabled = false;
            return;
        }
        if (!playableDirector.playOnAwake)
        {
            Debug.LogWarning("[TimelineTriggerOnQuestsComplete] Play On Awake is disabled on PlayableDirector. The Timeline may not play automatically when the GameObject is activated. Consider enabling it or adding manual playback logic.");
        }

        Debug.Log("[TimelineTriggerOnQuestsComplete] Initialized successfully.");
    }

    private void Update()
    {
        // Skip if the timeline has already been triggered
        if (hasTimelineTriggered)
            return;

        // Check if all quests are completed
        if (questManager.AreAllQuestsCompleted())
        {
            TriggerTimeline();
            press.gameObject.SetActive(false);


        }
    }

    private void TriggerTimeline()
    {
        if (timelineGameObject != null)
        {
            timelineGameObject.SetActive(true);
            hasTimelineTriggered = true;
            Debug.Log("[TimelineTriggerOnQuestsComplete] All quests completed! Timeline GameObject activated.");
        }
        else
        {
            Debug.LogWarning("[TimelineTriggerOnQuestsComplete] Cannot activate Timeline: Timeline GameObject is null.");
        }
    }

    // Method to reset the trigger state (e.g., for testing or restarting)
    public void ResetTimelineTrigger()
    {
        hasTimelineTriggered = false;
        if (timelineGameObject != null && timelineGameObject.activeSelf)
        {
            timelineGameObject.SetActive(false);
            Debug.Log("[TimelineTriggerOnQuestsComplete] Timeline GameObject deactivated and trigger reset.");
        }
        else
        {
            Debug.Log("[TimelineTriggerOnQuestsComplete] Timeline trigger reset.");
        }
    }
}
