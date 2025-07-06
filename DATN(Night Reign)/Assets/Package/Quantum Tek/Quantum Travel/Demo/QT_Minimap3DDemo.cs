using UnityEngine;

namespace QuantumTek.QuantumTravel.Demo
{
    public class QT_Minimap3DDemo : MonoBehaviour
    {
        public float rotSpeed = 1; // Tốc độ xoay khi di chuyển chuột

        private void Update()
        {
            // Xoay nhân vật dựa trên input chuột
            float mouseX = Input.GetAxis("Mouse X"); // Lấy sự thay đổi theo chiều ngang của chuột
            transform.Rotate(0, mouseX * rotSpeed, 0); // Xoay theo trục Y

            
        }
    }
}