using AG; // Đảm bảo namespace ND được import để tìm thấy CameraHandler
using UnityEngine;

public class BillboardNameplate : MonoBehaviour
{
    // Trường để kéo thả GameObject mà bạn muốn xoay (ví dụ: Canvas chứa tên NPC).
    // Nếu để trống, script sẽ tự động xoay GameObject mà nó được gắn vào.
    [Tooltip("Kéo GameObject mà bạn muốn xoay để nó luôn nhìn về phía camera (ví dụ: Canvas chứa tên NPC). Nếu để trống, script sẽ tự động xoay GameObject mà nó được gắn vào.")]
    [SerializeField] private GameObject objectToRotate;

    // Phạm vi khoảng cách để tên NPC hiển thị.
    // Nếu người chơi lại gần hơn khoảng cách này, tên sẽ hiện.
    [Tooltip("Phạm vi khoảng cách tính từ người chơi để tên NPC hiển thị. Nếu người chơi lại gần hơn khoảng cách này, tên sẽ hiện.")]
    [SerializeField] private float detectionRange = 5f; // Mặc định 5 mét

    private CameraHandler playerCameraHandler;

    void Awake()
    {
        // Nếu objectToRotate không được gán từ Inspector, mặc định sẽ là GameObject mà script này gắn vào.
        // Điều này cho phép script linh hoạt: có thể gắn trực tiếp vào Canvas hoặc vào một Manager và kéo Canvas vào.
        if (objectToRotate == null)
        {
            objectToRotate = this.gameObject;
        }
    }

    void Start()
    {
        // Kiểm tra xem objectToRotate đã được gán hoặc tự động gán chưa.
        // Mặc dù Awake đã cố gắng gán, nhưng vẫn kiểm tra để an toàn.
        if (objectToRotate == null)
        {
            Debug.LogError("BillboardNameplate: 'Object To Rotate' field is not assigned and could not be automatically set on GameObject: " + gameObject.name + ". Script will be disabled.", this);
            enabled = false; // Tắt script nếu không có GameObject để xoay
            return; // Thoát Start sớm
        }

        // Ban đầu ẩn tên đi.
        // Điều này đảm bảo tên không hiển thị ngay lập tức khi bắt đầu trò chơi.
        objectToRotate.SetActive(false);

        // Tìm CameraHandler trong scene.
        playerCameraHandler = FindObjectOfType<CameraHandler>();
        if (playerCameraHandler == null)
        {
            Debug.LogWarning("BillboardNameplate: CameraHandler not found in scene on GameObject: " + gameObject.name + ". Billboard rotation and proximity visibility will not function.", this);
            // Lưu ý: Không disable script ở đây. Các chức năng phụ thuộc vào CameraHandler sẽ không chạy,
            // nhưng các phần khác của script (nếu có) sẽ vẫn hoạt động.
            // Để an toàn, các kiểm tra null trong LateUpdate sẽ đảm bảo không có lỗi.
        }
    }

    void LateUpdate()
    {
        // Chỉ thực hiện logic nếu có đủ các tham chiếu cần thiết
        if (objectToRotate != null && playerCameraHandler != null && playerCameraHandler.cameraTransform != null)
        {
            // Lấy vị trí camera (thường là vị trí người chơi hoặc gần người chơi)
            Vector3 camPosition = playerCameraHandler.cameraTransform.position;

            // Lấy vị trí của đối tượng cần xoay (tên NPC)
            Vector3 objectPosition = objectToRotate.transform.position;

            // Tính khoảng cách giữa camera và đối tượng cần xoay (tên NPC)
            float distance = Vector3.Distance(camPosition, objectPosition);

            // Kiểm tra khoảng cách để quyết định hiển thị hay ẩn tên
            if (distance <= detectionRange)
            {
                // Nếu tên đang ẩn, bật nó lên
                if (!objectToRotate.activeSelf)
                {
                    objectToRotate.SetActive(true);
                }

                // Logic xoay tên về phía camera (billboard)
                // Đảm bảo tên luôn thẳng đứng bằng cách chỉ nhìn vào trục XZ của camera
                Vector3 lookAtTarget = new Vector3(camPosition.x, objectPosition.y, camPosition.z);
                objectToRotate.transform.LookAt(lookAtTarget);
                objectToRotate.transform.Rotate(0, 180f, 0); // Đảo ngược nếu cần (tùy thuộc vào model)
            }
            else
            {
                // Nếu tên đang hiển thị, ẩn nó đi
                if (objectToRotate.activeSelf)
                {
                    objectToRotate.SetActive(false);
                }
            }
        }
        // Nếu objectToRotate, playerCameraHandler hoặc cameraTransform là null, hàm sẽ không làm gì.
        // Thông báo lỗi/cảnh báo đã được xử lý trong Start().
    }
}