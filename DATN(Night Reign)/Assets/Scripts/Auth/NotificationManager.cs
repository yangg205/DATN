using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] GameObject notificationPanel;
    [SerializeField] TextMeshProUGUI notificationText;
    [SerializeField] Canvas canvas; // Gán Canvas chứa notificationPanel trong Inspector

    private bool isShowingNotification = false;
    private Queue<(string message, float duration)> notificationQueue = new Queue<(string, float)>();
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Another NotificationManager instance found on {gameObject.name}! Destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"NotificationManager Singleton initialized: {gameObject.name}");
    }

    private void Start()
    {
        if (canvas == null)
        {
            canvas = notificationPanel?.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found for notificationPanel!");
            }
        }

        if (notificationPanel == null)
        {
            Debug.LogError("notificationPanel is not assigned in Inspector!");
        }
        if (notificationText == null)
        {
            Debug.LogError("notificationText is not assigned in Inspector!");
        }
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            Debug.Log("notificationPanel initialized as inactive");
        }
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        if (notificationPanel == null || notificationText == null || canvas == null)
        {
            Debug.LogError("Notification panel, text, or canvas is not set!");
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("NotificationManager is disabled or destroyed! Cannot show notification.");
            return;
        }

        notificationQueue.Enqueue((message, duration));
        Debug.Log($"Enqueued notification: {message} for {duration}s (Queue count: {notificationQueue.Count})");

        if (!isShowingNotification)
        {
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            notificationCoroutine = StartCoroutine(ProcessNotificationQueue());
        }
    }

    private IEnumerator ProcessNotificationQueue()
    {
        isShowingNotification = true;

        while (notificationQueue.Count > 0)
        {
            var (message, duration) = notificationQueue.Dequeue();
            Debug.Log($"Processing notification: {message} for {duration}s");

            // Đảm bảo panel tắt trước khi hiển thị thông báo mới
            notificationPanel.SetActive(false);
            notificationText.text = message;
            notificationPanel.SetActive(true);

            yield return new WaitForSecondsRealtime(duration);

            notificationPanel.SetActive(false);
            Debug.Log($"Hid notification: {message}");
        }

        isShowingNotification = false;
        notificationCoroutine = null;
    }

    // Hàm để kiểm tra trạng thái panel khi debug
    public bool IsNotificationPanelActive()
    {
        return notificationPanel != null && notificationPanel.activeSelf;
    }
}