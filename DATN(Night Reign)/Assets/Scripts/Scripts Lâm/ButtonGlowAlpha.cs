using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonGlowAlpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Glow Image")]
    public RawImage glowImage; 

    [Header("Alpha Settings")]
    public float enterAlpha = 1f;
    public float exitAlpha = 0f;
    public float fadeSpeed = 10f;

    private float targetAlpha;

    void Awake()
    {
        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 0f; // start alpha = 0
            glowImage.color = c;
        }
        targetAlpha = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetAlpha = enterAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetAlpha = exitAlpha;
    }

    void Update()
    {
        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            glowImage.color = c;
        }
    }
}
