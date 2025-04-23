using Fusion;
using UnityEngine;

public class DuyMove : NetworkBehaviour
{
    // Di chuyển, camera
    public Transform cameraTransform;
    public CharacterController controller;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpForce = 7f;
    public float gravity = 10f; // Trọng lực vừa phải
    public float fallMultiplier = 2.5f; // Tăng tốc độ rơi khi đang rơi

    // Hoạt ảnh
    public Animator animator;
    [Networked, OnChangedRender(nameof(OnSpeedChanged))]
    public float AnimatorSpeed { get; set; }

    private Vector3 moveDirection = Vector3.zero;
    public bool canMove = true;

    void OnSpeedChanged()
    {
        animator.SetFloat("Speed", AnimatorSpeed);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        // Lấy hướng từ camera thay vì Transform nhân vật
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0; // Loại bỏ độ nghiêng

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxisRaw("Horizontal") : 0;

        Vector3 move = forward * curSpeedX + right * curSpeedY;

        // Kiểm tra nếu nhân vật đang chạm đất
        if (controller.isGrounded)
        {
            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpForce; // Nhảy lên
                AnimatorSpeed = 0f; // Khi nhảy, animation là Idle
            }
            else
            {
                moveDirection.y = -0.5f; // Giữ nhân vật dính sàn
                AnimatorSpeed = controller.velocity.magnitude; // Di chuyển bình thường
            }
        }
        else
        {
            // Nếu đang rơi, áp dụng trọng lực tự nhiên hơn
            moveDirection.y -= gravity * (moveDirection.y > 0 ? 1 : fallMultiplier) * Runner.DeltaTime;
            AnimatorSpeed = 0f; // Khi đang rơi, animation là Idle
        }

        move.y = moveDirection.y;

        // Di chuyển nhân vật
        controller.Move(move * Runner.DeltaTime);
        moveDirection = move; // Cập nhật hướng di chuyển

        // Cập nhật animation
        animator.SetFloat("Speed", AnimatorSpeed);
    }
}

