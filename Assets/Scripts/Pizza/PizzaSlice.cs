using UnityEngine;

// TODO: Can probably remove this script eventually? Use ThrowablePizza instead
public class PizzaSlice : MonoBehaviour
{

    // How much damage the pizzaSlice does
    [SerializeField] private int damage;
    private float time;

    // Start is called before the first frame update
    private void Start()
    {
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - time > 2.5f)
            Destroy(gameObject);
    }

}
