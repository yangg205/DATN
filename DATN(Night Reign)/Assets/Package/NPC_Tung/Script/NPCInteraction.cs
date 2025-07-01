using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject questUI;
    public Button acceptButton;
    public Button declineButton;
    public Button claimRewardButton;

    [Header("Quest")]
    public QuestManager questManager;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            questUI.SetActive(!questUI.activeSelf);

            if (questUI.activeSelf)
            {
                var quest = questManager?.GetCurrentQuest();
                if (quest != null && quest.questType == QuestType.FindNPC)
                {
                    var npcIdentity = GetComponent<NPCIdentity>();
                    if (npcIdentity != null && npcIdentity.npcID == quest.targetNPCID)
                    {
                        questManager.TryCompleteQuestByTalk();
                    }
                }

                ShowCorrectDialogue();
            }
        }
    }

    private void ShowCorrectDialogue()
    {
        var quest = questManager?.GetCurrentQuest();
        if (quest == null) return;

        if (questManager.IsQuestCompleted())
        {
            dialogueManager.StartDialogue(quest.dialogueAfterComplete);
            ShowClaimRewardUI();
        }
        else
        {
            dialogueManager.StartDialogue(quest.dialogueBeforeComplete);
            ShowDefaultUI();
        }
    }

    private void ShowDefaultUI()
    {
        acceptButton?.gameObject.SetActive(true);
        declineButton?.gameObject.SetActive(true);
        claimRewardButton?.gameObject.SetActive(false);
    }

    private void ShowClaimRewardUI()
    {
        acceptButton?.gameObject.SetActive(false);
        declineButton?.gameObject.SetActive(false);
        claimRewardButton?.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (questUI != null) questUI.SetActive(false);
        }
    }
}
