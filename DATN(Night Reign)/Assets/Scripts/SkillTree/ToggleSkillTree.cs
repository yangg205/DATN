using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleSkillTree : MonoBehaviour
{
    public GameObject skillTreeUI;

    void Start()
    {
        // Đảm bảo UI được tắt khi bắt đầu
        if (skillTreeUI != null)
        {
            skillTreeUI.SetActive(false);
        }
        else
        {
            Debug.LogError("skillTreeUI is not assigned in ToggleSkillTree!");
        }

        // Kiểm tra xem có EventSystem trong scene không
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning("No EventSystem found in the scene! Adding a new one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    void Update()
    {
        // Kiểm tra phím P để bật/tắt UI
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleSkill();
        }
    }

    private void ToggleSkill()
    {
        if (skillTreeUI != null)
        {
            skillTreeUI.SetActive(!skillTreeUI.activeSelf);
            // Đảm bảo Canvas được cập nhật khi bật
            if (skillTreeUI.activeSelf)
            {
                Canvas canvas = skillTreeUI.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }
        }
    }
}