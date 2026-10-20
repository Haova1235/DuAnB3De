using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Cài đặt Tấn công & Truy đuổi")]
    [SerializeField] private float chaseSpeed = 6f;

    [Header("Cài đặt Tấn công hình quạt (Attack Cone)")]
    [SerializeField] private float attackRange = 2.71f;   // Bán kính tầm đánh

    [Range(0f, 360f)]
    [SerializeField] private float attackAngle = 90f;     // Góc quét tấn công

    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime;

    [HideInInspector] public bool isSearching = false;
    public float GetAttackRange() { return attackRange; }
    public float GetAttackAngle() { return attackAngle; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Chase(Transform target)
    {
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

        // KIỂM TRA ĐIỀU KIỆN TẤN CÔNG: 
        if (IsPlayerInFrontOfAttackCone(target))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttackAction();
            }
        }
    }

    bool IsPlayerInFrontOfAttackCone(Transform target)
    {
        Vector3 directionToPlayer = target.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > attackRange + 0.5f) return false;

        directionToPlayer.y = 0;
        Vector3 forwardDir = transform.forward;
        forwardDir.y = 0;

        float angle = Vector3.Angle(forwardDir, directionToPlayer);

        if (angle <= attackAngle / 2f)
        {
            return true;
        }

        return false;
    }

    void PerformAttackAction()
    {
        Debug.Log("Enemy tung đòn tấn công chính diện vào Player!");
    }

    public void HandleLostPlayer(Vector3 lastKnownPosition, System.Action onFinishSearchCallback)
    {
        if (isSearching) return;
        StartCoroutine(SearchRoutine(lastKnownPosition, onFinishSearchCallback));
    }

    IEnumerator SearchRoutine(Vector3 targetPos, System.Action onFinishSearch)
    {
        isSearching = true;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(targetPos);

        while (agent.pathPending || agent.remainingDistance > attackRange + 0.5f)
        {
            yield return null;
        }

        agent.isStopped = true;
        float timer = 0f;
        float searchDuration = 5f;

        Quaternion startRot = transform.rotation;

        while (timer < searchDuration)
        {
            float angleOffset = Mathf.Sin(timer * 4f) * 45f;
            transform.rotation = startRot * Quaternion.Euler(0, angleOffset, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        isSearching = false;
        onFinishSearch?.Invoke();
    }

    // --- VẼ GIZMOS TRỰC TIẾP TRÊN SCENE VIEW ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -attackAngle / 2f, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, attackAngle / 2f, 0);

        Vector3 leftRayDir = leftRayRotation * forward;
        Vector3 rightRayDir = rightRayRotation * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftRayDir * attackRange);
        Gizmos.DrawRay(transform.position, rightRayDir * attackRange);
    }
}