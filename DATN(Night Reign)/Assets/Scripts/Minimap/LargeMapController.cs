using ND;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo và thả GameObject Panel chính của bản đồ lớn vào đây từ Hierarchy.")]
    public GameObject largeMapPanel;
    [Tooltip("Kéo và thả RawImage sẽ hiển thị bản đồ từ camera vào đây từ Hierarchy.")]
    public RawImage largeMapDisplayRaw;
    [Tooltip("Kéo và thả RectTransform của icon người chơi trên bản đồ lớn vào đây từ Hierarchy. Đảm bảo nó là con của LargeMap_Display_Raw.")]
    public RectTransform playerIconLargeMapRectTransform;

    [Header("World References")]
    [Tooltip("Kéo và thả Camera sẽ render bản đồ lớn vào đây từ Hierarchy. Đảm bảo Target Texture của camera này được đặt thành một RenderTexture và RenderTexture đó được gán cho Large Map Display Raw.")]
    public Camera largeMapCamera;
    [Tooltip("Kéo và thả Transform của người chơi chính (hoặc camera chính của người chơi) vào đây.")]
    public Transform playerMainCameraTransform;

    [Header("Waypoint Creation")]
    [Tooltip("Sprite sẽ được sử dụng cho các waypoint tùy chỉnh do người chơi đặt.")]
    public Sprite customWaypointIcon;
    private string currentCustomWaypointId = "PlayerCustomMarker";

    private int terrainLayerMask;
    private InputHandler playerInputHandler;
    private Bounds terrainsBounds;

    [Header("Checkpoint Settings")]
    [Tooltip("Icon mặc định cho checkpoint nếu bạn không đặt riêng trong mỗi mục")]
    public Sprite checkpointIcon;
    [Tooltip("Khoảng cách đặt lại player khi dịch chuyển (tránh dính vào collider).")]
    public float teleportOffsetY = 1.5f;
    [Tooltip("Kéo các GameObject checkpoint trong Scene vào danh sách này (bắt buộc). Không nhập tay ID hay tọa độ.")]
    public List<CheckpointData> checkpointList = new List<CheckpointData>();

    private Dictionary<string, WaypointUI> checkpointUIMap = new Dictionary<string, WaypointUI>();

    private void Start()
    {
        CheckRequiredReferences();

#if UNITY_2023_2_OR_NEWER
        playerInputHandler = UnityEngine.Object.FindFirstObjectByType<InputHandler>();
#else
        playerInputHandler = FindObjectOfType<InputHandler>();
#endif

        if (playerInputHandler == null)
        {
            Debug.LogWarning("[LargeMapController] InputHandler not found in scene.");
        }

        int terrainLayerIndex = LayerMask.NameToLayer("Terrain");
        terrainLayerMask = (terrainLayerIndex != -1) ? 1 << terrainLayerIndex : 0;

        if (largeMapPanel != null) largeMapPanel.SetActive(false);
        if (largeMapCamera != null) largeMapCamera.enabled = false;

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
        if (WaypointManager.Instance.largeMapWaypointsParent == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.largeMapWaypointsParent is not assigned!");

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
        Debug.Log($"Kích thước terrain: Tâm={terrainsBounds.center}, Kích thước={terrainsBounds.size}");
    }

    private void SetupLargeMapCamera()
    {
        if (largeMapCamera == null || terrainsBounds.size == Vector3.zero) return;

        float requiredSize = Mathf.Max(terrainsBounds.size.x, terrainsBounds.size.z) / 2f;

        largeMapCamera.transform.position = new Vector3(
            terrainsBounds.center.x,
            terrainsBounds.max.y + 100f,
            terrainsBounds.center.z
        );

        largeMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        largeMapCamera.orthographicSize = requiredSize;

        Debug.Log($"Thiết lập camera - Vị trí: {largeMapCamera.transform.position}\n" +
                 $"Kích thước: {requiredSize}\n" +
                 $"Kích thước terrain: {terrainsBounds.size.x}x{terrainsBounds.size.z}");
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
            Camera uiCam = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : largeMapDisplayRaw.canvas.worldCamera;
            Vector2 localPoint;
            bool validPointOnMapUI = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCam, out localPoint);

            if (!validPointOnMapUI) return;

            float normX = (localPoint.x / largeMapDisplayRaw.rectTransform.rect.width) + 0.5f;
            float normY = (localPoint.y / largeMapDisplayRaw.rectTransform.rect.height) + 0.5f;

            Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, terrainLayerMask))
            {
                Vector3 hitPoint = hit.point;
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

    private void UpdatePlayerIconPositionAndRotation()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null ||
            playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null ||
            WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 ||
            playerMainCameraTransform == null || terrainsBounds.size == Vector3.zero)
        {
            return;
        }

        Transform playerTransform = WaypointManager.Instance.playerTransform;

        float normalizedX = Mathf.InverseLerp(terrainsBounds.min.x, terrainsBounds.max.x, playerTransform.position.x);
        float normalizedZ = Mathf.InverseLerp(terrainsBounds.min.z, terrainsBounds.max.z, playerTransform.position.z);

        RectTransform mapRectTransform = largeMapDisplayRaw.rectTransform;
        float uiX = (normalizedX - 0.5f) * mapRectTransform.rect.width;
        float uiY = (normalizedZ - 0.5f) * mapRectTransform.rect.height;

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
    }

    private void UpdateWaypointUIsOnLargeMap()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.largeMapWaypointUIs == null ||
            largeMapDisplayRaw == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || terrainsBounds.size == Vector3.zero) return;

        foreach (var entry in WaypointManager.Instance.largeMapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = entry.Value.GetWaypointData();

            if (waypointUI == null || waypointUI.gameObject == null) continue;

            if (WaypointManager.Instance.activeWaypointsData.ContainsKey(waypointData.id))
            {
                Vector2 uiPos = ConvertWorldPositionToMapUI(waypointData.worldPosition);
                float scale = waypointUI.maxScale;
                if (!waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(true);
                waypointUI.SetMapUIPosition(uiPos, scale);
                waypointUI.UpdateDistanceText();
            }
            else
            {
                if (waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(false);
            }
        }
    }

    private Vector2 ConvertWorldPositionToMapUI(Vector3 worldPos)
    {
        if (terrainsBounds.size == Vector3.zero || largeMapDisplayRaw == null)
        {
            Debug.LogWarning("[Bản đồ] Không thể chuyển đổi tọa độ");
            return Vector2.zero;
        }

        float normalizedX = Mathf.InverseLerp(terrainsBounds.min.x, terrainsBounds.max.x, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(terrainsBounds.min.z, terrainsBounds.max.z, worldPos.z);

        Rect mapRect = largeMapDisplayRaw.rectTransform.rect;
        float uiX = (normalizedX - 0.5f) * mapRect.width;
        float uiY = (normalizedZ - 0.5f) * mapRect.height;

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
        if (WaypointManager.Instance == null || WaypointManager.Instance.largeMapWaypointsParent == null) return;

        foreach (var cp in checkpointList)
        {
            if (cp == null || cp.checkpointTransform == null) continue;

            string id = cp.checkpointTransform.gameObject.name;
            if (checkpointUIMap.ContainsKey(id)) continue;

            GameObject iconObj = new GameObject("Checkpoint_" + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            iconObj.transform.SetParent(WaypointManager.Instance.largeMapWaypointsParent, false);

            RectTransform rect = iconObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32, 32);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Image img = iconObj.GetComponent<Image>();
            img.sprite = cp.icon != null ? cp.icon : checkpointIcon;
            img.raycastTarget = true;

            Button btn = iconObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCheckpointClicked(id));

            var hoverTooltip = iconObj.AddComponent<CheckpointTooltip>();
            hoverTooltip.Initialize(cp.checkpointTransform.gameObject.name);

            WaypointUI wpUI = iconObj.AddComponent<WaypointUI>();
            Waypoint wpData = new Waypoint(id, id, cp.GetWorldPosition(), WaypointType.Checkpoint, img.sprite);
            wpUI.SetData(wpData);

            checkpointUIMap[id] = wpUI;
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
            var cp = checkpointList.Find(c => c != null &&
                                    c.checkpointTransform != null &&
                                    c.checkpointTransform.gameObject.name == kvp.Key);
            if (cp == null) continue;

            Vector3 worldPos = cp.GetWorldPosition();
            Vector2 uiPos = ConvertWorldPositionToMapUI(worldPos);
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
            marker.transform.SetParent(largeMapDisplayRaw.transform);
            RectTransform rt = marker.AddComponent<RectTransform>();
            rt.anchoredPosition = uiPos;
            rt.sizeDelta = new Vector2(10, 10);
            marker.AddComponent<Image>().color = Color.red;
            Destroy(marker, 5f);
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
        [Tooltip("Kéo GameObject Checkpoint trong Scene vào đây (bắt buộc). ID và tên sẽ lấy từ tên GameObject.")]
        public Transform checkpointTransform;

        [Tooltip("Tuỳ chọn: icon riêng cho checkpoint (để trống sẽ dùng checkpointIcon chung)")]
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
            tooltipObj.transform.SetParent(transform);

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (checkpointList != null)
        {
            foreach (var cp in checkpointList)
            {
                if (cp != null && cp.checkpointTransform != null && cp.icon == null)
                {
                    cp.icon = checkpointIcon;
                }
            }
        }
    }
#endif
}