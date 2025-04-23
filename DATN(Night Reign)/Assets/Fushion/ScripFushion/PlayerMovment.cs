using Fusion;
using UnityEngine;

public class DuyPlayerMovement : NetworkBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float jumpHeight = 3f; // Độ cao khi nhảy
    public float gravity = -9.81f; // Lực hấp dẫn
    private Vector3 velocity; // Vector để xử lý lực rơi

    public Transform groundCheck; // Vị trí kiểm tra chạm đất
    public float groundCheckRadius = 0.4f; // Bán kính kiểm tra chạm đất
    public LayerMask groundMask; // Lớp mặt đất

    private bool isGrounded; // Kiểm tra trạng thái nhân vật chạm đất
    public bool canMove = true;

    private Transform cameraTransform;
    public Animator animator;

    [Networked, OnChangedRender(nameof(OnRenderAnimationStateChanged))]
    public NetworkBool isMoving { get; set; }

    public void OnRenderAnimationStateChanged()
    {
        UpdateAnimatorState();
    }

    void Start()
    {
        // Gắn camera
        var camFPS = FindFirstObjectByType<CamFPS>();
        if (camFPS != null)
        {
            cameraTransform = camFPS.FPSCam.transform;
        }

        // Gắn Animator
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on the player.");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (controller == null || !Object.HasStateAuthority || !canMove) return;

        // Kiểm tra nhân vật có chạm đất không
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Đặt vận tốc trục y để giữ nhân vật ở mặt đất
        }

        // Nhận input di chuyển
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

        forward.y = 0;
        right.y = 0;

        Vector3 move = right * x + forward * z;
        controller.Move(move.normalized * speed * Runner.DeltaTime);

        // Nhảy
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            TriggerAnimation("Jump"); // Kích hoạt animation Jump
        }

        // Tính toán trọng lực
        velocity.y += gravity * Runner.DeltaTime;
        controller.Move(velocity * Runner.DeltaTime);

        // Cập nhật trạng thái di chuyển
        bool isMovingNow = move.sqrMagnitude > 0.01f;
        if (isMoving != isMovingNow && Object.HasStateAuthority)
        {
            isMoving = isMovingNow;
        }
    }

    private void UpdateAnimatorState()
    {
        if (animator == null) return;

        // Cập nhật trạng thái chạy hoặc đứng yên
        if (isMoving)
        {
            animator.SetTrigger("Run");
        }
        else
        {
            animator.SetTrigger("Idie");
        }
    }

    public void TakeDamage()
    {
        TriggerAnimation("Hit"); // Kích hoạt animation Hit
        Debug.Log("Player took damage!");
    }

    // Kích hoạt trigger trong Animator
    private void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}
