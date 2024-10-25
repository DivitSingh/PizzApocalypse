using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    public Image timerImage;
    [NonSerialized] public float maxTime = -1.0f;
    [NonSerialized] public float currentTime;
    public Slider healthBar;
    public GameObject orderPanel;
    [SerializeField] private GameObject slotPrefab;
    private Dictionary<PizzaType, Sprite> pizzaSprites;
    [SerializeField] private Sprite cheesePizzaSprite;
    [SerializeField] private Sprite mushroomPizzaSprite;
    [SerializeField] private Sprite pineapplePizzaSprite;

    private void Awake()
    {
        timerImage.fillAmount = 1f;
        pizzaSprites = new Dictionary<PizzaType, Sprite>
        {
            { PizzaType.Cheese, cheesePizzaSprite},
            { PizzaType.Mushroom, mushroomPizzaSprite},
            { PizzaType.Pineapple, pineapplePizzaSprite}
        };
    }

    private void Update()
    {
        timerImage.transform.LookAt(GameObject.Find("Player").transform);
        orderPanel.transform.LookAt(GameObject.Find("Player").transform);
        if (currentTime > 0 && maxTime != -1.0f)
        {
            currentTime -= Time.deltaTime;
            timerImage.fillAmount = currentTime / maxTime;
        }
    }

    public void UpdateHealthBar(float healthPercentage)
    {
        if (healthBar != null)
            healthBar.value = healthPercentage;
    }

    public void UpdateOrderDisplay(Order orders)
    {
        foreach (Transform child in orderPanel.transform)
            Destroy(child.gameObject);

        foreach (var order in orders.GetOrders())
        {
            foreach (var item in order)
            {
                PizzaType pizzaType = item.Key;
                int count = item.Value;

                for (int i = 0; i < count; i++)
                {
                    GameObject slot = Instantiate(slotPrefab, orderPanel.transform);
                    Image icon = slot.GetComponent<Image>();
                    icon.sprite = pizzaSprites[pizzaType];
                    icon.preserveAspect = true;
                }
            }
        }
    }
}
