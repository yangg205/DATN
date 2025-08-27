using AG;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;

    public bool isInputLocked { get; private set; } = false; // 🔑 cờ kiểm tra input có bị khóa hay không

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LockCursor();
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
        isInputLocked = true; // 🔥 khóa input
    }

    public void HideCursorAndEnableInput()
    {
        LockCursor();
        isInputLocked = false; // 🔥 mở input
    }
}
