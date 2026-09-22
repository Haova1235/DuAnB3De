using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target; // Đối tượng Player mà camera sẽ đi theo

    [Header("Cài đặt khoảng cách và góc nhìn")]
    [SerializeField] private Vector3 offset = new Vector3(0, 10f, -10f); // Khoảng cách so với Player
    [SerializeField] private float smoothSpeed = 5f; // Tốc độ mượt khi di chuyển theo

    void Start()
    {
        // Tự động tìm Player dựa vào Tag đã thiết lập trước đó
        GameObject playerObj = GameObject.FindGameObjectWithTag("Enemy");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogError("CameraController không tìm thấy đối tượng có Tag 'Player'!");
        }
    }

    // Dùng LateUpdate để camera chuyển động mượt mà sau khi Player đã di chuyển xong trong Update
    void LateUpdate()
    {
        if (target == null) return;

        // Vị trí mong muốn của camera dựa trên vị trí Player cộng thêm khoảng cách (offset)
        Vector3 desiredPosition = target.position + offset;

        // Dùng Lerp để camera dịch chuyển mượt mà thay vì giật cục giật cục theo nhân vật
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Cập nhật vị trí mới cho camera
        transform.position = smoothedPosition;
    }
}