using UnityEngine;

public class FollowHeadTarget : MonoBehaviour
{
    public Transform headBone;        // Kéo xương đầu (Head) vào đây
    public Vector3 localOffset = new Vector3(0, 0, 0.3f); // Đẩy ra trước mặt nếu muốn

    void LateUpdate()
    {
        if (headBone == null)
            return;

        transform.position = headBone.position + headBone.rotation * localOffset;
        transform.rotation = headBone.rotation;
    }
}
