using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Security Camera Components")]
    public Camera securityCam;

    private void Awake()
    {
        if (securityCam == null)
            securityCam = GetComponentInChildren<Camera>();

        if (securityCam != null)
        {
            securityCam.gameObject.SetActive(false);
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

    public void EnableCameraView(bool active)
    {
        if (securityCam != null)
        {
            securityCam.gameObject.SetActive(active);
        }
    }
}