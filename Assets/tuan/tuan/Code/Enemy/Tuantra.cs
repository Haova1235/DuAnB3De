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
    [SerializeField] private float randomSearchRadius = 10f; // Bán kính vòng tròn quét điểm ngẫu nhiên

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

        // Nếu chưa có điểm đến hoặc đã tới điểm ngẫu nhiên (hoặc đường đi bị lỗi/chặn)
        if (!hasDestination || (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance || agent.pathStatus == NavMeshPathStatus.PathInvalid)))
        {
            SetNewRandomDestination();
        }

        // Gọi script đặt bẫy hoạt động khi tuần tra
        if (trapSpawner != null)
        {
            trapSpawner.TickTrapTimer();
        }
    }

    // Hàm quét chọn vị trí ngẫu nhiên trong bán kính randomSearchRadius độc lập
    public void SetNewRandomDestination()
    {
        // Tạo một điểm ngẫu nhiên nằm trong hình cầu/vòng tròn bán kính randomSearchRadius quanh vị trí Enemy
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * randomSearchRadius;

        NavMeshHit hit;
        // Kiểm tra xem điểm ngẫu nhiên đó có nằm trên mặt phẳng NavMesh đã Bake hay không
        if (NavMesh.SamplePosition(randomPoint, out hit, randomSearchRadius, NavMesh.AllAreas))
        {
            currentRandomDestination = hit.position;
            agent.SetDestination(currentRandomDestination);
            hasDestination = true;
        }
        else
        {
            hasDestination = false; // Nếu chọn trúng góc không Bake được, lượt sau sẽ thử chọn lại
        }
    }

    // Vẽ tầm quét vị trí ngẫu nhiên bằng màu xanh lam (Cyan) trên cửa sổ Scene View để dễ canh chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, randomSearchRadius);

        // Vẽ thêm vị trí mục tiêu ngẫu nhiên hiện tại (nếu có)
        if (hasDestination)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentRandomDestination, 0.3f);
            Gizmos.DrawLine(transform.position, currentRandomDestination);
        }
    }
}