using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyView))]
public class EnemyController : MonoBehaviour
{
    private Transform player;
    private EnemyPatrol patrolScript;
    private EnemyCombat combatScript;
    private EnemyView viewScript;

    private Vector3 lastKnownPlayerPosition;
    private bool hasRecordedLastPosition = false;

    void Start()
    {
        // Lấy các component đã được gắn trên Enemy
        patrolScript = GetComponent<EnemyPatrol>();
        combatScript = GetComponent<EnemyCombat>();
        viewScript = GetComponent<EnemyView>();

        // Tự động tìm Player dựa vào Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng nào có Tag là 'Player' trong Scene!");
        }
    }

    void Update()
    {
        if (player == null) return;

        bool canSeePlayer = viewScript.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Lấy giá trị attackRange trực tiếp từ component EnemyCombat để đồng bộ
        float currentAttackRange = combatScript.GetAttackRange();

        if (canSeePlayer)
        {
            // Khi nhìn thấy Player, liên tục lưu lại vị trí hiện tại của họ
            lastKnownPlayerPosition = player.position;
            hasRecordedLastPosition = true;

            if (distanceToPlayer > currentAttackRange)
            {
                combatScript.Chase(player);
            }
            else
            {
                combatScript.Attack(player);
            }
        }
        else
        {
            // Nếu đang trong quá trình chạy đến điểm cuối hoặc đứng quét 5 giây, giữ nguyên trạng thái
            if (combatScript.isSearching) return;

            // Nếu vừa mất dấu và có ghi nhận vị trí cuối cùng
            if (hasRecordedLastPosition)
            {
                hasRecordedLastPosition = false; // Đảm bảo chỉ kích hoạt sự kiện này 1 lần duy nhất khi vừa mất dấu

                // Gọi hành động: Chạy đến điểm cuối -> Quét 5 giây -> Hết 5 giây không thấy sẽ gọi callback quay lại tuần tra ngẫu nhiên
                combatScript.HandleLostPlayer(lastKnownPlayerPosition, () => {
                    patrolScript.DoPatrol();
                });
            }
            else
            {
                // Mặc định tiếp tục tìm đường tuần tra ngẫu nhiên trên NavMesh
                patrolScript.DoPatrol();
            }
        }
    }

    // Vẽ trực quan tầm đánh hình chữ V (hình quạt) trên Scene View dựa theo thông số của EnemyCombat
    private void OnDrawGizmosSelected()
    {
        EnemyCombat combat = GetComponent<EnemyCombat>();
        float range = combat != null ? combat.GetAttackRange() : 2.71f;
        float angle = combat != null ? combat.GetAttackAngle() : 90f;

        // 1. Vẽ vòng tròn bán kính tầm đánh
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // 2. Vẽ hai đường biên tạo thành hình chữ V trước mặt
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -angle / 2f, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, angle / 2f, 0);

        Vector3 leftRayDir = leftRayRotation * forward;
        Vector3 rightRayDir = rightRayRotation * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftRayDir * range);
        Gizmos.DrawRay(transform.position, rightRayDir * range);
    }
}