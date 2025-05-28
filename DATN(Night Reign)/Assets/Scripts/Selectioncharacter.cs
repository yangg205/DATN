using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Selectioncharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject border; // gán viền vàng object

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleSpeed = 8f;

    private bool isSelected = false;
    private bool isHovered = false;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        if (border != null)
            border.SetActive(false);
    }

    private void Update()
    {
        if (border != null)
            Debug.Log("Border active: " + border.activeSelf);

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    //public void ShowBorder(bool show)
    //{
    //    if (border != null)
    //        border.SetActive(show);
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!isSelected)
        {
            targetScale = originalScale * 1.1f;
            if (border != null)
                border.SetActive(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isSelected)
        {
            targetScale = originalScale;
            if (border != null)
                border.SetActive(false);
        }
    }

    [System.Obsolete]
    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = true;
        targetScale = originalScale * 1.15f;
        if (border != null)
            border.SetActive(true);
        foreach (var other in
        // Deselect những cái khác
        from Selectioncharacter other in FindObjectsOfType<Selectioncharacter>()
        where other != this
        select other)
        {
            other.Deselect();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        targetScale = originalScale;
        if (!isHovered && border != null)
            border.SetActive(false);
    }
}
