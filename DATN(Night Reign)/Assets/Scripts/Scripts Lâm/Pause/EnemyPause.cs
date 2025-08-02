using UnityEngine;

public class EnemyPause : MonoBehaviour
{
    public static bool IsPaused = false;

    void Update()
    {
        if (IsPaused) return;

        // Enemy AI logic ở đây
    }
}
