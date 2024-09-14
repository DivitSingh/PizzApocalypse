using System;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private EnemyPatrol _enemyPatrol;
    private EnemyFollow _enemyFollow;

    #region State
    private enum CustomerState
    {
        Chasing,
        Patrolling,
        Waiting
    }

    private CustomerState _state = CustomerState.Chasing;
    #endregion
    

    private void Start()
    {
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _enemyPatrol.enabled = false;
        _enemyPatrol.onPlayerDetected += HandlePlayerDetected;

        _enemyFollow = GetComponent<EnemyFollow>();
        _enemyFollow.enabled = true;
    }

   

    private void HandlePlayerDetected()
    {
        if (_state != CustomerState.Patrolling) return;
        
        _state = CustomerState.Chasing;
        _enemyPatrol.enabled = false;
        _enemyFollow.enabled = true;
    }
}
