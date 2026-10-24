using UnityEngine;
using UnityEngine.InputSystem;

public class MouseControl : MonoBehaviour
{
    // Script camera nhìn theo chuột
    public MonoBehaviour cameraLookScript;

    private bool mouseUnlocked = false;

    void Start()
    {
        // Khi bắt đầu game:
        // Khóa chuột
        LockMouse();
    }

    void Update()
    {
        // Nhấn V để đổi trạng thái
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (mouseUnlocked)
            {
                LockMouse();
            }
            else
            {
                UnlockMouse();
            }
        }
    }

    void LockMouse()
    {
        mouseUnlocked = false;

        // Khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;

        // Ẩn chuột
        Cursor.visible = false;

        // Cho phép camera nhìn theo chuột
        if (cameraLookScript != null)
        {
            cameraLookScript.enabled = true;
        }
    }

    void UnlockMouse()
    {
        mouseUnlocked = true;

        // Cho chuột di chuyển tự do
        Cursor.lockState = CursorLockMode.None;

        // Hiện chuột
        Cursor.visible = true;

        // Tắt camera nhìn theo chuột
        if (cameraLookScript != null)
        {
            cameraLookScript.enabled = false;
        }
    }
}