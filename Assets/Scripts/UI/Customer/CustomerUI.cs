using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    public Slider timerSlider;
    [NonSerialized] public float maxTime = -1.0f;
    [NonSerialized] public float currentTime;
    public Slider healthBar;
    public GameObject orderPanel;
    [SerializeField] private GameObject slotPrefab;
    private Dictionary<PizzaType, Sprite> pizzaSprites;
    [SerializeField] private Sprite cheesePizzaSprite;
    [SerializeField] private Sprite mushroomPizzaSprite;
    [SerializeField] private Sprite pineapplePizzaSprite;

    private static readonly Color LowTimerColor = new(217f / 255, 35f / 255, 11f / 225);
    private static readonly Color MidTimerColor = new(254f / 255, 215f / 255, 50f / 255);
    private static readonly Color HighTimerColor = new(57f / 255, 229f / 255, 5f / 255);

    private void Awake()
    {
        timerSlider.value = 1f;
        pizzaSprites = new Dictionary<PizzaType, Sprite>
        {
            { PizzaType.Cheese, cheesePizzaSprite},
            { PizzaType.Mushroom, mushroomPizzaSprite},
            { PizzaType.Pineapple, pineapplePizzaSprite}
        };
    }

    private void Update()
    {
        timerSlider.transform.LookAt(GameObject.Find("Player").transform);
        orderPanel.transform.LookAt(GameObject.Find("Player").transform);
        if (currentTime > 0 && maxTime != -1.0f)
        {
            currentTime -= Time.deltaTime;
            timerSlider.value = currentTime / maxTime;
            timerSlider.GetComponentsInChildren<Image>().FirstOrDefault(c => c.name == "Fill").color = timerSlider.value switch
            {
                <= 0.33f => LowTimerColor,
                <= 0.67f => MidTimerColor,
                _ => HighTimerColor
            };
        }
    }

    public void UpdateHealthBar(float healthPercentage)
    {
        if (healthBar != null)
            healthBar.value = healthPercentage;
    }

    public void UpdateOrderDisplay(Order order)
    {
        foreach (Transform child in orderPanel.transform)
            Destroy(child.gameObject);

        foreach (var (pizzaType, count) in order.Items)
        {
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
