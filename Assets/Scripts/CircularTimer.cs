using UnityEngine;
using UnityEngine.UI;

public class CircularTimer : MonoBehaviour
{
    public Image timerImage;
    public float maxTime = 3f;
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    private float currentTime;
    private Transform targetTransform;
    private Camera mainCamera;
    private RectTransform rectTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        if (timerImage == null)
        {
            timerImage = GetComponent<Image>();
        }
        if (timerImage == null)
        {
            Debug.LogError("Image component not found in CircularTimer!");
        }
    }

    private void OnEnable()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            timerImage.fillAmount = currentTime / maxTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
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

    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    public void ResetTimer()
    {
        currentTime = maxTime;
        timerImage.fillAmount = 1f;
    }
}
