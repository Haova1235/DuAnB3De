using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTrapSpawner))] // Tự động yêu cầu component đặt bẫy
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

        // Xử lý di chuyển qua lại giữa các Waypoints
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            GoToNextWaypoint();
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