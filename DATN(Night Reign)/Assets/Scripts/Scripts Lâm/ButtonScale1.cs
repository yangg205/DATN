//﻿using UnityEngine;
//using UnityEngine.EventSystems;

//public class ButtonScale : MonoBehaviour
//{
//    public ButtonScale buttonScale;
//    private Vector3 originalScale;
//    private Vector3 hoverScale;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        originalScale = transform.localScale; // Lưu scale ban đầu
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        transform.localScale = hoverScale; // Phóng to khi hover
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        transform.localScale = originalScale; // Quay lại scale ban đầu khi rời
//    }
//}