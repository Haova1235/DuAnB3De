using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Cài đặt Tấn công & Truy đuổi")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime;

    [HideInInspector] public bool isSearching = false; // Đang trong trạng thái đứng quét tìm kiếm

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Chase(Transform target)
    {
        // Nếu đang quét tìm kiếm mà thấy player thì hủy quét, lập tức đuổi theo
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    public void Attack(Transform target)
    {
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
        }

        agent.stoppingDistance = attackRange;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Xoay mặt mượt mà về phía Player
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Enemy tấn công Player!");
        }
    }

    // Hàm được gọi khi vừa mất dấu Player: Chạy đến điểm cuối, sau đó đứng quét 5 giây
    public void HandleLostPlayer(Vector3 lastKnownPosition, System.Action onFinishSearchCallback)
    {
        if (isSearching) return; // Tránh gọi chồng chéo

        StartCoroutine(SearchRoutine(lastKnownPosition, onFinishSearchCallback));
    }

    IEnumerator SearchRoutine(Vector3 targetPos, System.Action onFinishSearch)
    {
        isSearching = true;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(targetPos);

        // 1. Chờ cho đến khi Enemy di chuyển đến được vị trí cuối cùng của Player
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.5f)
        {
            // Nếu trong lúc chạy đến điểm cuối mà phát hiện thấy player (bên controller sẽ ngắt coroutine này bằng cách gọi Chase), hàm này sẽ bị dừng
            yield return null;
        }

        // 2. Đã đến nơi, dừng di chuyển và bắt đầu quay trái phải trong 5 giây
        agent.isStopped = true;
        float timer = 0f;
        float searchDuration = 5f;

        // Góc quay qua lại (ví dụ xoay qua trái 45 độ, qua phải 45 độ)
        Quaternion startRot = transform.rotation;

        while (timer < searchDuration)
        {
            // Dùng hàm Sin để tạo hiệu ứng lắc lư đầu/xoay người trái phải mượt mà
            float angleOffset = Mathf.Sin(timer * 4f) * 45f;
            transform.rotation = startRot * Quaternion.Euler(0, angleOffset, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        // 3. Hết 5 giây không thấy gì -> Kết thúc tìm kiếm, quay lại tuần tra ngẫu nhiên
        isSearching = false;
        onFinishSearch?.Invoke();
    }
}