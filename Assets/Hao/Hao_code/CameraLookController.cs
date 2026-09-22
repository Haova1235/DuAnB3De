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
        // Chỉ xoay khi giữ chuột phải
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * mouseSensitivity * Time.deltaTime * 10f;
            pitch -= delta.y * mouseSensitivity * Time.deltaTime * 10f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            // Khóa và ẩn con trỏ chuột khi đang kéo
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Thả chuột phải ra thì hiện con trỏ lại
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}