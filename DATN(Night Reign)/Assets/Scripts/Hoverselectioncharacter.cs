using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Hoverselectioncharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 targetScale;
    private Vector3 originalScale;
    private float scaleSpeed = 8f;

    private bool isHovered = false;
    private bool isSelected = false;

    private Outline outline;

    [Header("UI Panel hiển thị khi hover")]
    public GameObject hoverPanel;
    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private Coroutine panelAnimCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        if (hoverPanel != null)
        {
            panelCanvasGroup = hoverPanel.GetComponent<CanvasGroup>();
            panelRectTransform = hoverPanel.GetComponent<RectTransform>();

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }

            if (panelRectTransform != null)
                panelRectTransform.localScale = Vector3.one * 0.8f;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!isSelected)
        {
            targetScale = originalScale * 1.1f;
            if (outline != null)
            {
                outline.effectColor = Color.yellow;
                outline.enabled = true;
            }
        }

        if (hoverPanel != null)
        {
            if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
            panelAnimCoroutine = StartCoroutine(ShowPanelSmooth(true));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isSelected)
        {
            targetScale = originalScale;
            if (outline != null)
                outline.enabled = false;
        }

        if (hoverPanel != null)
        {
            if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
            panelAnimCoroutine = StartCoroutine(ShowPanelSmooth(false));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = true;
        targetScale = originalScale * 1.15f;
        if (outline != null)
        {
            outline.effectColor = new Color(1f, 0.84f, 0f); // Gold
            outline.enabled = true;
        }

        foreach (Hoverselectioncharacter other in FindObjectsOfType<Hoverselectioncharacter>())
        {
            if (other != this)
                other.Deselect();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        targetScale = originalScale;
        if (!isHovered && outline != null)
            outline.enabled = false;
    }

    private IEnumerator ShowPanelSmooth(bool show)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        float startAlpha = panelCanvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;

        Vector3 startScale = panelRectTransform.localScale;
        Vector3 targetScale = show ? Vector3.one : Vector3.one * 0.8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (panelRectTransform != null)
                panelRectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = targetAlpha;
            panelCanvasGroup.interactable = show;
            panelCanvasGroup.blocksRaycasts = show;
        }

        panelAnimCoroutine = null;
    }
}
