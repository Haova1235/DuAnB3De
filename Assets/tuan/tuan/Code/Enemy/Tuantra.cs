using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTrapSpawner))]
public class EnemyPatrol : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyTrapSpawner trapSpawner;

    [Header("Cài đặt Tuần tra ngẫu nhiên")]
    [SerializeField] private float patrolSpeed = 3.5f;

    [Tooltip("Tầm quét riêng biệt để chọn vị trí ngẫu nhiên cho Enemy")]
    [SerializeField] private float randomSearchRadius = 10f;

    [Tooltip("Số lần thử tối đa để tìm điểm đi tới được")]
    [SerializeField] private int maxPickAttempts = 10;

    private Vector3 currentRandomDestination;
    private bool hasDestination = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        trapSpawner = GetComponent<EnemyTrapSpawner>();
        SetNewRandomDestination();
    }

    public void DoPatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        // Chưa có điểm đến, đã tới nơi, hoặc đường đi bị lỗi/chặn (ví dụ cửa vừa đóng)
        if (!hasDestination || (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance || agent.pathStatus != NavMeshPathStatus.PathComplete)))
        {
            SetNewRandomDestination();
        }

        if (trapSpawner != null)
        {
            trapSpawner.TickTrapTimer();
        }
    }

    // Chọn điểm ngẫu nhiên mà Enemy THẬT SỰ đi tới được (bỏ qua phòng sau cửa khóa)
    public void SetNewRandomDestination()
    {
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < maxPickAttempts; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * randomSearchRadius;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, randomSearchRadius, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    currentRandomDestination = hit.position;
                    agent.SetPath(path);
                    hasDestination = true;
                    return;
                }
            }
        }

        hasDestination = false; // lượt sau thử lại
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, randomSearchRadius);

        if (hasDestination)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentRandomDestination, 0.3f);
            Gizmos.DrawLine(transform.position, currentRandomDestination);
        }
    }
}