using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator enemyAnimator; // Thêm biến Animator

    [Header("Cài đặt Tấn công & Truy đuổi")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float rotationSpeed = 25f;

    [Header("Cài đặt Tấn công hình quạt (Attack Cone)")]
    [SerializeField] private float attackRange = 2.71f;
    [Range(0f, 360f)]
    [SerializeField] private float attackAngle = 90f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Cài đặt Khựng sau khi tấn công (Recovery)")]
    [SerializeField] private float attackRecoveryTime = 0.5f;
    private float recoveryTimer = 0f;

    private float lastAttackTime;

    [HideInInspector] public bool isSearching = false;
    public float GetAttackRange() { return attackRange; }
    public float GetAttackAngle() { return attackAngle; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = 360f; // Tăng tốc độ quay góc của NavMeshAgent cho mượt

        // Tự động tìm Animator trên bản thân hoặc các object con bên trong
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Cập nhật thông số Speed cho Animator dựa trên tốc độ thực tế của NavMeshAgent
        if (enemyAnimator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            enemyAnimator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
        }

        // Giảm thời gian recovery theo thời gian thực
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
        }
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

        // Xoay mặt mượt mà về hướng Player khi đang Chase
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void Attack(Transform target)
    {
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
        }

        if (recoveryTimer > 0f)
        {
            agent.isStopped = true;
            return;
        }

        agent.stoppingDistance = attackRange;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (IsPlayerInFrontOfAttackCone(target))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttackAction();

                recoveryTimer = attackRecoveryTime;
                agent.isStopped = true;
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
        return angle <= attackAngle / 2f;
    }

    void PerformAttackAction()
    {
        Debug.Log("Enemy tung đòn tấn công chính diện vào Player!");

        // Kích hoạt Trigger Attack để chạy animation tấn công
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Attack");
        }
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