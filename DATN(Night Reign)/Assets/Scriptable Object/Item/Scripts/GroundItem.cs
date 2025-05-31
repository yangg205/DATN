using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Cần thiết cho EditorUtility và OnValidate
#endif

public class GroundItem : MonoBehaviour // Không cần ISerializationCallbackReceiver nữa cho mục đích này
{
    public ItemObject item; // ItemObject ScriptableObject chứa thông tin item

    // Biến để lưu trữ visual được tạo ra (hoặc tham chiếu đến sprite renderer con)
    private GameObject currentVisualInstance;
    // Tùy chọn: Nếu bạn muốn có một SpriteRenderer con cố định, có thể gán vào đây
    // Nếu không, script sẽ cố gắng tìm hoặc tạo một SpriteRenderer con nếu cần
    public SpriteRenderer spriteRendererChild;

    void Awake()
    {
        UpdateVisuals();
    }

#if UNITY_EDITOR
    // Được gọi trong Editor khi script được tải hoặc một giá trị thay đổi trong Inspector
    void OnValidate()
    {
        // Dùng EditorApplication.delayCall để tránh lỗi khi thay đổi prefab trong OnValidate
        // và đảm bảo hàm chạy sau khi các thay đổi khác của Unity đã được xử lý.
        if (this != null && gameObject != null && item != null) // Kiểm tra để tránh lỗi khi component mới được thêm
        {
            EditorApplication.delayCall += _UpdateVisualsSafely;
        }
    }

    private void _UpdateVisualsSafely()
    {
        // Kiểm tra xem đối tượng còn tồn tại không trước khi chạy
        if (this != null && gameObject != null)
        {
            UpdateVisuals();
        }
    }
#endif

    // Hàm này sẽ được gọi để cập nhật hình ảnh của GroundItem
    public void UpdateVisuals()
    {
        // 1. Dọn dẹp visual cũ
        if (currentVisualInstance != null)
        {
            // Nếu currentVisualInstance là spriteRendererChild.gameObject, không hủy nó, chỉ tắt hoặc xóa sprite
            if (spriteRendererChild != null && currentVisualInstance == spriteRendererChild.gameObject)
            {
                spriteRendererChild.sprite = null; // Xóa sprite
            }
            else // Nếu là prefab được instantiate
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(currentVisualInstance);
                else
                    Destroy(currentVisualInstance);
#else
                Destroy(currentVisualInstance);
#endif
            }
            currentVisualInstance = null;
        }
        // Nếu spriteRendererChild được gán sẵn và không phải là currentVisualInstance đã bị hủy, reset nó
        else if (spriteRendererChild != null)
        {
            spriteRendererChild.sprite = null;
            spriteRendererChild.gameObject.SetActive(false); // Mặc định tắt đi
        }


        if (item == null)
        {
            // Nếu không có item, đảm bảo không có gì hiển thị
            if (spriteRendererChild != null) spriteRendererChild.gameObject.SetActive(false);
            return;
        }

        // 2. Tạo visual mới dựa trên ItemObject
        // Ưu tiên 1: World Item Prefab
        if (item.worldItemPrefab != null)
        {
            currentVisualInstance = Instantiate(item.worldItemPrefab, transform);
            currentVisualInstance.transform.localPosition = Vector3.zero; // Reset vị trí tương đối
            currentVisualInstance.transform.localRotation = Quaternion.identity; // Reset xoay tương đối

            // Nếu có spriteRendererChild riêng, tắt nó đi để không bị chồng chéo
            if (spriteRendererChild != null && spriteRendererChild.transform.parent == transform)
            {
                spriteRendererChild.gameObject.SetActive(false);
            }
        }
        // Ưu tiên 2: World Item Sprite
        else if (item.worldItemSprite != null)
        {
            EnsureSpriteRendererChild(); // Đảm bảo có SpriteRenderer con
            spriteRendererChild.gameObject.SetActive(true);
            spriteRendererChild.sprite = item.worldItemSprite;
            currentVisualInstance = spriteRendererChild.gameObject; // Coi như đây là visual hiện tại
        }
        // Ưu tiên 3: Fallback về uiDisplay Sprite
        else if (item.uiDisplay != null)
        {
            EnsureSpriteRendererChild();
            spriteRendererChild.gameObject.SetActive(true);
            spriteRendererChild.sprite = item.uiDisplay;
            currentVisualInstance = spriteRendererChild.gameObject;
        }
        else
        {
            // Không có gì để hiển thị
            if (spriteRendererChild != null) spriteRendererChild.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Đánh dấu đối tượng này là "dirty" để Unity Editor biết rằng nó đã thay đổi và cần được lưu.
            EditorUtility.SetDirty(this);
            if (spriteRendererChild != null) EditorUtility.SetDirty(spriteRendererChild);
        }
#endif
    }

    // Đảm bảo có một SpriteRenderer con để hiển thị sprite
    private void EnsureSpriteRendererChild()
    {
        if (spriteRendererChild == null)
        {
            spriteRendererChild = GetComponentInChildren<SpriteRenderer>();
            // Nếu không tìm thấy, hoặc tìm thấy nhưng không phải là con trực tiếp (có thể là con của prefab vừa instantiate), tạo mới
            if (spriteRendererChild == null || spriteRendererChild.transform.parent != transform)
            {
                GameObject spriteGO = new GameObject("Sprite_Visual_Child");
                spriteGO.transform.SetParent(transform);
                spriteGO.transform.localPosition = Vector3.zero;
                spriteGO.transform.localRotation = Quaternion.identity;
                spriteRendererChild = spriteGO.AddComponent<SpriteRenderer>();
            }
        }
    }

    // ISerializationCallbackReceiver không còn cần thiết cho việc cập nhật sprite này nữa.
    // OnBeforeSerialize và OnAfterDeserialize có thể được xóa nếu không có mục đích nào khác.
    // public void OnAfterDeserialize() { }
    // public void OnBeforeSerialize() { } // Xóa nội dung cũ của hàm này
}