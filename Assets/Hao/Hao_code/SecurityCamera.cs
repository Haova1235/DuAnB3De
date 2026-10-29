using UnityEditor.Rendering;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Security Camera Components")]
    public Camera securityCam;
    public CameraUI cameraHUD; // Kéo component quản lý HUD UI vào đây

    private void Awake()
    {
        if (securityCam == null)
            securityCam = GetComponentInChildren<Camera>();

        if (securityCam != null)
        {
            securityCam.gameObject.SetActive(false);
        }

        // Tắt HUD khi mới khởi tạo
        if (cameraHUD != null)
        {
            cameraHUD.SetHUDActive(false);
        }
    }

    private void Start()
    {
        // Tự động đăng ký vào hệ thống quản lý chung khi được tạo ra
        CameraMonitorSystem monitorSystem = FindAnyObjectByType<CameraMonitorSystem>();
        if (monitorSystem != null)
        {
            monitorSystem.RegisterCamera(this);
        }
    }

    private void OnDestroy()
    {
        // Tự động hủy đăng ký khi camera bị phá hủy
        CameraMonitorSystem monitorSystem = FindAnyObjectByType<CameraMonitorSystem>();
        if (monitorSystem != null)
        {
            monitorSystem.UnregisterCamera(this);
        }
    }

    // Hàm duy nhất để quản lý bật/tắt góc nhìn và giao diện của camera
    public void EnableCameraView(bool isEnabled)
    {
        Debug.Log("Camera " + gameObject.name + " trạng thái: " + isEnabled);
        // Bật hoặc tắt đối tượng Camera
        if (securityCam != null)
        {
            securityCam.gameObject.SetActive(isEnabled);
        }

        // Bật hoặc tắt giao diện HUD tương ứng (đồng hồ + khung `[` `]`)
        if (cameraHUD != null)
        {
            cameraHUD.SetHUDActive(isEnabled);
        }
    }
}