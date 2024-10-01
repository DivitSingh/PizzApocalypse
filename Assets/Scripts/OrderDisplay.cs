using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public Vector3 offset;
    private Transform targetTransform;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Image pizzaImage;

    [SerializeField] private Sprite cheesePizzaSprite;
    [SerializeField] private Sprite mushroomPizzaSprite;
    [SerializeField] private Sprite pineapplePizzaSprite;

    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        pizzaImage = GetComponent<Image>();
        if (pizzaImage == null)
        {
            Debug.LogError("Image component not found in OrderDisplay!");
        }
        else
        {
            // Set the Image type to Simple
            pizzaImage.type = Image.Type.Simple;
            // Make sure the native size is preserved
            pizzaImage.preserveAspect = true;
        }
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    private void LateUpdate()
    {
        if (targetTransform != null && mainCamera != null)
        {
            Vector3 worldPosition = targetTransform.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                screenPosition,
                null,
                out canvasPosition);
            rectTransform.anchoredPosition = canvasPosition;
        }
    }

    public void UpdateOrderDisplay(PizzaType pizzaType)
    {
        if (pizzaImage != null)
        {
            Sprite selectedSprite = null;
            switch (pizzaType)
            {
                case PizzaType.Cheese:
                    selectedSprite = cheesePizzaSprite;
                    break;
                case PizzaType.Mushroom:
                    selectedSprite = mushroomPizzaSprite;
                    break;
                case PizzaType.Pineapple:
                    selectedSprite = pineapplePizzaSprite;
                    break;
                // Add more cases as needed
                default:
                    Debug.LogWarning($"No sprite assigned for pizza type: {pizzaType}");
                    break;
            }

            if (selectedSprite != null)
            {
                pizzaImage.sprite = selectedSprite;
                pizzaImage.enabled = true;
            }
            else
            {
                pizzaImage.enabled = false;
            }
        }
        else
        {
            Debug.LogError("Image component is null in OrderDisplay!");
        }
    }
}