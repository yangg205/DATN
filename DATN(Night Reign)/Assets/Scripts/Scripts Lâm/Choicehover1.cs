﻿using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Choicehover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;

    [SerializeField] private Outline outline;

    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        if (text != null)
            text.color = normalColor;

        if (outline == null)
            outline = GetComponent<Outline>() ?? GetComponentInChildren<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (outline is null)
            Debug.LogWarning($"Outline is missing on {gameObject.name}");
    }

    void Update()
    {
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

    public void OnPointerClick(PointerEventData eventData)
    {
        ChoiceManager.Instance.Select(this);
    }

    public void SetSelected(bool isSelected)
    {
        if (outline != null)
            outline.enabled = isSelected;
    }

    void OnDisable()
    {
        isHovered = false;
        transform.localScale = originalScale;
        if (text != null)
            text.color = normalColor;
        if (outline != null)
            outline.enabled = false;
    }
}