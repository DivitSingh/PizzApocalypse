using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the UI for an active order in the order bar.
/// </summary>
public class OrderUI : MonoBehaviour
{
    [Header("Local UI Elements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image panel;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text moneyLabel;
    [SerializeField] private GameObject itemsGrid;
    [SerializeField] private TMP_Text numberLabel;
    private Image sliderFill;

    [Header("Icons and Prefabs")]
    [SerializeField] private Sprite cheeseIcon;
    [SerializeField] private Sprite pineappleIcon;
    [SerializeField] private Sprite mushroomIcon;
    [SerializeField] private GameObject itemSlot;
    private Dictionary<PizzaType, Sprite> iconMap;

    private static readonly Color LowTimerColor = new(217f / 255, 35f / 255, 11f / 225);
    private static readonly Color MidTimerColor = new(254f / 255, 215f / 255, 50f / 255);
    private static readonly Color HighTimerColor = new(57f / 255, 229f / 255, 5f / 255);

    // Animation configurations
    private static readonly Vector2 SlideStartPosition = new Vector2(0, 400);
    private static readonly Vector2 SlideEndPosition = Vector2.zero;
    private const float SlideDuration = 0.3f;

    private const float RemoveDuration = 0.4f;
    private static readonly Color FailedOrderColor = new(241f / 255, 86f / 255, 49f / 225);
    private static readonly Color CompletedOrderColor = new(57f / 255, 229f / 255, 5f / 255);

    private Customer customer;
    private float initialPatience;

    private void Awake()
    {
        sliderFill = slider.GetComponentsInChildren<Image>().FirstOrDefault(c => c.name == "Fill");
        iconMap = new Dictionary<PizzaType, Sprite>()
        {
            { PizzaType.Cheese, cheeseIcon },
            { PizzaType.Mushroom, mushroomIcon },
            { PizzaType.Pineapple, pineappleIcon }
        };

        StartCoroutine(PlaySlideAnimation());
    }

    public void Configure(Customer customer)
    {
        var order = customer.Order;
        moneyLabel.text = $"${order.Value}";
        numberLabel.text = customer.Id.ToString();
        ConfigureItems(customer.Order);
        this.customer = customer;
        initialPatience = customer.Patience;
    }

    private void ConfigureItems(Order order)
    {
        foreach (var (pizzaType, amount) in order.Items)
        {
            for (int i = 0; i < amount; i++)
            {
                var slot = Instantiate(itemSlot, itemsGrid.transform);
                var image = slot.GetComponent<Image>();
                image.sprite = iconMap[pizzaType];
                image.preserveAspect = true;
            }
        }
    }

    private void Update()
    {
        UpdateSlider(customer.Patience, initialPatience);
    }

    private void UpdateSlider(float timeRemaining, float initialTime)
    {
        var pct = timeRemaining / initialTime;
        sliderFill.color = pct switch
        {
            <= 0.33f => LowTimerColor,
            <= 0.67f => MidTimerColor,
            _ => HighTimerColor
        };
        slider.value = pct;
    }

    public void Remove(bool success)
    {
        StopAllCoroutines();
        if (!success) slider.value = 0;
        var animColor = success ? CompletedOrderColor : FailedOrderColor;
        StartCoroutine(FadeOut(animColor));
    }

    #region Animations
    private IEnumerator PlaySlideAnimation()
    {
        var innerContainer = transform.GetChild(0).GetComponent<RectTransform>();
        innerContainer.anchoredPosition = SlideStartPosition;

        var elapsedTime = 0f;
        while (elapsedTime < SlideDuration)
        {
            innerContainer.anchoredPosition =
                Vector2.Lerp(SlideStartPosition, SlideEndPosition, elapsedTime / SlideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        innerContainer.anchoredPosition = SlideEndPosition;
    }

    private IEnumerator FadeOut(Color color)
    {
        // Set panel and shadow colors
        var shadows = panel.gameObject.GetComponents<Shadow>();
        foreach (var shadow in shadows) { shadow.effectColor = color; }
        panel.color = color;

        // Play fade animation
        var elapsedTime = 0f;
        while (elapsedTime < RemoveDuration)
        {
            var alpha = Mathf.Lerp(1, 0, elapsedTime / RemoveDuration);
            canvasGroup.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
    }

    #endregion
}