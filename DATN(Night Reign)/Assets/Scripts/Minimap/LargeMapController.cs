using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ND;

// Không có namespace

public class LargeMapController : MonoBehaviour
{
    // --- UI References ---
    [Header("UI References")]
    public GameObject largeMapPanel;
    public RawImage largeMapDisplayRaw;
    public RectTransform playerIconLargeMapRectTransform;
    public RectTransform largeMapWaypointsParentRectTransform; // Same as WaypointManager's largeMapWaypointsParent

    // --- World References ---
    [Header("World References")]
    public Camera largeMapCamera;
    public Transform playerTransform;
    public Transform playerMainCameraTransform;

    // --- Terrain Configuration ---
    [Header("Terrain Configuration")]
    public Terrain activeTerrain;

    // --- Waypoint Creation ---
    [Header("Waypoint Creation")]
    public Sprite customWaypointIcon;
    public Sprite questLocationIcon;
    private string currentCustomWaypointId = "PlayerCustomMarker"; // ID cố định cho custom marker người chơi

    // --- Internal References & Settings ---
    private InputHandler playerInputHandler; // Không cần namespace YourGame.Core nữa
    private int terrainLayerMask;

    void Awake()
    {
        if (activeTerrain != null)
        {
            // Lấy thông số terrain tự động
            if (WaypointManager.Instance != null)
            {
                WaypointManager.Instance.activeTerrain = activeTerrain; // Đảm bảo WaypointManager có terrain data
            }
            else
            {
                Debug.LogWarning("[LargeMapController] WaypointManager Instance not found in Awake. Terrain might not be set.");
            }
        }
        else
        {
            Debug.LogWarning("[LargeMapController] Active Terrain is not assigned. Please drag your Terrain GameObject into the 'Active Terrain' slot.");
        }
    }

    void Start()
    {
        CheckRequiredReferences();

        playerInputHandler = FindObjectOfType<InputHandler>();
        if (playerInputHandler == null)
        {
            Debug.LogWarning("[LargeMapController] InputHandler not found in scene. Player input cannot be locked/unlocked automatically.");
        }

        int terrainLayerIndex = LayerMask.NameToLayer("Terrain");
        if (terrainLayerIndex == -1)
        {
            Debug.LogError("[LargeMapController] Layer 'Terrain' does NOT exist in Project Settings or name is misspelled! Please create it via Edit > Project Settings > Tags & Layers.");
            terrainLayerMask = 0;
        }
        else
        {
            terrainLayerMask = 1 << terrainLayerIndex;
            Debug.Log($"[LargeMapController] Successfully found 'Terrain' layer at index {terrainLayerIndex}. Calculated LayerMask value: {terrainLayerMask}");
        }

        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
            if (largeMapCamera != null) largeMapCamera.enabled = false;
        }

        SetupLargeMapCamera();

        // Set up player icon parent (should be the same as WaypointManager's largeMapWaypointsParent)
        if (playerIconLargeMapRectTransform != null && largeMapWaypointsParentRectTransform != null)
        {
            playerIconLargeMapRectTransform.SetParent(largeMapWaypointsParentRectTransform, false);
            Debug.Log("[LargeMapController] Player icon parent set to largeMapWaypointsParentRectTransform.");
        }

        // Ví dụ: Tạo một QuestLocation ngay khi bắt đầu game
        // Uncomment dòng này nếu bạn muốn có một QuestLocation mặc định để kiểm tra
        // AddSampleQuestLocation();
    }

    void Update()
    {
        HandleMapToggleInput();

        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            HandleWaypointCreationInput();
        }
    }

    void LateUpdate()
    {
        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            UpdatePlayerIconPositionAndRotation();
        }
    }

    private void CheckRequiredReferences()
    {
        if (largeMapPanel == null) Debug.LogError("[LargeMapController] largeMapPanel is not assigned!");
        if (largeMapDisplayRaw == null) Debug.LogError("[LargeMapController] largeMapDisplayRaw is not assigned!");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("[LargeMapController] playerIconLargeMapRectTransform is not assigned!");
        if (largeMapWaypointsParentRectTransform == null) Debug.LogError("[LargeMapController] largeMapWaypointsParentRectTransform is not assigned! Waypoint UI might not be correctly parented.");
        if (largeMapCamera == null) Debug.LogError("[LargeMapController] largeMapCamera is not assigned!");
        if (playerTransform == null) Debug.LogError("[LargeMapController] playerTransform is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("[LargeMapController] playerMainCameraTransform is not assigned!");
        if (customWaypointIcon == null) Debug.LogWarning("[LargeMapController] Custom Waypoint Icon is not assigned. Default icon will be used or waypoint creation might fail visually.");
        if (questLocationIcon == null) Debug.LogWarning("[LargeMapController] Quest Location Icon is not assigned. AddSampleQuestLocation might not have a proper icon.");
        if (activeTerrain == null) Debug.LogError("[LargeMapController] Active Terrain is not assigned!");
    }

    private void SetupLargeMapCamera()
    {
        if (largeMapCamera == null || activeTerrain == null) return;

        largeMapCamera.orthographic = true;
        // Lấy kích thước terrain từ activeTerrain
        float terrainWidth = activeTerrain.terrainData.size.x;
        float terrainLength = activeTerrain.terrainData.size.z;

        float maxDimension = Mathf.Max(terrainWidth, terrainLength);
        largeMapCamera.orthographicSize = maxDimension / 2f;

        largeMapCamera.transform.position = new Vector3(
            activeTerrain.transform.position.x + terrainWidth / 2f,
            activeTerrain.transform.position.y + maxDimension * 0.75f, // Đặt camera cao hơn terrain
            activeTerrain.transform.position.z + terrainLength / 2f
        );
        largeMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Nhìn thẳng xuống
        Debug.Log($"[LargeMapController] LargeMapCamera configured: Position={largeMapCamera.transform.position}, Orthographic Size={largeMapCamera.orthographicSize}");
    }

    private void HandleMapToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool isActive = !largeMapPanel.activeSelf;
            largeMapPanel.SetActive(isActive);
            if (largeMapCamera != null) largeMapCamera.enabled = isActive;

            if (isActive)
            {
                // Đảm bảo camera được setup lại khi mở bản đồ
                SetupLargeMapCamera();
                UpdatePlayerIconPositionAndRotation();
            }

            SetPlayerInputLocked(isActive);
            Debug.Log($"[LargeMapController] Large Map Toggled: {isActive}");
        }
    }

    private void HandleWaypointCreationInput()
    {
        if (Input.GetMouseButtonDown(0)) // Click chuột trái
        {
            // Kiểm tra xem có click vào UI elements khác không
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Lọc ra các UI element mà con trỏ đang ở trên
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                bool clickedOnMapRawImage = false;
                foreach (RaycastResult r in results)
                {
                    if (r.gameObject == largeMapDisplayRaw.gameObject)
                    {
                        clickedOnMapRawImage = true;
                        break;
                    }
                }

                if (!clickedOnMapRawImage)
                {
                    Debug.Log("[LargeMapController] Click was not directly on the Large Map RawImage. Aborting waypoint creation.");
                    return;
                }
            }

            // Chuyển đổi vị trí click từ Screen Space sang Local Space của RawImage
            Vector2 localPoint;
            Camera uiCamera = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : largeMapDisplayRaw.canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCamera, out localPoint))
            {
                Debug.LogWarning("[LargeMapController] Could not convert screen point to local point on RawImage. Aborting waypoint creation.");
                return;
            }

            // Chuyển đổi Local Point (từ tâm của RectTransform) sang Normalized Viewport (0-1)
            float normalizedX = (localPoint.x / largeMapDisplayRaw.rectTransform.rect.width) + 0.5f;
            float normalizedY = (localPoint.y / largeMapDisplayRaw.rectTransform.rect.height) + 0.5f;

            // Tạo Ray từ Large Map Camera đến điểm trên bản đồ
            Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
            {
                Vector3 hitPoint = hit.point;
                Debug.Log($"[LargeMapController] SUCCESS: Clicked on terrain at: {hitPoint}.");

                if (WaypointManager.Instance != null)
                {
                    Waypoint newWaypoint = new Waypoint(
                        currentCustomWaypointId, // Sử dụng ID cố định để chỉ có một custom marker
                        "Điểm đánh dấu tùy chỉnh",
                        hitPoint,
                        WaypointType.CustomMarker,
                        customWaypointIcon
                    );

                    // Gọi WaypointManager để thêm waypoint, truyền RectTransform của RawImage
                    WaypointManager.Instance.AddWaypoint(newWaypoint, true, largeMapDisplayRaw.rectTransform);
                }
                else
                {
                    Debug.LogWarning("[LargeMapController] WaypointManager Instance not found. Cannot add waypoint.");
                }
            }
            else
            {
                Debug.LogWarning($"[LargeMapController] FAIL: Raycast from largeMapCamera did not hit 'Terrain' layer. Mouse Pos: {Input.mousePosition}. Ray Origin: {ray.origin}, Direction: {ray.direction}");
                Debug.DrawRay(ray.origin, ray.direction * 500, Color.red, 2f);
            }
        }
    }

    // Ví dụ: Hàm để thêm một QuestLocation cố định (có thể gọi từ Start hoặc một sự kiện game)
    public void AddSampleQuestLocation()
    {
        if (WaypointManager.Instance != null && questLocationIcon != null)
        {
            // Đặt một vị trí QuestLocation cố định trong thế giới của bạn
            Vector3 questPos = new Vector3(50f, 0f, 75f); // THAY ĐỔI TỌA ĐỘ NÀY CHO PHÙ HỢP VỚI TERRAIN CỦA BẠN
            string questId = "MainQuest_1";

            Waypoint questWaypoint = new Waypoint(
                questId,
                "Đến đây để hoàn thành",
                questPos,
                WaypointType.QuestLocation,
                questLocationIcon
            );
            WaypointManager.Instance.AddWaypoint(questWaypoint, false, largeMapDisplayRaw.rectTransform);
            Debug.Log($"[LargeMapController] Added sample QuestLocation at {questPos}");
        }
        else
        {
            Debug.LogWarning("[LargeMapController] Cannot add sample QuestLocation. WaypointManager or QuestLocationIcon is null.");
        }
    }

    private void UpdatePlayerIconPositionAndRotation()
    {
        if (playerTransform == null || playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null || activeTerrain == null || playerMainCameraTransform == null)
        {
            return;
        }

        if (largeMapDisplayRaw.rectTransform.rect.width == 0 || largeMapDisplayRaw.rectTransform.rect.height == 0)
        {
            return;
        }

        // Lấy thông số terrain từ WaypointManager hoặc tính toán lại
        float terrainWidth = activeTerrain.terrainData.size.x;
        float terrainLength = activeTerrain.terrainData.size.z;
        float terrainMinX = activeTerrain.transform.position.x;
        float terrainMinZ = activeTerrain.transform.position.z;

        // Tính toán vị trí của người chơi trên bản đồ lớn
        float normalizedX = (playerTransform.position.x - terrainMinX) / terrainWidth;
        float normalizedY = (playerTransform.position.z - terrainMinZ) / terrainLength;

        // Chuyển đổi normalized position sang anchoredPosition trên RectTransform của RawImage
        float uiX = (normalizedX - 0.5f) * largeMapDisplayRaw.rectTransform.rect.width;
        float uiY = (normalizedY - 0.5f) * largeMapDisplayRaw.rectTransform.rect.height;

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);

        // Xoay icon người chơi trên bản đồ để khớp với hướng nhìn của camera chính
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
    }

    private void SetPlayerInputLocked(bool locked)
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.enabled = !locked; // Vô hiệu hóa script InputHandler
        }
        else
        {
            Debug.LogWarning("[LargeMapController] InputHandler is null. Cannot lock/unlock player input.");
        }

        // Quản lý trạng thái con trỏ chuột
        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}