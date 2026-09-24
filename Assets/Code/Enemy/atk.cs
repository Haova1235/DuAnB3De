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
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    public void Attack(Transform target)
    {
        agent.isStopped = true;

        // Xoay mặt mượt mà về phía Player
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
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
        Debug.Log("Enemy tấn công Player !");
    }
}