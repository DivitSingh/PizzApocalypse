using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    private Transform player;
    public Transform[] points;
    private NavMeshAgent agent;

    public delegate void PlayerDetectedHandler();

    public event PlayerDetectedHandler onPlayerDetected;
    private const float MaxDetectionDistance = 3f;

    private int curPoint;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        GoToNextPoint();
    }

    private void GoToNextPoint()
    {
        if (points.Length == 0) return;

        agent.destination = points[curPoint].position;
        curPoint = (curPoint + 1) % points.Length;
    }

    private void Update()
    {
        var distance = Vector3.Distance(agent.transform.position, player.position);
        if (distance < MaxDetectionDistance)
        {
            onPlayerDetected?.Invoke();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }
}