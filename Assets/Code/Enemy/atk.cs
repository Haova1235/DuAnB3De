using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Cài đặt Tấn công")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Chase(Transform target)
    {
        // Luôn mở khóa di chuyển khi rượt đuổi
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    public void Attack(Transform target)
    {
        // Dừng lại hoàn toàn khi đang đứng trong tầm đánh để tấn công
        agent.isStopped = true;

        // Xoay mặt mượt mà về phía Player một cách an toàn (tránh lỗi xoay tròn)
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Hồi chiêu đòn đánh liên tục theo nhịp
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttackAction();
        }
    }

    void PerformAttackAction()
    {
        Debug.Log("Enemy đang tấn công Player!");
        // Thêm logic gây sát thương trực tiếp lên Player ở đây nếu cần (ví dụ: target.GetComponent<PlayerHealth>()?.TakeDamage(...));
    }

    // Hàm chủ động mở khóa Agent khi chuyển trạng thái khỏi Combat (về Patrol)
    public void StopCombat()
    {
        agent.isStopped = false;
    }
}