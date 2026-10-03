using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLookController : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public float minPitch = -40f;
    public float maxPitch = 80f;

    private float yaw;
    private float pitch;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
        else
        {
            // Ngay khi buông ra: khóa cứng rotation, KHÔNG đọc delta nữa dù có giá trị thừa
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }
    }

    void LateUpdate()
    {
        // Đảm bảo transform không bị component khác (như Cinemachine) ghi đè sau Update
        if (!Mouse.current.rightButton.isPressed)
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}