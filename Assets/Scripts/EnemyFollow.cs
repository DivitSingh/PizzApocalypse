using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    private Transform player;

    private NavMeshAgent agent;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.destination = player.position;
    }

    private void Update()
    {
        agent.destination = player.position;
    }
}
