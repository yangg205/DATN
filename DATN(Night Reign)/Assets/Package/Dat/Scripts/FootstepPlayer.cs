using System.Collections.Generic;
using UnityEngine;
using ND; // Đảm bảo namespace này được thêm vào để truy cập các script ND

public class FootstepPlayer : MonoBehaviour
{
    // ... (Giữ nguyên các biến đã có)
    [Header("Raycast Foot Positions")]
    public Transform footL;
    public Transform footR;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip defaultFootstepClip;

    [System.Serializable]
    public class MaterialFootstepData
    {
        public string materialName;  // Tên của PhysicsMaterial HOẶC tên của Terrain Layer
        public AudioClip clip;      // Clip tương ứng
    }

    [Header("Footstep Clips by Material Name")]
    public List<MaterialFootstepData> footstepClips;
    private Dictionary<string, AudioClip> materialToClipMap;

    [Header("Settings")]
    public float stepRate = 0.5f;
    public float raycastDistance = 1f;
    public LayerMask groundMask;

    private float stepTimerL;
    private float stepTimerR;
    private bool leftFootDown = false;
    private bool rightFootDown = false;

    [Header("References")]
    public Animator animatorRef; // Đã có
    public Rigidbody rb; // Đã có

    // THÊM BIẾN NÀY CHO TERRAIN
    [Header("Terrain References")]
    public Terrain activeTerrain;

    // THÊM REFERENCE TỚI PLAYERLOCOMOTION và ANIMATORHANDLER
    private PlayerLocomotion playerLocomotion;
    private AnimatorHandler animatorHandler; // Đây là animatorHandler của bạn, không phải anim trực tiếp trên Animator

    void Awake()
    {
        // Gán các reference cần thiết
        if (animatorRef == null) animatorRef = GetComponent<Animator>(); // Animator component chính
        if (rb == null) rb = GetComponent<Rigidbody>(); // Rigidbody component chính
        playerLocomotion = GetComponent<PlayerLocomotion>(); // Lấy PlayerLocomotion từ cùng GameObject
        animatorHandler = GetComponentInChildren<AnimatorHandler>(); // Lấy AnimatorHandler từ Children

        // Khởi tạo materialToClipMap
        materialToClipMap = new Dictionary<string, AudioClip>();
        foreach (var entry in footstepClips)
        {
            if (!materialToClipMap.ContainsKey(entry.materialName) && entry.clip != null)
            {
                materialToClipMap.Add(entry.materialName, entry.clip);
            }
            else if (entry.clip == null)
            {
                Debug.LogWarning($"[FootstepPlayer] MaterialFootstepData for '{entry.materialName}' has a null AudioClip.", this);
            }
        }

        // Kiểm tra AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("FootstepPlayer requires an AudioSource component on the same GameObject or assigned in Inspector.", this);
            }
        }

        // Tìm Terrain nếu chưa được gán (quan trọng cho Terrain footsteps)
        if (activeTerrain == null)
        {
            FindAnyObjectByType<Terrain>();
            if (activeTerrain != null)
            {
                Debug.Log($"[FootstepPlayer] Found Terrain '{activeTerrain.name}' in scene and assigned.", this);
            }
            else
            {
                Debug.LogWarning("[FootstepPlayer] No Terrain found in scene. Terrain footstep sounds will not work.", this);
            }
        }
    }

    void Update()
    {
        // Sử dụng IsMoving() mới thay vì IsWalking()
        if (IsMoving())
        {
            HandleFoot(footL, ref leftFootDown, ref stepTimerL);
            HandleFoot(footR, ref rightFootDown, ref stepTimerR);
        }
        else
        {
            leftFootDown = false;
            rightFootDown = false;
            // Reset timers khi đứng yên để không phát ra âm thanh ngay lập tức khi bắt đầu di chuyển
            stepTimerL = 0;
            stepTimerR = 0;
        }
    }

    void HandleFoot(Transform foot, ref bool footDown, ref float stepTimer)
    {
        Debug.DrawRay(foot.position + Vector3.up * 0.1f, Vector3.down * raycastDistance, Color.red);

        // Đảm bảo Raycast đủ dài và hit đúng layer
        if (Physics.Raycast(foot.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            if (!footDown && stepTimer <= 0f)
            {
                PlayFootstep(hit);
                footDown = true;
                stepTimer = stepRate; // Reset timer sau khi phát âm thanh
            }
        }
        else
        {
            footDown = false; // Chân không chạm đất
        }

        // Giảm timer theo thời gian
        stepTimer -= Time.deltaTime;
    }

    void PlayFootstep(RaycastHit hit)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = defaultFootstepClip;
        string detectedSurfaceName = "Default";

        // Ưu tiên kiểm tra PhysicsMaterial
        PhysicsMaterial hitPhysicsMaterial = hit.collider.sharedMaterial;
        if (hitPhysicsMaterial != null)
        {
            // Debug.Log($"[Footstep] Hit PhysicsMaterial: '{hitPhysicsMaterial.name}' on object '{hit.collider.name}'.", hit.collider.gameObject);
            // Lấy tên vật liệu, loại bỏ "(Instance)" nếu có
            string materialKey = hitPhysicsMaterial.name.Replace(" (Instance)", "");
            if (materialToClipMap.TryGetValue(materialKey, out AudioClip matchedClip))
            {
                clipToPlay = matchedClip;
                detectedSurfaceName = materialKey;
            }
            else
            {
                Debug.LogWarning($"[Footstep] PhysicsMaterial '{materialKey}' found, but no corresponding AudioClip defined in Footstep Clips list. Using default.", this);
            }
        }        // Nếu không có PhysicsMaterial, kiểm tra xem có phải Terrain không
        else if (hit.collider is TerrainCollider)
        {
            // Debug.Log($"[Footstep] Hit TerrainCollider on object '{hit.collider.name}'.", hit.collider.gameObject);
            string terrainLayerName = GetTerrainTextureName(hit.point);
            if (!string.IsNullOrEmpty(terrainLayerName))
            {
                // Debug.Log($"[Footstep] Detected Terrain Layer: '{terrainLayerName}' at {hit.point}.", hit.collider.gameObject);
                if (materialToClipMap.TryGetValue(terrainLayerName, out AudioClip matchedClip))
                {
                    clipToPlay = matchedClip;
                    detectedSurfaceName = terrainLayerName;
                }
                else
                {
                    Debug.LogWarning($"[Footstep] Terrain Layer '{terrainLayerName}' found, but no corresponding AudioClip defined in Footstep Clips list. Using default.", this);
                }
            }
            else
            {
                Debug.LogWarning($"[Footstep] Could not get Terrain Layer name at hit point. Using default.", this);
            }
        }
        // Trường hợp còn lại: không có PhysicsMaterial và không phải Terrain
        else
        {
            Debug.LogWarning($"[Footstep] Raycast hit object '{hit.collider.name}' but collider has no PhysicsMaterial and is not a TerrainCollider. Using default footstep clip.", hit.collider.gameObject);
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
            // Debug.Log($"[Footstep] Played '{clipToPlay.name}' for '{detectedSurfaceName}' at {hit.point} (Object: {hit.collider.name})", hit.collider.gameObject);
        }
        else
        {
            Debug.LogWarning("[Footstep] Final clipToPlay is null, cannot play sound. Default clip might be missing.", this);
        }
    }

    // HÀM LẤY TÊN TERRAIN LAYER (giữ nguyên hoặc cập nhật như đã hướng dẫn)
    private string GetTerrainTextureName(Vector3 worldPos)
    {
        if (activeTerrain == null)
        {
            // Debug.LogWarning("Active Terrain not assigned in FootstepPlayer. Cannot detect terrain layer.", this);
            return null;
        }

        TerrainData terrainData = activeTerrain.terrainData;
        Vector3 terrainPos = activeTerrain.transform.position;

        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

        float[,,] alphaMap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        string textureName = null;
        float maxWeight = 0f;
        int dominantTextureIndex = 0;

        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (alphaMap[0, 0, i] > maxWeight)
            {
                maxWeight = alphaMap[0, 0, i];
                dominantTextureIndex = i;
            }
        }

        if (terrainData.terrainLayers != null && terrainData.terrainLayers.Length > dominantTextureIndex)
        {
            textureName = terrainData.terrainLayers[dominantTextureIndex].name;
        }

        return textureName;
    }

    // HÀM MỚI ĐỂ XÁC ĐỊNH KHI NÀO NHÂN VẬT ĐANG DI CHUYỂN
    bool IsMoving()
    {
        // Cách 1: Dựa vào input amount (tốt cho di chuyển có kiểm soát)
        if (playerLocomotion != null && playerLocomotion.inputHandler != null)
        {
            // Nếu có di chuyển horizontal hoặc vertical input
            if (playerLocomotion.inputHandler.moveAmount > 0.1f) // Một ngưỡng nhỏ để tránh nhiễu
            {
                // Thêm điều kiện không phải đang lăn (roll) hoặc tương tác (interacting)
                if (!playerLocomotion.animatorHandler.anim.GetBool("isInteracting") && !playerLocomotion.inputHandler.rollFlag)
                {
                    return true;
                }
            }
        }

        // Cách 2: Dựa vào vận tốc của Rigidbody (tốt nếu di chuyển không chỉ do input, ví dụ: đẩy, trượt)
        if (rb != null)
        {
            if (rb.linearVelocity.magnitude > 0.1f) // Một ngưỡng nhỏ
            {
                // Kiểm tra xem nhân vật có đang trên mặt đất không, để tránh tiếng bước chân khi bay/rơi
                if (playerLocomotion != null && playerLocomotion.playerManager.isGrounded)
                {
                    // Lọc thêm các trường hợp không mong muốn (ví dụ: đang nhảy)
                    if (!animatorRef.GetBool("isJumping") && !animatorRef.GetBool("isFalling")) // Giả sử bạn có các bool này trong animator
                    {
                        return true;
                    }
                }
            }
        }

        // Cách 3: Dựa vào Animator (cuối cùng)
        // Đây là cách bạn đã dùng, nhưng cần đảm bảo parameter "Speed" luôn được cập nhật chính xác
        if (animatorRef != null)
        {
            // "Speed" thường là kết quả của moveAmount. Nếu bạn đã có moveAmount > 0.1f, thì cách này có thể hơi dư
            // Tuy nhiên, nếu animation của bạn có các chuyển động nhỏ không do input trực tiếp, thì nó vẫn hữu ích.
            float speed = animatorRef.GetFloat("Vertical"); // Hoặc "Speed" nếu bạn có parameter đó
            // Debug.Log($"[FootstepPlayer] Animator Vertical: {speed}");
            return Mathf.Abs(speed) > 0.1f; // Kiểm tra cả tiến và lùi
        }

        return false;
    }
}