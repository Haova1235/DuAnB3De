using UnityEngine;
using TMPro;
using System;

public class CameraUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hudPanel;       // Kéo Canvas hoặc Panel chứa khung viền & đồng hồ vào đây
    public TextMeshProUGUI timeText;  // Kéo TextMeshPro hiển thị giờ vào đây

    void Update()
    {
        // Chỉ chạy đồng hồ khi bảng UI của camera đang được bật
        if (hudPanel != null && hudPanel.activeSelf)
        {
            if (timeText != null)
            {
                // Lấy giờ phút giây hệ thống theo định dạng chuẩn an ninh
                timeText.text = DateTime.Now.ToString("HH:mm:ss");
            }
        }
    }

    // Hàm bật/tắt toàn bộ giao diện HUD của camera này
    public void SetHUDActive(bool isActive)
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(isActive);
        }
    }
}