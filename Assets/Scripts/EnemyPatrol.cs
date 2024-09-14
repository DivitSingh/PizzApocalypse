using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public Transform player;
    public Transform[] points;
    private NavMeshAgent _agent;

    public delegate void PlayerDetectedHandler();

    public event PlayerDetectedHandler onPlayerDetected;
    private const float MaxDetectionDistance = 3f;
    
    private int _curPoint;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.autoBraking = false;
        GoToNextPoint();
    }

    private void GoToNextPoint()
    {
        if (points.Length == 0) return;

        _agent.destination = points[_curPoint].position;
        _curPoint = (_curPoint + 1) % points.Length;
    }

    private void Update()
    {
        var distance = Vector3.Distance(_agent.transform.position, player.position);
        if (distance < MaxDetectionDistance)
        {
            onPlayerDetected?.Invoke();
        } else if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }
}