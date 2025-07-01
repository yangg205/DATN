using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D hoặc mũi tên trái/phải
        float moveZ = Input.GetAxis("Vertical");   // W/S hoặc mũi tên lên/xuống

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
