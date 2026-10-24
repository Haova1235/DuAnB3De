using UnityEngine;

public class KeypadBridge : MonoBehaviour
{
    public static void NotifyAccessGranted()
    {
        // 1. Khóa Keypad và ẩn chữ [E] ở Scene 1
        KeypadTrigger keypadTrigger = Object.FindFirstObjectByType<KeypadTrigger>();
        if (keypadTrigger != null)
        {
            keypadTrigger.SetUnlocked();
        }

        // 2. Tự động tìm cửa (DoorV1 hoặc Door_1_Black) ở Scene 1 và gọi mở cửa
        DoorController door = Object.FindFirstObjectByType<DoorController>();
        if (door != null)
        {
            door.OpenDoor();
        }
    }
}