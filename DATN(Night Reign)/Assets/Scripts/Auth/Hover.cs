using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;
    SignalRClient signalRClient;
    private Vector3 originalScale;
    private bool isHovered = false;
    NotificationManager notificationManager;

    void Start()
    {
        originalScale = transform.localScale;
        if (text != null)
            text.color = normalColor;
        signalRClient = FindAnyObjectByType<SignalRClient>();
        notificationManager = FindAnyObjectByType<NotificationManager>();
    }

    void Update()
    {
        // Smooth scale animation
        Vector3 targetScale = isHovered ? originalScale * scaleAmount : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (text != null)
            text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (text != null)
            text.color = normalColor;
    }

    void OnDisable()
    {
        // Reset khi menu bị tắt
        isHovered = false;
        transform.localScale = originalScale;
        if (text != null)
            text.color = normalColor;
    }
    public async void OnClickName()
    {
        var name = text.text.Trim();
        var result  = await signalRClient.UpdateName(PlayerPrefs.GetString("email"),name);
        if (result.status)
        {
           notificationManager.ShowNotification(result.message,4);
        }
        else
        {
            notificationManager.ShowNotification(result.message, 4);

        }
    }
}
