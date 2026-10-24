using UnityEngine;

public class UnlockCursor : MonoBehaviour
{
    private void Update()
    {
        // Luôn đảm bảo chuột hiện và không bị khóa khi đang ở giao diện Keypad
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}