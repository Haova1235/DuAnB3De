using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Cài đặt Tuần tra")]
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float patrolSpeed = 3.5f;
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextWaypoint();
    }

    public void DoPatrol()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}