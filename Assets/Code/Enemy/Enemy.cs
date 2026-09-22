using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Cài đặt tầm nhìn & tấn công")]
    [SerializeField] private float visionRange = 10f;    // Tầm nhìn phát hiện player
    [SerializeField] private float attackRange = 2f;     // Tầm đánh cận chiến
    [SerializeField] private float attackCooldown = 1.5f;// Thời gian hồi chiêu đánh
    private float lastAttackTime;

    [Header("Cài đặt Tuần tra cố định")]
    [SerializeField] private List<Transform> waypoints;  // Danh sách các điểm tuần tra
    [SerializeField] private float patrolSpeed = 3.5f;   // Tốc độ đi tuần
    [SerializeField] private float chaseSpeed = 6f;      // Tốc độ rượt đuổi
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tự động tìm Player dựa vào Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Bắt đầu di chuyển đến điểm tuần tra đầu tiên
        if (waypoints != null && waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Ngoài tầm nhìn -> Tiếp tục lộ trình tuần tra
        if (distanceToPlayer > visionRange)
        {
            PatrolRoutine();
        }
        // 2. Trong tầm nhìn nhưng ngoài tầm đánh -> Rượt đuổi theo Player
        else if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        // 3. Trong tầm đánh -> Dừng lại tấn công
        else
        {
            AttackPlayer();
        }
    }

    void PatrolRoutine()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        // Nếu quái đã đi đến gần điểm waypoint hiện tại, chuyển sang điểm tiếp theo
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        agent.isStopped = true;

        // Xoay mặt về phía Player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        // Thực hiện đòn đánh theo thời gian hồi chiêu
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Quái đang tấn công Player!");
        }
    }

    // Vẽ bán kính tầm nhìn để dễ căn chỉnh trên Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}