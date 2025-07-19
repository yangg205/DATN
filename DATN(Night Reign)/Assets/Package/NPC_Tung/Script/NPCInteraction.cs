using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _questUI;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private Button _continueButton;

    [Header("Quest")]
    public QuestManager questManager;
    [SerializeField] private QuestData _questToGive;
    [SerializeField] private QuestData _nextQuestAfterCompletion;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Default Dialogues")]
    [TextArea(3, 5)][SerializeField] private string[] defaultGreetingDialogueKeys;
    [SerializeField] private AudioClip[] defaultGreetingVoiceClips;
    [TextArea(3, 3)][SerializeField] private string[] noRelevantQuestDialogueKeys;
    [SerializeField] private AudioClip[] noRelevantQuestVoiceClips;

    private bool isPlayerInRange = false;
    private string _npcID;
    private bool _shouldContinueAfterComplete = false;
    private QuestData _lastCompletedQuest = null;

    void Start()
    {
        
        if (!questManager || !dialogueManager || !TryGetComponent(out NPCIdentity npcIdentity))
        {
            Debug.LogError("Missing required components.");
            enabled = false;
            return;
        }

        _npcID = npcIdentity.npcID;

        _acceptButton?.onClick.AddListener(AcceptCurrentQuest);
        _declineButton?.onClick.AddListener(DeclineQuest);
        _claimRewardButton?.onClick.AddListener(ClaimReward);
        _continueButton?.GetComponent<Button>()?.onClick.AddListener(() => dialogueManager.DisplayNextSentence());

        _questUI?.SetActive(false);
        HideAllActionButtons();
    }

    void Update()
    {
        
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C))
        {
            _continueButton.gameObject.SetLocalizationKey("btn_next");
            _claimRewardButton.gameObject.SetLocalizationKey("btn_claim");
            HandleInteraction();
            

        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            questManager.ReportKill();
        }
    }

    private void HandleInteraction()
    {
        _questUI?.SetActive(true);
        MouseManager.Instance.ShowCursorAndDisableInput();

        if (_shouldContinueAfterComplete && _lastCompletedQuest != null)
        {
            HideAllActionButtons();
            ContinueAfterCompleteDialogue();
            return;
        }

        QuestData currentQuestToManage = GetQuestToOfferOrManage();
        QuestData findNPCQuest = GetFindNPCQuestIfTarget();

        if (findNPCQuest != null)
        {
            questManager.DisplayQuestOfferUI(findNPCQuest);
            var status = questManager.GetQuestStatus(findNPCQuest);
            int previousProgress = status?.currentProgress ?? 0;
            bool wasObjectiveMet = status?.isObjectiveMet ?? false;

            questManager.TryCompleteQuestByTalk();

            int updatedProgress = status?.currentProgress ?? 0;
            bool isObjectiveMet = status?.isObjectiveMet ?? false;

            if (updatedProgress > previousProgress || (!wasObjectiveMet && isObjectiveMet))
            {
                dialogueManager.StartDialogue(
                    findNPCQuest.GetDialogueKeys(QuestDialogueType.ObjectiveMet),
                    findNPCQuest.GetDialogueVoiceClips(QuestDialogueType.ObjectiveMet),
                    OnDialogueCompleted);
            }
            else
            {
                dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
            }
            return;
        }

        if (currentQuestToManage != null)
        {
            questManager.DisplayQuestOfferUI(currentQuestToManage);
            var status = questManager.GetQuestStatus(currentQuestToManage);
            bool isAccepted = status != null;
            bool isObjectiveMet = status?.isObjectiveMet ?? false;
            bool isCompleted = status?.isCompleted ?? false;

            if (!isAccepted)
            {
                ShowDefaultUI();
                dialogueManager.StartDialogue(
                    currentQuestToManage.GetDialogueKeys(QuestDialogueType.BeforeComplete),
                    currentQuestToManage.GetDialogueVoiceClips(QuestDialogueType.BeforeComplete),
                    OnDialogueCompleted);
            }
            else if (!isObjectiveMet)
            {
                HideAllActionButtons();
                dialogueManager.StartDialogue(
                    currentQuestToManage.GetDialogueKeys(QuestDialogueType.ObjectiveMet),
                    currentQuestToManage.GetDialogueVoiceClips(QuestDialogueType.ObjectiveMet),
                    OnDialogueCompleted);
            }
            else if (!isCompleted)
            {
                ShowClaimRewardUI();
                string[] keys = currentQuestToManage.GetDialogueKeys(QuestDialogueType.AfterComplete);
                AudioClip[] clips = currentQuestToManage.GetDialogueVoiceClips(QuestDialogueType.AfterComplete);

                if (keys != null && keys.Length > 0)
                {
                    dialogueManager.StartDialogue(new string[] { keys[0] }, new AudioClip[] { clips[0] }, OnDialogueCompleted);
                }
                else
                {
                    MouseManager.Instance.HideCursorAndEnableInput();
                }
            }
            else
            {
                _questToGive = null;
                dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
            }
            return;
        }

        dialogueManager.StartDialogue(defaultGreetingDialogueKeys, defaultGreetingVoiceClips, OnDialogueCompleted);
    }

    private void ContinueAfterCompleteDialogue()
    {
        if (_lastCompletedQuest == null)
        {
            _shouldContinueAfterComplete = false;
            return;
        }

        string[] keys = _lastCompletedQuest.GetDialogueKeys(QuestDialogueType.AfterComplete);
        AudioClip[] clips = _lastCompletedQuest.GetDialogueVoiceClips(QuestDialogueType.AfterComplete);

        if (keys != null && keys.Length > 1)
        {
            string[] remainingKeys = new string[keys.Length - 1];
            AudioClip[] remainingClips = new AudioClip[clips.Length - 1];
            Array.Copy(keys, 1, remainingKeys, 0, remainingKeys.Length);
            Array.Copy(clips, 1, remainingClips, 0, remainingClips.Length);

            dialogueManager.StartDialogue(remainingKeys, remainingClips, () =>
            {
                _shouldContinueAfterComplete = false;
                _lastCompletedQuest = null;

                // Tự động nhận nhiệm vụ tiếp theo nếu có
                if (_nextQuestAfterCompletion != null && !questManager.IsQuestTrulyCompleted(_nextQuestAfterCompletion))
                {
                    _questToGive = _nextQuestAfterCompletion;
                    questManager.AcceptQuest();
                    HandleInteraction(); // Hiển thị thông tin nhiệm vụ mới
                }
                else
                {
                    _questToGive = null;
                    HandleInteraction();
                }
            });
        }
        else
        {
            _shouldContinueAfterComplete = false;
            _lastCompletedQuest = null;

            // Tự động nhận nhiệm vụ tiếp theo nếu có
            if (_nextQuestAfterCompletion != null && !questManager.IsQuestTrulyCompleted(_nextQuestAfterCompletion))
            {
                _questToGive = _nextQuestAfterCompletion;
                questManager.AcceptQuest();
                HandleInteraction(); // Hiển thị thông tin nhiệm vụ mới
            }
            else
            {
                _questToGive = null;
                HandleInteraction();
            }
        }
    }

    public void ClaimReward()
    {
        var q = questManager.GetCurrentQuest();
        if (q == null || !(questManager.GetQuestStatus(q)?.isObjectiveMet ?? false))
        {
            return;
        }

        questManager.CompleteQuest();
        _lastCompletedQuest = q;

        _questUI?.SetActive(true);
        HideAllActionButtons();
        MouseManager.Instance.ShowCursorAndDisableInput();

        string[] keys = q.GetDialogueKeys(QuestDialogueType.AfterComplete);
        AudioClip[] clips = q.GetDialogueVoiceClips(QuestDialogueType.AfterComplete);

        if (keys != null && keys.Length > 0)
        {
            dialogueManager.StartDialogue(keys, clips, () =>
            {
                // Sau khi hết thoại, kiểm tra và nhận nhiệm vụ tiếp theo
                if (_nextQuestAfterCompletion != null && !questManager.IsQuestTrulyCompleted(_nextQuestAfterCompletion))
                {
                    _questToGive = _nextQuestAfterCompletion;
                    questManager.AcceptQuest();
                }
                OnDialogueCompleted();
            });
        }
        else
        {
            OnDialogueCompleted();
        }
    }
    public void AcceptCurrentQuest()
    {
        var q = questManager.GetCurrentQuest();
        if (q == null || q != _questToGive) return;

        questManager.AcceptQuest();

        // Cập nhật UI ngay lập tức
        UIManager.Instance?.UpdateQuestProgressText(q.GetQuestNameLocalized());
        UIManager.Instance?.UpdateCurrentQuestObjective(q.GetDescriptionLocalized());

        dialogueManager.StartDialogue(
            q.GetDialogueKeys(QuestDialogueType.ObjectiveMet),
            q.GetDialogueVoiceClips(QuestDialogueType.ObjectiveMet),
            OnDialogueCompleted
        );

        HideAllActionButtons();
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    public void DeclineQuest()
    {
        var q = questManager.GetCurrentQuest();
        if (q == null || q != _questToGive) return;

        questManager.DeclineQuest();
        dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);

        HideAllActionButtons();
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    private QuestData GetQuestToOfferOrManage()
    {
        var currentActiveQuest = questManager.GetCurrentQuest();
        if (currentActiveQuest != null && !questManager.IsQuestTrulyCompleted(currentActiveQuest))
        {
            if (currentActiveQuest.questType == QuestType.FindNPC && currentActiveQuest.targetNPCID == _npcID)
                return currentActiveQuest;

            if (_questToGive != null && _questToGive == currentActiveQuest)
                return _questToGive;
        }

        if (_questToGive != null && !questManager.IsQuestTrulyCompleted(_questToGive))
        {
            return _questToGive;
        }

        return null;
    }

    private QuestData GetFindNPCQuestIfTarget()
    {
        QuestData q = questManager?.GetCurrentQuest();
        return q != null && q.questType == QuestType.FindNPC && q.targetNPCID == _npcID && !questManager.IsQuestTrulyCompleted(q) ? q : null;
    }

    private void ShowDefaultUI()
    {
        _acceptButton?.gameObject.SetActive(true);
        _declineButton?.gameObject.SetActive(true);
        _claimRewardButton?.gameObject.SetActive(false);
        if (_continueButton != null) _continueButton.gameObject.SetActive(false);
    }

    private void ShowClaimRewardUI()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(true);
        if (_continueButton != null) _continueButton.gameObject.SetActive(false);
    }

    private void HideAllActionButtons()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(false);
        if (_continueButton != null) _continueButton.gameObject.SetActive(false);
    }

    private void OnDialogueCompleted()
    {
        if (_continueButton != null) _continueButton.gameObject.SetActive(false);
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            _questUI?.SetActive(false);
            dialogueManager?.EndDialogue();
            MouseManager.Instance.HideCursorAndEnableInput();

            _shouldContinueAfterComplete = false;
            _lastCompletedQuest = null;
        }
    }
}
