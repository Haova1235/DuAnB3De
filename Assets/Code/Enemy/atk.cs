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
        // BẮT BUỘC: Mở khóa agent ngay lập tức khi chuyển sang rượt đuổi
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    public void Attack(Transform target)
    {
        // Cài đặt khoảng cách dừng của NavMeshAgent bằng đúng tầm đánh (attackRange) hoặc một khoảng an toàn
        agent.stoppingDistance = 1.5f; // Bạn có thể chỉnh con số này khớp với bán kính tầm đánh
        agent.isStopped = false; // Vẫn bật agent để nó tự động căn chỉnh khoảng cách
        agent.SetDestination(target.position); // Tiến hoặc lùi nhẹ để giữ đúng khoảng cách

        // Xoay mặt mượt mà về phía Player một cách an toàn
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Hồi chiêu đòn đánh
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