using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("Cài đặt Tầm nhìn hình quạt (FOV)")]
    [SerializeField] private float visionRadius = 10f;             // Bán kính nhìn xa
    [Range(0, 360)]
    [SerializeField] private float visionAngle = 60f;              // Góc mở hình quạt (ví dụ: 60 độ)

    private Transform player;
    private bool isPlayerInSight = false; // Biến trạng thái cờ hiệu

    void Start()
    {
        // Tự động tìm Player dựa vào Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Hàm kiểm tra xem quái có thấy player không
    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 1. Kiểm tra xem player có nằm trong bán kính hình tròn không
        if (distanceToPlayer <= visionRadius)
        {
            // Lấy hướng nhìn phía trước của quái (bỏ qua trục Y để tính góc trên mặt phẳng ngang)
            Vector3 forwardDir = transform.forward;
            forwardDir.y = 0;

            Vector3 dirToPlayerFlat = directionToPlayer;
            dirToPlayerFlat.y = 0;
            dirToPlayerFlat.Normalize();

            // 2. Tính góc lệch giữa hướng mặt quái và hướng tới player
            float angleBetween = Vector3.Angle(forwardDir, dirToPlayerFlat);

            // 3. Nếu góc lệch nằm trong phạm vi nửa góc quạt (visionAngle / 2)
            if (angleBetween <= visionAngle / 2f)
            {
                isPlayerInSight = true; // Cập nhật trạng thái là đã thấy
                return true;            // Trả về kết quả là Thấy
            }
        }

        // Nếu không thỏa mãn các điều kiện trên thì cập nhật là Không thấy
        isPlayerInSight = false;
        return false;
    }

    // Hàm để lấy giá trị biến isPlayerInSight nếu các script khác cần đọc trực tiếp
    public bool GetIsPlayerInSight()
    {
        return isPlayerInSight;
    }

    // Lấy thông tin vị trí Player
    public Transform GetPlayerTransform()
    {
        return player;
    }

    // Vẽ mô phỏng vùng nhìn hình quạt trên cửa sổ Scene View để dễ canh chỉnh
    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn bán kính tầm nhìn
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Tính toán và vẽ 2 đường biên của hình quạt
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftBoundary * visionRadius);
        Gizmos.DrawRay(transform.position, rightBoundary * visionRadius);
    }
}