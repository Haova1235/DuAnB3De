using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTrapSpawner))]
public class EnemyPatrol : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyTrapSpawner trapSpawner;

    [Header("Cài đặt Tuần tra")]
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float patrolSpeed = 3.5f;
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        trapSpawner = GetComponent<EnemyTrapSpawner>();
        GoToNextWaypoint();
    }

    public void DoPatrol()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        // KIỂM TRA ĐƯỜNG ĐI THÔNG MINH: Nếu đường đi bị chặn hoàn toàn hoặc không hợp lệ
        if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogWarning("Đường đi tuần tra bị chặn hoặc không hợp lệ! Đang chuyển sang waypoint tiếp theo...");
            // Chuyển hướng sang waypoint kế tiếp để tránh bị kẹt cứng một chỗ
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            GoToNextWaypoint();
            return;
        }

        // Xử lý di chuyển qua lại giữa các Waypoints khi đến đích
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                GoToNextWaypoint();
            }
        }

        // Gọi script đặt bẫy chạy (chỉ khi hàm DoPatrol này được gọi)
        if (trapSpawner != null)
        {
            trapSpawner.TickTrapTimer();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}