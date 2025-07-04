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

    [Header("Default Dialogues")]
    [TextArea(3, 5)] public string[] defaultGreetingDialogue;
    [TextArea(3, 5)] public string[] questAlreadyAcceptedDialogue;
    [TextArea(3, 5)] public string[] noRelevantQuestDialogue;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            var quest = questManager?.GetCurrentQuest();
            var npcID = GetComponent<NPCIdentity>()?.npcID;

            if (quest == null || string.IsNullOrEmpty(npcID))
            {
                dialogueManager.StartDialogue(defaultGreetingDialogue);
                if (questUI != null && questUI.activeSelf) questUI.SetActive(false);
                return;
            }

            if (questUI != null) questUI.SetActive(true);

            bool isQuestAccepted = questManager.IsQuestAccepted();
            bool isQuestCompleted = questManager.IsQuestCompleted();

            if (quest.giverNPCID == npcID)
            {
                if (isQuestAccepted && !isQuestCompleted)
                {
                    // ✅ Thông báo trong khung thoại chính
                    dialogueManager.StartDialogue(new string[] {
                        "Ngươi đã nhận nhiệm vụ này rồi, hãy hoàn thành nhiệm vụ trước."
                    });
                    ShowDefaultUI();
                }
                else if (isQuestCompleted)
                {
                    Debug.Log("✅ Nhiệm vụ đã hoàn thành! Hãy nhận thưởng từ người giao.");
                    ShowCorrectDialogue();
                }
                else
                {
                    Debug.Log("🆕 Có nhiệm vụ mới từ NPC này!");
                    ShowCorrectDialogue();
                }
            }
            else if (quest.questType == QuestType.FindNPC && quest.targetNPCID == npcID && isQuestAccepted && !isQuestCompleted)
            {
                Debug.Log($"✅ Đã nói chuyện với NPC mục tiêu {npcID} cho nhiệm vụ {quest.questName}. Nhiệm vụ hoàn thành!");
                questManager.TryCompleteQuestByTalk();
                isQuestCompleted = questManager.IsQuestCompleted();
                ShowCorrectDialogue();
            }
            else if (quest.questType == QuestType.FindNPC && quest.targetNPCID == npcID && isQuestCompleted)
            {
                Debug.Log($"✅ Nhiệm vụ đã hoàn thành tại NPC mục tiêu {npcID}. Hãy nhận thưởng!");
                ShowCorrectDialogue();
            }
            else
            {
                Debug.Log($"⛔ NPC {npcID} không liên quan đến nhiệm vụ hiện tại ({quest.questName}).");
                dialogueManager.StartDialogue(noRelevantQuestDialogue);
                if (questUI != null && questUI.activeSelf) questUI.SetActive(false);
            }
        }
    }

    public void AcceptCurrentQuest()
    {
        var quest = questManager.GetCurrentQuest();

        if (quest == null)
        {
            Debug.LogWarning("❌ Không có nhiệm vụ hiện tại để nhận.");
            return;
        }

        if (questManager.IsQuestAccepted())
        {
            Debug.Log("⚠️ Bạn đã nhận nhiệm vụ này rồi.");
            dialogueManager.StartDialogue(new string[] {
                "Ngươi đã nhận nhiệm vụ này rồi, hãy hoàn thành nhiệm vụ trước."
            });
            ShowDefaultUI();
            return;
        }

        questManager.AcceptQuest();
        Debug.Log("🆕 Nhiệm vụ đã được nhận!");
        dialogueManager.StartDialogue(new string[] { "Tốt, hãy bắt đầu nhiệm vụ!" });
        ShowDefaultUI();
    }

    public void ClaimReward()
    {
        questManager.CompleteQuest();
        Debug.Log("🎉 Nhận thưởng thành công!");
        dialogueManager.StartDialogue(new string[] {
            "Ngươi đã hoàn thành tốt nhiệm vụ. Hãy nhận phần thưởng xứng đáng!"
        });
        HideAllButtons();
    }

    public void DeclineQuest()
    {
        Debug.Log("❌ Người chơi từ chối nhận nhiệm vụ.");
        dialogueManager.StartDialogue(new string[] { "Khi nào sẵn sàng, hãy quay lại gặp ta." });
        HideAllButtons();
    }

    private void ShowCorrectDialogue()
    {
        var quest = questManager?.GetCurrentQuest();
        if (quest == null) return;

        if (questManager.IsQuestCompleted())
        {
            dialogueManager.StartDialogue(quest.keydialogueAfterComplete);
            ShowClaimRewardUI();
        }
        else
        {
            dialogueManager.StartDialogue(quest.keydialogueBeforeComplete);
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

    private void HideAllButtons()
    {
        acceptButton?.gameObject.SetActive(false);
        declineButton?.gameObject.SetActive(false);
        claimRewardButton?.gameObject.SetActive(false);
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
            dialogueManager?.EndDialogue();
        }
    }
}
