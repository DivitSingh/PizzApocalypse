using UnityEngine;

// NOTE: I added this script temporarily instead of refactoring PizzaSlice, but eventually we should remove one of them
public class ThrowablePizza : MonoBehaviour
{
    private float time;
    public IPizza Pizza { get; private set; }

    private void Start()
    {
        time = Time.time;
    }

    public void Initialize(IPizza pizza)
    {
        Pizza = pizza;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Customer>() != null)
        {
            Customer currentCustomer = other.gameObject.GetComponent<Customer>();
            currentCustomer.ReceivePizza(Pizza);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Time.time - time > 2.5f)
            Destroy(gameObject);
    }
}