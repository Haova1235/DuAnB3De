using UnityEngine;

public class PlaceableCamera : Item
{
    [Header("Camera Setup")]
    [Tooltip("Prefab của SecurityCamera sẽ được sinh ra khi đặt xuống")]
    public GameObject securityCameraPrefab;

    // Hàm này sẽ được gọi khi người chơi bấm nút đặt camera vào điểm cố định
    public void PlaceDown(Transform targetMountPoint)
    {
        if (securityCameraPrefab != null)
        {
            // Sinh ra camera an ninh tại vị trí điểm đặt định sẵn
            Instantiate(securityCameraPrefab, targetMountPoint.position, targetMountPoint.rotation);
        }

        // Xóa item camera trên tay người chơi
        Destroy(gameObject);
    }
}