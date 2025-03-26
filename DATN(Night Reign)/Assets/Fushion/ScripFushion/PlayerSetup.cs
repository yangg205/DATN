using Fusion;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    public Transform Head; // Transform của đầu nhân vật
    public Transform Gun;  // Transform của súng
    public Transform PlayerBody; // Transform của toàn bộ nhân vật

    public void SetUpCam()
    {
        if (Object.HasStateAuthority)
        {
            // Tìm đối tượng CamFPS
            var camFPS = FindFirstObjectByType<CamFPS>();
            if (camFPS != null)
            {
                // Thiết lập Camera, Head, Gun và PlayerBody
                camFPS.Setup(PlayerBody, Head, Gun);
            }
        }
    }
}
