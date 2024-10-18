using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public Vector3 offset;
    private Transform targetTransform;
    private Camera mainCamera;
    private Canvas orderCanvas;
    private RectTransform canvasRect;
    private GameObject orderPanel;
    public GameObject pizzaUIPrefab;
    public Dictionary<PizzaType, Sprite> pizzaSprites;

    [SerializeField] private Sprite cheesePizzaSprite;
    [SerializeField] private Sprite mushroomPizzaSprite;
    [SerializeField] private Sprite pineapplePizzaSprite;

    private void Awake()
    {
        mainCamera = Camera.main;
        pizzaSprites = new Dictionary<PizzaType, Sprite>
        {
            { PizzaType.Cheese, cheesePizzaSprite},
            { PizzaType.Mushroom, mushroomPizzaSprite},
            { PizzaType.Pineapple, pineapplePizzaSprite}
        };
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        CreateCanvas();
    }

    private void LateUpdate()
    {
        if (targetTransform != null && mainCamera != null && orderCanvas != null)
        {
            // Update canvas position to follow the target
            Vector3 targetPosition = targetTransform.position + offset;
            orderCanvas.transform.position = targetPosition;

            // Make the canvas face the camera
            orderCanvas.transform.forward = mainCamera.transform.forward;

            // Scale the canvas based on distance to camera to maintain apparent size
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, targetPosition);
            float scale = distanceToCamera / 8f; // Adjust the divisor to fine-tune the scale
            if (scale > 2f) {scale = 2f;} //Let icon size not get bigger then 10f
            orderCanvas.transform.localScale = Vector3.one * scale;
        }
    }

    public void CreateCanvas()
    {
        // Create a canvas dynamically for each customer order
        GameObject canvasGO = new GameObject("OrderCanvas");
        orderCanvas = canvasGO.AddComponent<Canvas>();
        orderCanvas.renderMode = RenderMode.WorldSpace;
        canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1, 1); // Base size, will be scaled in LateUpdate

        // Add a CanvasScaler to maintain consistent size regardless of resolution
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.scaleFactor = 1;
        scaler.dynamicPixelsPerUnit = 100;

        // Create order panel inside canvas
        orderPanel = new GameObject("OrderPanel");
        RectTransform panelRect = orderPanel.AddComponent<RectTransform>();
        panelRect.SetParent(canvasGO.transform, false);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Set initial position
        if (targetTransform != null)
        {
            orderCanvas.transform.position = targetTransform.position + offset;
        }

        // Hide the canvas initially
        orderCanvas.enabled = false;
    }

    public void DisplayOrderWithSprites(Order customerOrder)
    {
        if (customerOrder == null || orderPanel == null)
        {
            Debug.LogError("Customer order or order panel is null!");
            return;
        }

        // Clear existing sprites
        foreach (Transform child in orderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        Dictionary<PizzaType, int>[] Orders = customerOrder.GetOrders();

        float xOffset = 0f;
        float yOffset = 0;
        float iconSize = 0.5f; // Adjust this value to change the size of pizza icons

        foreach (var order in Orders)
        {
            foreach (var item in order)
            {
                PizzaType pizzaType = item.Key;
                int count = item.Value;

                for (int i = 0; i < count; i++)
                {
                    GameObject pizzaUI = Instantiate(pizzaUIPrefab, orderPanel.transform);
                    RectTransform rectTransform = pizzaUI.GetComponent<RectTransform>();
                    Image pizzaImage = pizzaUI.GetComponent<Image>();

                    pizzaImage.sprite = pizzaSprites[pizzaType];
                    pizzaImage.preserveAspect = true;

                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                    rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);

                    xOffset += iconSize;
                    if (xOffset > 1.5f) // Wrap to next line if too wide
                    {
                        xOffset = 0;
                        yOffset -= iconSize;
                    }
                }
            }
        }

        // Adjust canvas size based on content
        canvasRect.sizeDelta = new Vector2(Mathf.Max(1, xOffset), Mathf.Max(1, -yOffset));
        orderCanvas.enabled = true;
    }

    public void UpdateOrderDisplay(Order orderToUpdateDisplay)
    {
        if (orderPanel == null)
        {
            Debug.LogError("Order panel is null!");
            return;
        }

        // Clear existing sprites
        foreach (Transform child in orderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        DisplayOrderWithSprites(orderToUpdateDisplay);
    }

    public void RemoveOrderUI()
    {
        if (orderCanvas != null)
        {
            Destroy(orderCanvas.gameObject);
        }
    }
}