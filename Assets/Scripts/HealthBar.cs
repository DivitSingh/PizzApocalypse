using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    public Vector3 offset;

    private Transform targetTransform;
    private Camera mainCamera;
    private RectTransform rectTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        slider = GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogError("Slider component not found in HealthBar!");
        }
    }

    public void SetSlider(Slider newSlider)
    {
        slider = newSlider;
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

    public void UpdateHealthBar(float healthPercentage)
    {
        if (slider != null)
        {
            slider.value = healthPercentage;
        }
        else
        {
            Debug.LogError("Slider is null in HealthBar!");
        }
    }
}