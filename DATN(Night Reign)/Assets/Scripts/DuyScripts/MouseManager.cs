using AG;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DontDestroyOnLoad(gameObject); // Nếu muốn giữ lại giữa các scene
    }

    private void Start()
    {
        LockCursor(); // <-- Gọi khi bắt đầu game
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowCursorAndDisableInput()
    {
        UnlockCursor();
    }

    public void HideCursorAndEnableInput()
    {
        LockCursor();
    }
}
