using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleNR : MonoBehaviour
{
    [SerializeField] GameObject GO;
    [SerializeField] KeyCode key;

    void Start()
    {
        // Đảm bảo UI được tắt khi bắt đầu
        if (GO != null)
        {
            GO.SetActive(false);
        }
        else
        {
            Debug.LogError($"{GO} is not assigned in Toggle");
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
        if (Input.GetKeyDown(key))
        {
            ToggleGame();
        }
    }

    private void ToggleGame()
    {
        if (GO != null)
        {
            GO.SetActive(!GO.activeSelf);
            // Đảm bảo Canvas được cập nhật khi bật
            if (GO.activeSelf)
            {
                Canvas canvas = GO.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }
        }
    }
}