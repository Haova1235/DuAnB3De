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

    [Header("Cài đặt Delay trước khi đánh trúng (Wind-up)")]
    [SerializeField] private float attackWindupTime = 1f;
    private bool isWindingUp = false;

    [Header("Cài đặt Tấn công hình quạt (Attack Cone)")]
    [SerializeField] private float attackRange = 2.71f;
    [Range(0f, 360f)]
    [SerializeField] private float attackAngle = 90f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Cài đặt Khựng sau khi tấn công (Recovery)")]
    [SerializeField] private float attackRecoveryTime = 0.5f;
    private float recoveryTimer = 0f;

    private float lastAttackTime;
    private PlayerMove cachedPlayer;

    [HideInInspector] public bool isSearching = false;
    public float GetAttackRange() { return attackRange; }
    public float GetAttackAngle() { return attackAngle; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = 360f; // Tăng tốc độ quay góc của NavMeshAgent cho mượt

        // Tự động tìm Animator trên bản thân hoặc các object con bên trong
        enemyAnimator = GetComponentInChildren<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) cachedPlayer = p.GetComponentInParent<PlayerMove>();
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
        if (cachedPlayer != null && cachedPlayer.IsDefeated)
        {
            agent.isStopped = true;
            return;
        }
        if (isWindingUp) return;
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
            isWindingUp = false;
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
        if (cachedPlayer != null && cachedPlayer.IsDefeated)
        {
            agent.isStopped = true;
            return;
        }
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
            isWindingUp = false;
        }

        // Đang vung đòn thì đứng yên, chờ kết quả
        if (isWindingUp) return;

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

        if (IsPlayerInFrontOfAttackCone(target) && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackWindupRoutine(target));
        }
    }
    IEnumerator AttackWindupRoutine(Transform target)
    {
        isWindingUp = true;
        lastAttackTime = Time.time;

        // Dừng lại và bắt đầu vung đòn (animation chạy ngay)
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");

        // Chờ 1 giây, vẫn quay mặt theo Player
        float timer = 0f;
        while (timer < attackWindupTime)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed * 0.2f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Hết thời gian: nếu Player còn trong tầm thì mới trúng đòn
        if (IsPlayerInFrontOfAttackCone(target))
        {
            DealDamageToPlayer();
        }

        recoveryTimer = attackRecoveryTime;
        isWindingUp = false;
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

    void DealDamageToPlayer()
    {
        if (cachedPlayer == null || cachedPlayer.IsDefeated) return;
        Debug.Log("Enemy đánh trúng Player!");
        cachedPlayer.TriggerPlayerDefeat();
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
            // Đã sửa lỗi dấu nhân và số ở đây: từ 'timer4f' thành 'timer * 4f'
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