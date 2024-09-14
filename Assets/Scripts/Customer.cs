using System;
using System.Collections;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private int _health;
    private EnemyPatrol _enemyPatrol;
    private EnemyFollow _enemyFollow;
    
    public float waitingTime = 3f;

    #region State
    public enum CustomerState
    {
        Chasing, //chasing => angry customer
        Patrolling,
        Waiting  //waiting => hungry customer
    }

    private CustomerState _state = CustomerState.Patrolling;
    #endregion

    private void Start()
    {
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _enemyPatrol.enabled = false;
        _enemyPatrol.onPlayerDetected += HandlePlayerDetected;

        _enemyFollow = GetComponent<EnemyFollow>();
        _enemyFollow.enabled = false;
        
        ChangeState(CustomerState.Waiting);
    }

    private void HandlePlayerDetected()
    {
        if (_state != CustomerState.Patrolling) return;

        _state = CustomerState.Chasing;
        _enemyPatrol.enabled = false;
        _enemyFollow.enabled = true;
    }
    
    private IEnumerator WaitForFood()
    {
        yield return new WaitForSeconds(waitingTime);
        ChangeState(CustomerState.Chasing);
    }

    private void ChangeState(CustomerState newState)
    {
        if (newState == _state) return;
        _state = newState;
        switch (newState)
        {
            case CustomerState.Chasing:
                _enemyPatrol.enabled = false;
                _enemyFollow.enabled = true;
                break;
            case CustomerState.Waiting:
                StartCoroutine(WaitForFood());
                _enemyFollow.enabled = false;
                _enemyFollow.enabled = false;
                break;
            case CustomerState.Patrolling:
                _enemyFollow.enabled = false;
                _enemyPatrol.enabled = true;
                break;
            default:
                break;
        }
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
            StopCoroutine(WaitForFood());
            Destroy(gameObject);
            //TODO Trigger customer eating a pizza slice animation.
        }
    }
}
