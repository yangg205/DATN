//﻿using UnityEngine;
//using System.Collections.Generic;

//public class ChoiceManager : MonoBehaviour
//{
//    public static ChoiceManager Instance;

//    private Choicehover currentSelected;

//    void Awake()
//    {
//        // Singleton pattern để dùng dễ dàng
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    public void Select(Choicehover newSelection)
//    {
//        // Bỏ chọn cái cũ
//        if (currentSelected != null && currentSelected != newSelection)
//            currentSelected.SetSelected(false);

//        // Cập nhật cái mới
//        currentSelected = newSelection;
//        currentSelected.SetSelected(true);
//    }
//}