using UnityEngine;

public class MountController : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 200f;

    private CharacterController controller;
    private Animator anim;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>(); // ← Gắn Animator ở đây
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.forward * v * speed;
        controller.SimpleMove(move);

        transform.Rotate(Vector3.up, h * turnSpeed * Time.deltaTime);

        // 👉 Cập nhật tốc độ cho Animator
        float currentSpeed = controller.velocity.magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }
}
