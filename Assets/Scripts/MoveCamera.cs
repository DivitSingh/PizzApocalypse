using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.position = player.transform.position + new Vector3(0, 0.5f, 0);
    }
}
