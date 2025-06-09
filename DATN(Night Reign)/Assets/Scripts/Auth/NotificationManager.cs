using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] GameObject notificationPanel;
    [SerializeField] TextMeshProUGUI notificationText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Tồn tại qua các Scene
    }

    private void Start()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("Notification panel or text is not set!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisplayNotification(message, duration));
    }

    private IEnumerator DisplayNotification(string message, float duration)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        notificationPanel.SetActive(false);
    }
}
