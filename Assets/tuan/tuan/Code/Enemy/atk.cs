using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombat : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator enemyAnimator;

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

    [Header("Âm thanh")]
    [SerializeField] private SoundEffect attackWindupSound;
    [SerializeField] private SoundEffect hitSound;
    [Header("Âm thanh bước chân")]
    [SerializeField] private SoundEffect enemyStepSound;
    [SerializeField] private float patrolStepInterval = 0.7f;
    [SerializeField] private float chaseStepInterval = 0.35f;
    private float stepTimer = 0f;

    [Header("Âm thanh phát hiện & nhạc rượt đuổi")]
    [SerializeField] private SoundEffect spottedSound;
    [Tooltip("Nhạc phát khi quái bắt đầu rượt. Để trống thì không đổi nhạc")]
    [SerializeField] private AudioClip chaseMusic;
    [Tooltip("Nhạc quay lại khi hết rượt (kéo cùng clip với BackgroundMusic)")]
    [SerializeField] private AudioClip calmMusic;
    [Tooltip("Sau bao nhiêu giây không còn bị gọi Chase/Attack thì coi như hết rượt")]
    [SerializeField] private SoundEffect lostPlayerSound; // tiếng gầm/thở khi mất dấu Player
    [SerializeField] private float chaseLostDelay = 1.5f;
    private bool isChasingState = false;
    private float lastChaseTime = -99f;

    private float lastAttackTime;
    private float defaultStoppingDistance;
    private PlayerMove cachedPlayer;

    [HideInInspector] public bool isSearching = false;
    public float GetAttackRange() { return attackRange; }
    public float GetAttackAngle() { return attackAngle; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = 360f;
        defaultStoppingDistance = agent.stoppingDistance;

        enemyAnimator = GetComponentInChildren<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            cachedPlayer = p.GetComponentInParent<PlayerMove>();
            if (cachedPlayer == null) cachedPlayer = p.GetComponentInChildren<PlayerMove>();
        }
    }

    void Update()
    {
        if (enemyAnimator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            enemyAnimator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
        }

        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
        }
        HandleEnemyFootsteps();
        UpdateChaseState();
    }

    public void Chase(Transform target)
    {
        if (cachedPlayer != null && cachedPlayer.IsDefeated)
        {
            agent.isStopped = true;
            return;
        }
        MarkChasing();
        if (isWindingUp) return;
        if (isSearching)
        {
            StopAllCoroutines();
            isSearching = false;
            isWindingUp = false;
        }

        agent.stoppingDistance = defaultStoppingDistance;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);

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
        MarkChasing();
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

        // Dừng lại và bắt đầu vung đòn
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");
        attackWindupSound.PlayAt(transform.position);

        // Chờ, vẫn quay mặt theo Player
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
        hitSound.PlayAt(cachedPlayer.transform.position);
        cachedPlayer.TriggerPlayerDefeat();
    }

    public void HandleLostPlayer(Vector3 lastKnownPosition, System.Action onFinishSearchCallback)
    {
        if (isSearching) return;
        lostPlayerSound.PlayAt(transform.position);
        StartCoroutine(SearchRoutine(lastKnownPosition, onFinishSearchCallback));
    }

    IEnumerator SearchRoutine(Vector3 targetPos, System.Action onFinishSearch)
    {
        isSearching = true;
        agent.stoppingDistance = defaultStoppingDistance;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(targetPos);

        // Có giới hạn thời gian và thoát khi đường bị chặn (ví dụ cửa khóa)
        float waitTimer = 0f;
        float maxTravelTime = 15f;

        while (agent.pathPending || agent.remainingDistance > attackRange + 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= maxTravelTime) break;

            if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathPartial
                && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                break;

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
    // Gọi mỗi lần Chase()/Attack() chạy. Lần đầu tiên là lúc quái phát hiện Player
    private void MarkChasing()
    {
        lastChaseTime = Time.time;

        if (isChasingState)
        {
            // Đang lùng sục mà thấy lại Player thì kêu lại tiếng phát hiện
            if (isSearching) spottedSound.PlayAt(transform.position);
            return;
        }

        isChasingState = true;
        spottedSound.PlayAt(transform.position);
        if (chaseMusic != null) BackgroundMusic.Instance?.Play(chaseMusic);
    }

    private void UpdateChaseState()
    {
        if (!isChasingState) return;
        if (cachedPlayer != null && cachedPlayer.IsDefeated) return;

        // Đang vung đòn hoặc đang lùng sục thì vẫn tính là đang rượt
        if (isSearching || isWindingUp)
        {
            lastChaseTime = Time.time;
            return;
        }

        if (Time.time - lastChaseTime > chaseLostDelay)
        {
            isChasingState = false;
            if (calmMusic != null) BackgroundMusic.Instance?.Play(calmMusic);
        }
    }

    private void HandleEnemyFootsteps()
    {
        float interval = isChasingState ? chaseStepInterval : patrolStepInterval;
        bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.2f;

        if (!isMoving)
        {
            stepTimer = interval; // bắt đầu đi là có tiếng bước ngay
            enemyStepSound.Stop();
            return;
        }

        stepTimer += Time.deltaTime;
        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            enemyStepSound.PlayAt(transform.position);
        }
    }

}