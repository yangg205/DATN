using ND;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject largeMapPanel;
    public RawImage largeMapDisplayRaw;
    public RectTransform playerIconLargeMapRectTransform;

    [Header("World References")]
    public Camera largeMapCamera;
    public Transform playerMainCameraTransform;

    [Header("Waypoint Creation")]
    public Sprite customWaypointIcon;
    private string currentCustomWaypointId = "PlayerCustomMarker";

    private int terrainLayerMask;
    private InputHandler playerInputHandler;
    private Bounds terrainsBounds;

    [Header("Checkpoint Settings")]
    public float teleportOffsetY = 1.5f;
    public List<CheckpointData> checkpointList = new List<CheckpointData>();

    private Dictionary<string, WaypointUI> checkpointUIMap = new Dictionary<string, WaypointUI>();

    [Header("Debug / Options")]
    [Tooltip("Nếu ảnh map trên RawImage bị lật theo Y khi render, bật lên.")]
    public bool invertMapY = false;

    private void Start()
    {
        CheckRequiredReferences();

#if UNITY_2023_2_OR_NEWER
        playerInputHandler = UnityEngine.Object.FindFirstObjectByType<InputHandler>();
#else
        playerInputHandler = FindObjectOfType<InputHandler>();
#endif

        int terrainLayerIndex = LayerMask.NameToLayer("Terrain");
        terrainLayerMask = (terrainLayerIndex != -1) ? 1 << terrainLayerIndex : 0;

        if (largeMapPanel != null) largeMapPanel.SetActive(false);
        if (largeMapCamera != null) largeMapCamera.enabled = false;

        // Ensure RawImage pivot is centered so anchoredPosition math is simpler
        if (largeMapDisplayRaw != null)
        {
            var rt = largeMapDisplayRaw.rectTransform;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        ValidateAspectRatio();
        CalculateTerrainsBounds();
        SetupLargeMapCamera();
        InitializeCheckpointUIsFromInspector();
        VerifyUIAlignment();
    }

    private void Update()
    {
        HandleMapToggleInput();

        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            HandleWaypointCreationInput();
            UpdatePlayerIconPositionAndRotation();
            UpdateWaypointUIsOnLargeMap();
            UpdateCheckpointUIs();
        }
    }

    private void CheckRequiredReferences()
    {
        if (largeMapPanel == null) Debug.LogError("[LargeMapController] largeMapPanel is not assigned!");
        if (largeMapDisplayRaw == null) Debug.LogError("[LargeMapController] largeMapDisplayRaw is not assigned!");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("[LargeMapController] playerIconLargeMapRectTransform is not assigned!");
        if (largeMapCamera == null) Debug.LogError("[LargeMapController] largeMapCamera is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("[LargeMapController] playerMainCameraTransform is not assigned!");
        if (customWaypointIcon == null) Debug.LogWarning("[LargeMapController] customWaypointIcon is not assigned.");

        if (WaypointManager.Instance == null)
        {
            Debug.LogError("[LargeMapController] WaypointManager.Instance is NULL!");
            return;
        }
        if (WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || WaypointManager.Instance.terrains.Any(t => t == null))
        {
            Debug.LogError("[LargeMapController] WaypointManager.Instance.terrains is invalid!");
        }

        if (WaypointManager.Instance.playerTransform == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.playerTransform is not assigned!");

        if (largeMapCamera != null && largeMapCamera.targetTexture == null)
        {
            Debug.LogError("[LargeMapController] largeMapCamera's Target Texture is not assigned!");
        }
        if (largeMapDisplayRaw != null && largeMapDisplayRaw.texture == null)
        {
            Debug.LogError("[LargeMapController] largeMapDisplayRaw's Texture is not assigned!");
        }
    }

    private void CalculateTerrainsBounds()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0)
        {
            terrainsBounds = new Bounds(Vector3.zero, Vector3.zero);
            Debug.LogWarning("[LargeMapController] No terrains found, setting bounds to zero.");
            return;
        }

        Terrain firstTerrain = WaypointManager.Instance.terrains[0];
        Vector3 terrainSize = firstTerrain.terrainData.size;
        Vector3 terrainCenter = firstTerrain.transform.position + terrainSize / 2f;

        terrainsBounds = new Bounds(terrainCenter, terrainSize);

        foreach (Terrain terrain in WaypointManager.Instance.terrains)
        {
            if (terrain == null) continue;

            Vector3 tSize = terrain.terrainData.size;
            Vector3 tCenter = terrain.transform.position + tSize / 2f;

            terrainsBounds.Encapsulate(new Bounds(tCenter, tSize));
        }

        terrainsBounds.Expand(1.05f);
        Debug.Log($"Terrain bounds: Center={terrainsBounds.center}, Size={terrainsBounds.size}");
    }

    private void SetupLargeMapCamera()
    {
        if (largeMapCamera == null || terrainsBounds.size == Vector3.zero) return;

        // Tính toán orthographic size chính xác dựa trên aspect ratio của RawImage
        float terrainAspect = terrainsBounds.size.x / terrainsBounds.size.z;

        Rect mapRect = largeMapDisplayRaw.rectTransform.rect;
        float uiAspect = mapRect.width / mapRect.height;

        float requiredSize;
        if (terrainAspect > uiAspect)
        {
            // Terrain rộng hơn UI
            requiredSize = terrainsBounds.size.x / 2f;
        }
        else
        {
            // Terrain cao hơn UI
            requiredSize = terrainsBounds.size.z / 2f;
        }

        largeMapCamera.transform.position = new Vector3(
            terrainsBounds.center.x,
            terrainsBounds.max.y + 100f,
            terrainsBounds.center.z
        );

        largeMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        largeMapCamera.orthographic = true;
        largeMapCamera.orthographicSize = requiredSize;
        largeMapCamera.aspect = uiAspect; // Đảm bảo aspect ratio khớp với UI

        Debug.Log($"Camera setup - Position: {largeMapCamera.transform.position}, Orthographic Size: {requiredSize}, Aspect: {uiAspect}");
    }

    private void HandleMapToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool isActive = !largeMapPanel.activeSelf;
            largeMapPanel.SetActive(isActive);

            if (largeMapCamera != null) largeMapCamera.enabled = isActive;
            if (WaypointManager.Instance != null) WaypointManager.Instance.SetLargeMapPanelActive(isActive);

            if (isActive)
            {
                MouseManager.Instance.ShowCursorAndDisableInput();
                CalculateTerrainsBounds();
                SetupLargeMapCamera();
                UpdatePlayerIconPositionAndRotation();
                UpdateWaypointUIsOnLargeMap();
                UpdateCheckpointUIs();
            }
            else
            {
                MouseManager.Instance.HideCursorAndEnableInput();
            }

            SetPlayerInputLocked(isActive);
        }
    }

    private void HandleWaypointCreationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera uiCam = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ?
                          null : largeMapDisplayRaw.canvas.worldCamera;
            Vector2 localPoint;

            bool validPointOnMapUI = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCam, out localPoint);

            if (!validPointOnMapUI) return;

            // Tính normalized coordinates chính xác hơn
            Rect rect = largeMapDisplayRaw.rectTransform.rect;
            float normX = (localPoint.x / rect.width) + 0.5f;
            float normY = (localPoint.y / rect.height) + 0.5f;

            // Clamp để đảm bảo trong phạm vi hợp lệ
            normX = Mathf.Clamp01(normX);
            normY = Mathf.Clamp01(normY);

            // Xử lý flip Y
            if (invertMapY) normY = 1f - normY;

            // Sử dụng ViewportPointToRay (chính xác hơn cho normalized coordinates)
            Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, terrainLayerMask))
            {
                Vector3 hitPoint = hit.point;

                // Debug để kiểm tra
                Debug.Log($"Click UI: {localPoint} -> Norm: ({normX:F3}, {normY:F3}) -> World: {hitPoint}");

                if (WaypointManager.Instance != null)
                {
                    Waypoint newWaypoint = new Waypoint(
                        currentCustomWaypointId,
                        "Điểm đánh dấu tùy chỉnh",
                        hitPoint,
                        WaypointType.CustomMarker,
                        customWaypointIcon
                    );

                    WaypointManager.Instance.AddWaypoint(newWaypoint, true);
                    UpdateWaypointUIsOnLargeMap();
                }
            }
        }
    }


    // Thêm method mới để chuyển đổi viewport sang world position chính xác hơn
    private Vector3 ViewportToWorldPosition(float normX, float normY)
    {
        if (largeMapCamera == null || terrainsBounds.size == Vector3.zero)
            return Vector3.zero;

        // Tính toán world position dựa trên orthographic camera
        float worldX = Mathf.Lerp(terrainsBounds.min.x, terrainsBounds.max.x, normX);
        float worldZ = Mathf.Lerp(terrainsBounds.min.z, terrainsBounds.max.z, normY);

        // Raycast xuống để tìm terrain height
        Vector3 rayStart = new Vector3(worldX, terrainsBounds.max.y + 50f, worldZ);
        RaycastHit hit;

        // Mở rộng layer mask để bao gồm tất cả có thể
        int expandedLayerMask = terrainLayerMask;
        if (expandedLayerMask == 0) expandedLayerMask = ~0; // Nếu terrain layer không được set, sử dụng tất cả layer

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 1000f, expandedLayerMask))
        {
            return hit.point;
        }

        // Fallback: sử dụng terrain height trực tiếp
        float terrainHeight = GetTerrainHeightAtPosition(worldX, worldZ);
        return new Vector3(worldX, terrainHeight, worldZ);
    }

    // Helper method để lấy terrain height
    private float GetTerrainHeightAtPosition(float worldX, float worldZ)
    {
        if (WaypointManager.Instance?.terrains == null) return 0f;

        foreach (Terrain terrain in WaypointManager.Instance.terrains)
        {
            if (terrain == null) continue;

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            if (worldX >= terrainPos.x && worldX <= terrainPos.x + terrainSize.x &&
                worldZ >= terrainPos.z && worldZ <= terrainPos.z + terrainSize.z)
            {
                float relativeX = (worldX - terrainPos.x) / terrainSize.x;
                float relativeZ = (worldZ - terrainPos.z) / terrainSize.z;

                return terrainPos.y + terrain.terrainData.GetInterpolatedHeight(relativeX, relativeZ);
            }
        }

        return 0f;
    }


    private void UpdatePlayerIconPositionAndRotation()
    {
        if (WaypointManager.Instance?.playerTransform == null ||
            playerIconLargeMapRectTransform == null) return;

        Vector2 uiPos = ConvertWorldToUIPosition(
            WaypointManager.Instance.playerTransform.position,
            playerIconLargeMapRectTransform.parent as RectTransform
        );

        playerIconLargeMapRectTransform.anchoredPosition = uiPos;
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0,
            -playerMainCameraTransform.eulerAngles.y);
    }

    private void UpdateWaypointUIsOnLargeMap()
    {
        if (WaypointManager.Instance?.largeMapWaypointUIs == null) return;

        foreach (var entry in WaypointManager.Instance.largeMapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = waypointUI.GetWaypointData();

            if (waypointUI?.gameObject == null || waypointData == null) continue;

            if (WaypointManager.Instance.activeWaypointsData.ContainsKey(waypointData.id))
            {
                Vector2 uiPos = ConvertWorldToUIPosition(
                    waypointData.worldPosition,
                    waypointUI.transform.parent as RectTransform
                );

                if (!waypointUI.gameObject.activeSelf)
                    waypointUI.gameObject.SetActive(true);

                waypointUI.SetMapUIPosition(uiPos, waypointUI.maxScale);
                waypointUI.UpdateDistanceText();
            }
            else
            {
                if (waypointUI.gameObject.activeSelf)
                    waypointUI.gameObject.SetActive(false);
            }
        }
    }


    /// <summary>
    /// Convert world position to anchoredPosition relative to RawImage (center pivot).
    /// Uses largeMapCamera.WorldToViewportPoint for accurate mapping and clamps UI coordinates.
    /// </summary>
    private Vector2 ConvertWorldPositionToMapUI(Vector3 worldPos)
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null || terrainsBounds.size == Vector3.zero)
        {
            Debug.LogWarning("[LargeMapController] Cannot convert world position to UI: Missing references or zero bounds.");
            return Vector2.zero;
        }

        // Sử dụng inverse transform để tính toán chính xác hơn
        Vector3 relativePos = worldPos - terrainsBounds.center;

        // Normalize relative position (-0.5 to 0.5)
        float normX = relativePos.x / terrainsBounds.size.x;
        float normZ = relativePos.z / terrainsBounds.size.z;

        // Clamp to valid range
        normX = Mathf.Clamp(normX, -0.5f, 0.5f);
        normZ = Mathf.Clamp(normZ, -0.5f, 0.5f);

        // Apply Y inversion if needed
        if (invertMapY) normZ = -normZ;

        // Convert to UI coordinates
        Rect mapRect = largeMapDisplayRaw.rectTransform.rect;
        float uiX = normX * mapRect.width;
        float uiY = normZ * mapRect.height;

        return new Vector2(uiX, uiY);
    }
    /// <summary>
    /// Chuyển đổi vị trí thế giới sang tọa độ UI với độ chính xác cao
    /// </summary>
    private Vector2 ConvertWorldToUIPosition(Vector3 worldPos, RectTransform targetParent = null)
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null || terrainsBounds.size == Vector3.zero)
        {
            Debug.LogWarning("Thiếu tham chiếu để chuyển đổi tọa độ");
            return Vector2.zero;
        }

        // Sử dụng RawImage làm parent mặc định
        if (targetParent == null)
            targetParent = largeMapDisplayRaw.rectTransform;

        // Chuyển đổi sang viewport coordinates (0-1)
        Vector3 viewportPos = largeMapCamera.WorldToViewportPoint(worldPos);

        // Kiểm tra điểm có trong tầm nhìn camera
        if (viewportPos.z < 0)
        {
            Debug.LogWarning($"Vị trí {worldPos} nằm sau camera");
            return Vector2.zero;
        }

        // Xử lý lật Y nếu cần
        if (invertMapY)
            viewportPos.y = 1f - viewportPos.y;

        // Chuyển đổi sang UI coordinates
        Rect targetRect = targetParent.rect;
        float uiX = (viewportPos.x - 0.5f) * targetRect.width;
        float uiY = (viewportPos.y - 0.5f) * targetRect.height;

        return new Vector2(uiX, uiY);
    }



    private void SetPlayerInputLocked(bool locked)
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.enabled = !locked;
        }
        else
        {
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

    private void InitializeCheckpointUIsFromInspector()
    {
        if (checkpointList == null || checkpointList.Count == 0) return;

        Transform waypointsParent = (WaypointManager.Instance != null && WaypointManager.Instance.largeMapWaypointsParent != null)
            ? WaypointManager.Instance.largeMapWaypointsParent
            : (largeMapDisplayRaw != null ? largeMapDisplayRaw.transform : null);

        if (waypointsParent == null) return;

        foreach (var cp in checkpointList)
        {
            if (cp == null || cp.checkpointTransform == null) continue;

            string id = cp.checkpointTransform.gameObject.name;
            if (checkpointUIMap.ContainsKey(id)) continue;

            GameObject iconObj = new GameObject("Checkpoint_" + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            iconObj.transform.SetParent(waypointsParent, false);

            RectTransform rect = iconObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32, 32);
            rect.pivot = new Vector2(0.5f, 0.5f); // Đảm bảo pivot ở center
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero; // Reset position

            Image img = iconObj.GetComponent<Image>();
            img.raycastTarget = true;
            if (cp.icon != null) img.sprite = cp.icon; // Sử dụng icon từ checkpoint data

            Button btn = iconObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCheckpointClicked(id));

            var hoverTooltip = iconObj.AddComponent<CheckpointTooltip>();
            hoverTooltip.Initialize(cp.checkpointTransform.gameObject.name);

            WaypointUI wpUI = iconObj.AddComponent<WaypointUI>();

            // Lấy vị trí chính xác của checkpoint
            Vector3 checkpointWorldPos = cp.GetWorldPosition();

            Waypoint wpData = new Waypoint(id, id, checkpointWorldPos, WaypointType.Checkpoint, img.sprite);
            wpUI.SetData(wpData);
            wpUI.maxScale = 1f; // Đảm bảo scale cố định

            checkpointUIMap[id] = wpUI;

            Debug.Log($"Initialized checkpoint UI: {id} at world position: {checkpointWorldPos}");
        }
    }


    private void OnCheckpointClicked(string checkpointId)
    {
        Debug.Log($"Clicked on checkpoint: {checkpointId}");

        var checkpoint = checkpointList.Find(c => c != null &&
                                        c.checkpointTransform != null &&
                                        c.checkpointTransform.gameObject.name == checkpointId);

        if (checkpoint == null)
        {
            Debug.LogError($"Không tìm thấy checkpoint với ID: {checkpointId}");
            return;
        }

        TeleportPlayerToCheckpoint(checkpoint);

        if (largeMapPanel != null) largeMapPanel.SetActive(false);
        if (largeMapCamera != null) largeMapCamera.enabled = false;

        SetPlayerInputLocked(false);
    }

    private void TeleportPlayerToCheckpoint(CheckpointData checkpoint)
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null)
        {
            Debug.LogError("Không thể dịch chuyển - thiếu tham chiếu player");
            return;
        }

        Vector3 targetPosition = checkpoint.GetWorldPosition() + Vector3.up * teleportOffsetY;
        WaypointManager.Instance.playerTransform.position = targetPosition;

        Debug.Log($"Đã dịch chuyển player đến checkpoint: {checkpoint.checkpointTransform.name} tại vị trí: {targetPosition}");
    }

    private void UpdateCheckpointUIs()
    {
        if (checkpointUIMap == null || checkpointList == null) return;

        foreach (var kvp in checkpointUIMap)
        {
            var checkpoint = checkpointList.Find(c => c?.checkpointTransform != null &&
                                                c.checkpointTransform.gameObject.name == kvp.Key);
            if (checkpoint == null) continue;

            Vector3 worldPos = checkpoint.GetWorldPosition();
            Vector2 uiPos = ConvertWorldToUIPosition(
                worldPos,
                kvp.Value.transform.parent as RectTransform
            );

            kvp.Value.SetMapUIPosition(uiPos, 1f);

            if (!kvp.Value.gameObject.activeSelf)
                kvp.Value.gameObject.SetActive(true);
        }
    }

    private void VerifyUIAlignment()
    {
        if (playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null) return;

        Vector3[] testPositions = new Vector3[]
        {
            terrainsBounds.min,
            terrainsBounds.max,
            terrainsBounds.center,
            new Vector3(terrainsBounds.min.x, 0, terrainsBounds.max.z),
            new Vector3(terrainsBounds.max.x, 0, terrainsBounds.min.z)
        };

        foreach (Vector3 pos in testPositions)
        {
            Vector2 uiPos = ConvertWorldPositionToMapUI(pos);
            Debug.Log($"Kiểm tra căn chỉnh - Thế giới: {pos} -> UI: {uiPos}");

            GameObject marker = new GameObject("DebugMarker");
            marker.transform.SetParent(largeMapDisplayRaw.transform, false);
            RectTransform rt = marker.AddComponent<RectTransform>();
            rt.anchoredPosition = uiPos;
            rt.sizeDelta = new Vector2(10, 10);
            //marker.AddComponent<Image>().color = Color.red;
            Destroy(marker, 5f);
        }
    }
    private void ValidateAspectRatio()
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null) return;

        // Kiểm tra RenderTexture
        RenderTexture renderTex = largeMapCamera.targetTexture;
        if (renderTex == null)
        {
            Debug.LogError("largeMapCamera thiếu RenderTexture!");
            return;
        }

        // Tính aspect ratio của terrain
        float terrainAspect = terrainsBounds.size.x / terrainsBounds.size.z;

        // Tính aspect ratio hiện tại
        float renderTexAspect = (float)renderTex.width / renderTex.height;
        float rawImageAspect = largeMapDisplayRaw.rectTransform.rect.width /
                              largeMapDisplayRaw.rectTransform.rect.height;

        Debug.Log($"Terrain Aspect: {terrainAspect:F2}, RenderTexture Aspect: {renderTexAspect:F2}, RawImage Aspect: {rawImageAspect:F2}");

        // Cảnh báo nếu không khớp
        if (Mathf.Abs(renderTexAspect - rawImageAspect) > 0.1f)
        {
            Debug.LogWarning("Aspect ratio không khớp! Điều này sẽ gây lệch vị trí.");
        }
    }
    [ContextMenu("Debug Map Alignment")]
    private void DebugMapAlignment()
    {
        if (largeMapDisplayRaw == null || largeMapCamera == null) return;

        // Xóa marker cũ
        foreach (Transform child in largeMapDisplayRaw.transform)
        {
            if (child.name.Contains("DebugMarker"))
                DestroyImmediate(child.gameObject);
        }

        Vector3[] testPositions = {
        new Vector3(terrainsBounds.min.x, terrainsBounds.center.y, terrainsBounds.min.z), // Bottom-Left
        new Vector3(terrainsBounds.max.x, terrainsBounds.center.y, terrainsBounds.min.z), // Bottom-Right  
        new Vector3(terrainsBounds.min.x, terrainsBounds.center.y, terrainsBounds.max.z), // Top-Left
        new Vector3(terrainsBounds.max.x, terrainsBounds.center.y, terrainsBounds.max.z), // Top-Right
        terrainsBounds.center // Center
    };

        string[] labels = { "BL", "BR", "TL", "TR", "C" };
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };

        for (int i = 0; i < testPositions.Length; i++)
        {
            Vector3 worldPos = testPositions[i];
            Vector2 uiPos = ConvertWorldToUIPosition(worldPos);

            // Tạo marker
            GameObject marker = new GameObject($"DebugMarker_{labels[i]}");
            marker.transform.SetParent(largeMapDisplayRaw.transform, false);

            RectTransform rt = marker.AddComponent<RectTransform>();
            rt.anchoredPosition = uiPos;
            rt.sizeDelta = new Vector2(20, 20);

            Image img = marker.AddComponent<Image>();
            img.color = colors[i];

            Debug.Log($"{labels[i]}: World({worldPos.x:F1}, {worldPos.z:F1}) -> UI({uiPos.x:F1}, {uiPos.y:F1})");
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (terrainsBounds.size != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(terrainsBounds.center, terrainsBounds.size);
        }
    }

    [System.Serializable]
    public class CheckpointData
    {
        public Transform checkpointTransform;
        public Sprite icon;

        public Vector3 GetWorldPosition()
        {
            if (checkpointTransform == null)
            {
                Debug.LogError("[CheckpointData] checkpointTransform chưa được gán!");
                return Vector3.zero;
            }
            return checkpointTransform.position;
        }
    }

    public class CheckpointTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string checkpointName;
        private GameObject tooltipObj;

        public void Initialize(string name)
        {
            checkpointName = name;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipObj = new GameObject("Tooltip_" + checkpointName);
            tooltipObj.transform.SetParent(transform, false);

            var text = tooltipObj.AddComponent<Text>();
            text.text = checkpointName;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            var rect = tooltipObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 30);
            rect.anchoredPosition = new Vector2(0, 40);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipObj != null)
            {
                Destroy(tooltipObj);
            }
        }
    }

    // --- Thêm biến và UI mới ---
    [Header("Teleport UI")]
    public GameObject teleportButtonPrefab; // Kéo prefab nút dịch chuyển vào
    private GameObject activeTeleportButton;
    private string pendingTeleportCheckpointId;

    // --- Thêm hàm này để mở UI nút dịch chuyển ---
    private void ShowTeleportButton(string checkpointId)
    {
        // Xóa nút cũ nếu có
        if (activeTeleportButton != null)
        {
            Destroy(activeTeleportButton);
            activeTeleportButton = null;
        }

        pendingTeleportCheckpointId = checkpointId;

        if (teleportButtonPrefab != null && largeMapPanel != null)
        {
            activeTeleportButton = Instantiate(teleportButtonPrefab, largeMapPanel.transform);
            activeTeleportButton.transform.SetAsLastSibling(); // Đảm bảo nằm trên cùng UI

            // Gán sự kiện click
            Button btn = activeTeleportButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnTeleportButtonClicked);
            }
        }
        else
        {
            Debug.LogWarning("Chưa gán prefab nút dịch chuyển hoặc largeMapPanel!");
        }
    }

    // --- Khi nhấn nút dịch chuyển ---
    private void OnTeleportButtonClicked()
    {
        if (string.IsNullOrEmpty(pendingTeleportCheckpointId)) return;

        var checkpoint = checkpointList.Find(c => c != null &&
                                        c.checkpointTransform != null &&
                                        c.checkpointTransform.gameObject.name == pendingTeleportCheckpointId);

        if (checkpoint != null)
        {
            TeleportPlayerToCheckpoint(checkpoint);
        }

        // Ẩn panel và nút
        if (activeTeleportButton != null)
        {
            Destroy(activeTeleportButton);
            activeTeleportButton = null;
        }

        if (largeMapPanel != null) largeMapPanel.SetActive(false);
        if (largeMapCamera != null) largeMapCamera.enabled = false;
        SetPlayerInputLocked(false);
    }

    // --- Gọi ShowTeleportButton khi click checkpoint ---
    // Thêm hàm này để giữ nguyên code cũ nhưng bổ sung chức năng
    private void OnCheckpointClicked_OpenTeleportUI(string checkpointId)
    {
        Debug.Log($"Clicked on checkpoint (mở UI): {checkpointId}");
        ShowTeleportButton(checkpointId);
    }

}