using UnityEngine;

public class PlayerPause : MonoBehaviour
{
    public static bool IsPaused = false;

    void Update()
    {
        if (IsPaused) return;

        // Player điều khiển logic ở đây
    }
}
