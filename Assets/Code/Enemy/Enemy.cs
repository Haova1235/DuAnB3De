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

    [Header("Tầm nhìn & Tương tác")]
    [SerializeField] private float attackRange = 2f; // Khoảng cách dừng lại để đánh

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

        if (canSeePlayer)
        {
            // Khi nhìn thấy Player, liên tục lưu lại vị trí hiện tại của họ
            lastKnownPlayerPosition = player.position;
            hasRecordedLastPosition = true;

            if (distanceToPlayer > attackRange)
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

    // Vẽ bán kính tầm đánh trên Scene View để dễ canh chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}