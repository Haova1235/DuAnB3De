using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Cài đặt tầm nhìn & tấn công")]
    [SerializeField] private float visionRange = 10f;    // Khoảng cách phát hiện player
    [SerializeField] private float attackRange = 2f;     // Khoảng cách đứng lại để đánh
    [SerializeField] private float attackCooldown = 1.5f;// Thời gian chờ giữa các đòn đánh
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tự động tìm đối tượng có Tag là "Player"
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Ngoài tầm nhìn -> Đứng yên
        if (distanceToPlayer > visionRange)
        {
            agent.isStopped = true;
        }
        // 2. Trong tầm nhìn nhưng ngoài tầm đánh -> Đuổi theo
        else if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        // 3. Vào tầm đánh -> Dừng lại và tấn công
        else
        {
            agent.isStopped = true;

            // Xoay mặt về phía Player
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }

            // Tấn công theo thời gian hồi chiêu
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                Debug.Log("Quái đang tấn công Capsule Player!");
            }
        }
    }

    // Vẽ vòng tròn tầm nhìn trên cửa sổ Scene để dễ quan sát
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}