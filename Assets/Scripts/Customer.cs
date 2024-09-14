using System;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private int _health;
    private EnemyPatrol _enemyPatrol;
    private EnemyFollow _enemyFollow;

    #region State
    private enum CustomerState
    {
        Chasing, //chasing => angry customer
        Patrolling,
        Waiting  //waiting => hungry customer
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

    // called by a pizzaSlice collision
    public void ReceivePizza(int damage)
    {
        if (_state == CustomerState.Chasing)
        {
            _health -= damage;
            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }
        else if (_state == CustomerState.Waiting)
        {
            Destroy(gameObject);
            //TODO Trigger customer eating a pizza slice animation.
        }
    }
}
