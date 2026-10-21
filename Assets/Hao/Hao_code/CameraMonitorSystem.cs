using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMonitorSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerMainCamera;
    [SerializeField] private MonoBehaviour playerMoveScript;

    private List<SecurityCamera> availableCameras = new List<SecurityCamera>();
    private int currentIndex = 0;
    private bool isViewingCamera = false;

    private void Start()
    {
        if (playerMainCamera == null && Camera.main != null)
        {
            playerMainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Phím T để bật/tắt chế độ xem camera
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            ToggleCameraView();
        }

        // Khi đang xem camera, dùng Q và E để chuyển đổi qua lại giữa các góc máy
        if (isViewingCamera && availableCameras.Count > 1)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                SwitchCamera(-1); // Lùi về camera trước
            }
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SwitchCamera(1);  // Tiến tới camera sau
            }
        }
    }

    public void RegisterCamera(SecurityCamera cam)
    {
        if (!availableCameras.Contains(cam))
        {
            availableCameras.Add(cam);
        }
    }

    public void UnregisterCamera(SecurityCamera cam)
    {
        if (availableCameras.Contains(cam))
        {
            availableCameras.Remove(cam);
            if (currentIndex >= availableCameras.Count)
            {
                currentIndex = Mathf.Max(0, availableCameras.Count - 1);
            }
        }
    }

    private void ToggleCameraView()
    {
        if (availableCameras.Count == 0)
        {
            Debug.Log("Chưa có chiếc camera nào được đặt trong màn chơi!");
            return;
        }

        isViewingCamera = !isViewingCamera;

        if (isViewingCamera)
        {
            // --- BẬT XEM CAMERA ---
            if (playerMainCamera != null) playerMainCamera.gameObject.SetActive(false);

            currentIndex = 0;
            ActivateCurrentCamera();

            if (playerMoveScript != null) playerMoveScript.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Đang xem Camera an ninh. Dùng phím Q và E để đổi góc máy.");
        }
        else
        {
            // --- THOÁT XEM CAMERA ---
            DeactivateAllCameras();
            if (playerMainCamera != null) playerMainCamera.gameObject.SetActive(true);

            if (playerMoveScript != null) playerMoveScript.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Đã thoát chế độ xem Camera.");
        }
    }

    private void SwitchCamera(int direction)
    {
        if (availableCameras.Count == 0) return;

        // Tắt camera hiện tại
        availableCameras[currentIndex].EnableCameraView(false);

        // Tính toán index tiếp theo vòng tròn
        currentIndex += direction;
        if (currentIndex < 0) currentIndex = availableCameras.Count - 1;
        if (currentIndex >= availableCameras.Count) currentIndex = 0;

        // Bật camera mới
        ActivateCurrentCamera();
        Debug.Log("Đang chuyển sang Camera số: " + (currentIndex + 1));
    }

    private void ActivateCurrentCamera()
    {
        for (int i = 0; i < availableCameras.Count; i++)
        {
            if (availableCameras[i] != null)
            {
                availableCameras[i].EnableCameraView(i == currentIndex);
            }
        }
    }

    private void DeactivateAllCameras()
    {
        foreach (var cam in availableCameras)
        {
            if (cam != null) cam.EnableCameraView(false);
        }
    }
}