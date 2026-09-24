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

        // 1. Kiểm tra xem Player có đang nằm trong tầm nhìn hình quạt không (dựa vào script EnemyView)
        bool canSeePlayer = viewScript.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- HỆ THỐNG ĐIỀU PHỐI TRẠNG THÁI ---

        // Trường hợp 1: Không thấy Player trong hình quạt (hoặc đứng sau lưng) -> Tiếp tục đi tuần
        if (!canSeePlayer)
        {
            patrolScript.DoPatrol();
        }
        // Trường hợp 2: Thấy Player trong hình quạt nhưng khoảng cách vẫn ngoài tầm đánh -> Rượt đuổi
        else if (distanceToPlayer > attackRange)
        {
            combatScript.Chase(player);
        }
        // Trường hợp 3: Đã vào trong tầm đánh -> Dừng lại tấn công
        else
        {
            combatScript.Attack(player);
        }
    }

    // Vẽ bán kính tầm đánh trên Scene View để dễ canh chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}