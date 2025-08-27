using ND;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LargeMapController : MonoBehaviour
{
    [Header("Teleport Effect")]
    public GameObject teleportEffect;

    [Header("Teleport Offset")]
    public float playerOffsetY = 1.0f;     // Nâng player lên để tránh kẹt collider
    public float effectOffsetY = 0.0f;     // Hiệu ứng sát mặt đất

    [Header("UI Confirm Panel")]
    public GameObject confirmTeleportPanel;
    public Button yesButton;
    public Button noButton;

    private string pendingCheckpointId = null;

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

        float terrainAspect = terrainsBounds.size.x / terrainsBounds.size.z;
        Rect mapRect = largeMapDisplayRaw.rectTransform.rect;
        float uiAspect = mapRect.width / mapRect.height;

        float requiredSize;
        if (terrainAspect > uiAspect)
        {
            requiredSize = terrainsBounds.size.x / 2f;
        }
        else
        {
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
        largeMapCamera.aspect = uiAspect;

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
                if (confirmTeleportPanel != null) confirmTeleportPanel.SetActive(false);
            }

            SetPlayerInputLocked(isActive);
        }
    }

    private void HandleWaypointCreationInput()
    {
        Camera uiCam = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ?
                       null : largeMapDisplayRaw.canvas.worldCamera;

        Vector2 localPoint;
        bool validPointOnMapUI = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCam, out localPoint);

        if (!validPointOnMapUI) return;

        // --- Nếu đang trỏ vào 1 UI Button bất kỳ thì NHƯỜNG cho UI xử lý ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                // Nếu là Button thì return luôn (dừng waypoint)
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return; // 🚀 Nhường cho Button xử lý OnClick
                }
            }
        }

        // --- Nếu không click vào Button thì mới cho tạo / xóa waypoint ---
        if (Input.GetMouseButtonDown(0))
        {
            CreateWaypointAtUIPosition(localPoint);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TryRemoveWaypointAtUIPosition(localPoint);
        }
    }



    private void CreateWaypointAtUIPosition(Vector2 localPoint)
    {
        Rect rect = largeMapDisplayRaw.rectTransform.rect;
        float normX = (localPoint.x / rect.width) + 0.5f;
        float normY = (localPoint.y / rect.height) + 0.5f;

        if (invertMapY) normY = 1f - normY;

        Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
        int mask = terrainLayerMask == 0 ? ~0 : terrainLayerMask;

        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, mask))
        {
            Vector3 hitPoint = hit.point;

            Waypoint newWaypoint = new Waypoint(
                currentCustomWaypointId,
                "Điểm đánh dấu tùy chỉnh",
                hitPoint,
                WaypointType.CustomMarker,
                customWaypointIcon
            );

            WaypointManager.Instance.AddWaypoint(newWaypoint, true);
            UpdateWaypointUIsOnLargeMap();

            Debug.Log($"✅ Waypoint tạo tại {hitPoint} (normX={normX:F2}, normY={normY:F2})");
        }
        else
        {
            Debug.LogWarning($"❌ Raycast không trúng gì (normX={normX:F2}, normY={normY:F2})");
        }
    }

    private void TryRemoveWaypointAtUIPosition(Vector2 localPoint)
    {
        Rect rect = largeMapDisplayRaw.rectTransform.rect;
        float normX = (localPoint.x / rect.width) + 0.5f;
        float normY = (localPoint.y / rect.height) + 0.5f;

        if (invertMapY) normY = 1f - normY;

        Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
        int mask = terrainLayerMask == 0 ? ~0 : terrainLayerMask;

        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, mask))
        {
            Vector3 hitPoint = hit.point;

            float minDist = 10f;
            string targetId = null;

            foreach (var wp in WaypointManager.Instance.activeWaypointsData.Values)
            {
                float dist = Vector3.Distance(hitPoint, wp.worldPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetId = wp.id;
                }
            }

            if (!string.IsNullOrEmpty(targetId))
            {
                WaypointManager.Instance.RemoveWaypoint(targetId);
                UpdateWaypointUIsOnLargeMap();
                Debug.Log($"🗑️ Đã xóa waypoint: {targetId}");
            }
            else
            {
                Debug.Log("Không có waypoint nào gần click để xóa.");
            }
        }
    }

    private Vector3 ViewportToWorldPosition(float normX, float normY)
    {
        if (largeMapCamera == null || terrainsBounds.size == Vector3.zero)
            return Vector3.zero;

        float worldX = Mathf.Lerp(terrainsBounds.min.x, terrainsBounds.max.x, normX);
        float worldZ = Mathf.Lerp(terrainsBounds.min.z, terrainsBounds.max.z, normY);

        Vector3 rayStart = new Vector3(worldX, terrainsBounds.max.y + 50f, worldZ);
        RaycastHit hit;

        int expandedLayerMask = terrainLayerMask;
        if (expandedLayerMask == 0) expandedLayerMask = ~0;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 1000f, expandedLayerMask))
        {
            return hit.point;
        }

        float terrainHeight = GetTerrainHeightAtPosition(worldX, worldZ);
        return new Vector3(worldX, terrainHeight, worldZ);
    }

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

    private Vector2 ConvertWorldPositionToMapUI(Vector3 worldPos)
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null || terrainsBounds.size == Vector3.zero)
        {
            Debug.LogWarning("[LargeMapController] Cannot convert world position to UI: Missing references or zero bounds.");
            return Vector2.zero;
        }

        Vector3 relativePos = worldPos - terrainsBounds.center;
        float normX = relativePos.x / terrainsBounds.size.x;
        float normZ = relativePos.z / terrainsBounds.size.z;

        normX = Mathf.Clamp(normX, -0.5f, 0.5f);
        normZ = Mathf.Clamp(normZ, -0.5f, 0.5f);

        if (invertMapY) normZ = -normZ;

        Rect mapRect = largeMapDisplayRaw.rectTransform.rect;
        float uiX = normX * mapRect.width;
        float uiY = normZ * mapRect.height;

        return new Vector2(uiX, uiY);
    }

    private Vector2 ConvertWorldToUIPosition(Vector3 worldPos, RectTransform targetParent = null)
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null || terrainsBounds.size == Vector3.zero)
        {
            Debug.LogWarning("Thiếu tham chiếu để chuyển đổi tọa độ");
            return Vector2.zero;
        }

        if (targetParent == null)
            targetParent = largeMapDisplayRaw.rectTransform;

        Vector3 viewportPos = largeMapCamera.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0)
        {
            Debug.LogWarning($"Vị trí {worldPos} nằm sau camera");
            return Vector2.zero;
        }

        if (invertMapY)
            viewportPos.y = 1f - viewportPos.y;

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

        foreach (var cp in checkpointList)
        {
            if (cp == null || cp.checkpointTransform == null || cp.iconPrefab == null) continue;

            string id = cp.checkpointTransform.gameObject.name;
            if (checkpointUIMap.ContainsKey(id)) continue;

            Button btn = Instantiate(cp.iconPrefab, largeMapPanel.transform);
            cp.runtimeIcon = btn;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnCheckpointClicked(id));

            RectTransform rect = btn.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            var hoverTooltip = btn.gameObject.GetComponent<CheckpointTooltip>();
            if (hoverTooltip == null) hoverTooltip = btn.gameObject.AddComponent<CheckpointTooltip>();
            hoverTooltip.Initialize(id);

            var wpUI = btn.gameObject.GetComponent<WaypointUI>();
            if (wpUI == null) wpUI = btn.gameObject.AddComponent<WaypointUI>();

            Waypoint wpData = new Waypoint(
                id,
                id,
                cp.checkpointTransform.position,
                WaypointType.Checkpoint,
                btn.image != null ? btn.image.sprite : null
            );

            wpUI.SetData(wpData);
            wpUI.maxScale = 1f;

            checkpointUIMap[id] = wpUI;

            Debug.Log($"✅ Initialized checkpoint UI from prefab: {id}");
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

        pendingCheckpointId = checkpointId;

        if (confirmTeleportPanel != null)
            confirmTeleportPanel.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            TeleportPlayerToCheckpoint(checkpoint);
            CloseConfirmPanel();
        });

        noButton.onClick.AddListener(() =>
        {
            CloseConfirmPanel();
        });
    }

    private void CloseConfirmPanel()
    {
        if (confirmTeleportPanel != null)
            confirmTeleportPanel.SetActive(false);

        pendingCheckpointId = null;
    }

    private void TeleportPlayerToCheckpoint(CheckpointData checkpoint)
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null)
        {
            Debug.LogError("Không thể dịch chuyển - thiếu tham chiếu player");
            return;
        }

        Vector3 targetPosition = checkpoint.GetWorldPosition() + Vector3.up * playerOffsetY;
        WaypointManager.Instance.playerTransform.position = targetPosition;

        if (teleportEffect != null)
        {
            Vector3 effectPos = checkpoint.GetWorldPosition() + Vector3.up * effectOffsetY;
            GameObject fx = Instantiate(teleportEffect, effectPos, Quaternion.identity);

            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(fx, ps.main.duration);
            else
                Destroy(fx, 3f);
        }

        Debug.Log($"✨ Đã dịch chuyển player đến checkpoint: {checkpoint.checkpointTransform.name} tại vị trí: {targetPosition}");
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
            Destroy(marker, 5f);
        }
    }

    private void ValidateAspectRatio()
    {
        if (largeMapCamera == null || largeMapDisplayRaw == null) return;

        RenderTexture renderTex = largeMapCamera.targetTexture;
        if (renderTex == null)
        {
            Debug.LogError("largeMapCamera thiếu RenderTexture!");
            return;
        }

        float terrainAspect = terrainsBounds.size.x / terrainsBounds.size.z;
        float renderTexAspect = (float)renderTex.width / renderTex.height;
        float rawImageAspect = largeMapDisplayRaw.rectTransform.rect.width /
                              largeMapDisplayRaw.rectTransform.rect.height;

        Debug.Log($"Terrain Aspect: {terrainAspect:F2}, RenderTexture Aspect: {renderTexAspect:F2}, RawImage Aspect: {rawImageAspect:F2}");

        if (Mathf.Abs(renderTexAspect - rawImageAspect) > 0.1f)
        {
            Debug.LogWarning("Aspect ratio không khớp! Điều này sẽ gây lệch vị trí.");
        }
    }

    [ContextMenu("Debug Map Alignment")]
    private void DebugMapAlignment()
    {
        if (largeMapDisplayRaw == null || largeMapCamera == null) return;

        foreach (Transform child in largeMapDisplayRaw.transform)
        {
            if (child.name.Contains("DebugMarker"))
                DestroyImmediate(child.gameObject);
        }

        Vector3[] testPositions = {
            new Vector3(terrainsBounds.min.x, terrainsBounds.center.y, terrainsBounds.min.z),
            new Vector3(terrainsBounds.max.x, terrainsBounds.center.y, terrainsBounds.min.z),
            new Vector3(terrainsBounds.min.x, terrainsBounds.center.y, terrainsBounds.max.z),
            new Vector3(terrainsBounds.max.x, terrainsBounds.center.y, terrainsBounds.max.z),
            terrainsBounds.center
        };

        string[] labels = { "BL", "BR", "TL", "TR", "C" };
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };

        for (int i = 0; i < testPositions.Length; i++)
        {
            Vector3 worldPos = testPositions[i];
            Vector2 uiPos = ConvertWorldToUIPosition(worldPos);

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
        public Button iconPrefab;
        [HideInInspector] public Button runtimeIcon;

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
}