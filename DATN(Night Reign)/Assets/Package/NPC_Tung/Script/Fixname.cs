using UnityEngine;
using ND; // Đảm bảo namespace ND được import nếu CameraHandler của bạn nằm trong đó

/// <summary>
/// Script này được gắn vào GameObject chứa Canvas và Text của tên NPC.
/// Nó sẽ buộc vị trí của bảng tên luôn nằm trên một xương cụ thể của NPC (Head Bone)
/// và xoay để nhìn về phía camera.
/// </summary>
public class NPCNameplateFixedPositioner : MonoBehaviour
{
    [Header("Cấu hình Vị trí Bảng Tên")]
    [Tooltip("Kéo Transform của xương đầu (hoặc xương bạn muốn tên theo) từ NPC vào đây.")]
    public Transform headBoneTarget; // Kéo thả xương head vào đây

    [Tooltip("Khoảng cách dịch chuyển của bảng tên so với vị trí của Head Bone. Điều chỉnh X, Y, Z để tên nằm ở vị trí mong muốn.")]
    [SerializeField]
    private Vector3 nameplateOffset = new Vector3(0, 0.5f, 0); // Offset mặc định, bạn cần điều chỉnh trong Inspector

    // Biến nội bộ để tham chiếu đến Transform của camera người chơi (dùng cho billboard)
    private Transform playerCameraTransform;

    void Start()
    {
        // Kiểm tra xem Head Bone đã được gán chưa
        if (headBoneTarget == null)
        {
            Debug.LogError("NPCNameplateFixedPositioner: 'Head Bone Target' chưa được gán! Bảng tên sẽ không định vị được.", this);
            enabled = false; // Tắt script nếu không có Head Bone để theo dõi
            return;
        }

        // Tìm CameraHandler trong scene để lấy Transform của camera người chơi.
        var cameraHandler = FindObjectOfType<ND.CameraHandler>();
        if (cameraHandler != null)
        {
            playerCameraTransform = cameraHandler.cameraTransform;
        }
        else
        {
            Debug.LogWarning("NPCNameplateFixedPositioner: CameraHandler không tìm thấy trong scene. Chức năng xoay Billboard sẽ không hoạt động.", this);
        }
    }

    void LateUpdate() // Sử dụng LateUpdate để đảm bảo mọi chuyển động của NPC và camera đã hoàn tất
    {
        // --- Cập nhật Vị trí Bảng Tên ---
        if (headBoneTarget != null)
        {
            // Đặt vị trí của bảng tên bằng cách lấy vị trí thế giới của Head Bone + offset
            transform.position = headBoneTarget.position + nameplateOffset;
        }

        // --- Cập nhật Xoay Bảng Tên (Billboard) ---
        // 'transform' ở đây là Transform của GameObject chứa script này (tức là GameObject 'name' của bạn)
        if (playerCameraTransform != null)
        {
            // Tính toán hướng từ vị trí HIỆN TẠI của bảng tên đến camera
            Vector3 directionToCamera = playerCameraTransform.position - transform.position;

            // Giữ cho trục Y của hướng nhìn bằng 0 để bảng tên luôn thẳng đứng
            directionToCamera.y = 0;

            if (directionToCamera != Vector3.zero)
            {
                // Xoay bảng tên để nó nhìn về phía camera
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }
}
