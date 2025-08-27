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
            Debug.Log($"{GO} is not assigned in Toggle");
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
            bool isActive = !GO.activeSelf;
            GO.SetActive(isActive);

            if (isActive)
            {
                // Khi mở UI
                Canvas canvas = GO.GetComponent<Canvas>();
                if (canvas != null) canvas.enabled = true;

                MouseManager.Instance.ShowCursorAndDisableInput();
            }
            else
            {
                // Khi tắt UI
                MouseManager.Instance.HideCursorAndEnableInput();
            }
        }
    }

}