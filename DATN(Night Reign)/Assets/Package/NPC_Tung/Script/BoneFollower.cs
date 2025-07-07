using UnityEngine;

public class BoneFollower : MonoBehaviour
{
    // Biến public để gán xương mục tiêu từ Inspector
    [Tooltip("Kéo xương bạn muốn theo dõi vào đây từ Hierarchy.")]
    public Transform targetBone;

    // Sử dụng [SerializeField] để biến vẫn private nhưng có thể chỉnh sửa trong Inspector
    [SerializeField]
    [Tooltip("Khoảng cách dịch chuyển của tên so với xương. Điều chỉnh để tên nằm ở vị trí mong muốn.")]
    private Vector3 offset = Vector3.zero;

    [SerializeField]
    [Tooltip("Có nên xoay tên theo hướng của xương không?")]
    private bool followRotation = false;

    // Biến private, chỉ dùng nội bộ script, không hiển thị trong Inspector (trừ khi có [SerializeField])
    // Đã bỏ [Tooltip] và đổi lại thành private không [SerializeField] vì đây là biến debug
    private Vector3 debugBoneWorldPosition; // Không cần hiển thị trong Inspector khi không debug nữa

    void LateUpdate()
    {
        // Kiểm tra xem xương mục tiêu đã được gán chưa
        if (targetBone == null)
        {
            Debug.LogWarning("BoneFollower: Target Bone chưa được gán! Vui lòng kéo xương vào trường Target Bone trong Inspector.");
            return;
        }

        // Cập nhật vị trí của Game Object chứa Canvas
        transform.position = targetBone.position + offset;

        // Nếu muốn tên xoay theo xương
        if (followRotation)
        {
            transform.rotation = targetBone.rotation;
        }

        // Cập nhật giá trị debug (chỉ dùng nội bộ, không hiển thị mặc định trong Inspector)
        // debugBoneWorldPosition = targetBone.position; // Bỏ dòng này nếu không muốn debug
    }
}