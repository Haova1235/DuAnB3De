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

        bool canSeePlayer = viewScript.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Trường hợp 1: Không thấy Player -> Tiếp tục đi tuần
        if (!canSeePlayer)
        {
            // Bắt buộc gọi StopCombat để mở khóa agent.isStopped trước khi cho tuần tra
            combatScript.StopCombat();
            patrolScript.DoPatrol();
        }
        // Trường hợp 2: Thấy nhưng ngoài tầm đánh -> Rượt đuổi
        else if (distanceToPlayer > attackRange)
        {
            combatScript.Chase(player);
        }
        // Trường hợp 3: Trong tầm đánh -> Dừng lại tấn công
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