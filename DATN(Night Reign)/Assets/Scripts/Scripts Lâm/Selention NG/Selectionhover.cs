using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Selectionhover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 targetScale;
    private Vector3 originalScale;
    private float scaleSpeed = 8f;

    private bool isHovered = false;
    private bool isSelected = false;

    [SerializeField] private Outline outline;

    [Header("Panel hover chứa 2 button")]
    public GameObject hoverPanel;
    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private Coroutine panelAnimCoroutine;

    [Header("2 button khi hover")]
    public Button button1;
    public Button button2;

    [Header("2 panel sẽ hiện khi bấm button")]
    public GameObject panel1;
    public GameObject panel2;

    public Button panel1CloseButton;
    public Button panel2CloseButton;

    private void Awake()
    {
        if (panel1CloseButton != null)
            panel1CloseButton.onClick.AddListener(() => panel1.SetActive(false));
        if (panel2CloseButton != null)
            panel2CloseButton.onClick.AddListener(() => panel2.SetActive(false));
    }

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Auto tìm outline nếu không gán
        if (outline == null)
            outline = GetComponent<Outline>() ?? GetComponentInChildren<Outline>();

        if (outline != null)
        {
            outline.effectColor = new Color(1f, 1f, 1f, 0.5f); // viền trắng mờ
            outline.effectDistance = new Vector2(5f, 1f);
            outline.enabled = false;
        }
        else
        {
            Debug.LogWarning($"[Selectionhover] Missing Outline on {gameObject.name}");
        }

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

        if (button1 != null)
            button1.onClick.AddListener(ShowPanel1);
        if (button2 != null)
            button2.onClick.AddListener(ShowPanel2);

        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);
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
            targetScale = originalScale * 1.05f;
            if (outline != null)
                outline.enabled = true;
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
        targetScale = originalScale * 1.1f;

        if (outline != null)
            outline.enabled = true;

        // Hủy chọn những cái khác
        foreach (Selectionhover other in FindObjectsOfType<Selectionhover>())
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

    private void ShowPanel1()
    {
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);
    }

    private void ShowPanel2()
    {
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(true);
    }
}
