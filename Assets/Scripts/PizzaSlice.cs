using UnityEngine;

public class PizzaSlice : MonoBehaviour
{

    // How much damage the pizzaSlice does
    public int damage;
    private float time;

    // Start is called before the first frame update
    private void Start()
    {
        time = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Customer>() != null)
        {
            Customer currentCustomer = other.gameObject.GetComponent<Customer>();
            currentCustomer.ReceivePizza(damage);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - time > 2.5f)
            Destroy(gameObject);
    }

}
