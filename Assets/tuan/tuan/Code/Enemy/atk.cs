using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Cài đặt Tấn công & Truy đuổi")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Cài đặt Tấn công hình quạt (Attack Cone)")]
    [SerializeField] private float attackRange = 2.71f;   // Bán kính tầm đánh
    [Range(0f, 360f)]
    [SerializeField] private float attackAngle = 90f;     // Góc quét tấn công
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Cài đặt Khựng sau khi tấn công (Recovery)")]
    [SerializeField] private float attackRecoveryTime = 0.5f; // Thời gian đứng khựng lại sau khi ra đòn (giây)
    private float recoveryTimer = 0f;

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

        // --- KIỂM TRA THỜI GIAN KHỰNG SAU TẤN CÔNG (RECOVERY) ---
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
            agent.isStopped = true; // Dừng di chuyển trong lúc đang khựng ra đòn
            return; // Thoát hàm sớm, chưa cho phép di chuyển hay đánh tiếp
        }

        agent.stoppingDistance = attackRange;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // --- XOAY MẶT MƯỢT MÀ VỀ PHÍA PLAYER ---
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // KIỂM TRA ĐIỀU KIỆN TẤN CÔNG: 
        if (IsPlayerInFrontOfAttackCone(target))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttackAction();

                // Kích hoạt thời gian khựng sau khi vừa ra đòn xong
                recoveryTimer = attackRecoveryTime;
                agent.isStopped = true; // Dừng hẳn lại ngay khoảnh khắc tung đòn
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