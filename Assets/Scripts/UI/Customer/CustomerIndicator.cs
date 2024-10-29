using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles displaying an indicator for a given customer relative to the player's current position.
/// </summary>
public class CustomerIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    private GameObject canvas;
    private Camera playerCamera;
    private GameObject indicator;
    private Image image;

    private const float HeightOffset = 3f;
    private const float BorderSize = 8f;

    private void Awake()
    {
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        canvas = GameObject.FindWithTag("Indicators");
        
        indicator = Instantiate(indicatorPrefab, canvas.transform);
        image = indicator.GetComponent<Image>();
    }

    private void Start()
    {
        var idLabel = indicator.GetComponentInChildren<TMP_Text>();
        idLabel.text = GetComponent<Customer>().Id.ToString();
    }

    private void Update()
    {
        float minX = BorderSize + image.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;
        float minY = BorderSize + image.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;
        
        // Compute world position relative to player
        var worldPos = playerCamera.WorldToScreenPoint(transform.position + Vector3.up * HeightOffset);
        var direction = (transform.position - playerCamera.transform.position).normalized;
        var isBehind = Vector3.Dot(direction, playerCamera.transform.forward.normalized) < 0;
        var isOffScreen = worldPos.x <= 0 || worldPos.x >= Screen.width || worldPos.y <= 0 ||
                           worldPos.y >= Screen.height || isBehind;

        if (!isOffScreen)
        {
            // Hide indicator if player can directly see customer
            if (Physics.Raycast(playerCamera.transform.position + Vector3.up, direction, out var hit))
            {
                var customerComponent = hit.collider.gameObject.GetComponent<Customer>();
                if (customerComponent != null || hit.collider.GetComponent<ThrowablePizza>() != null)
                {
                    // NOTE: Need to check for ThrowablePizza, else indicator reappears for a split second on throws
                    indicator.SetActive(false);
                }
                else
                {
                    // Not visible due to some obstruction
                    indicator.SetActive(true);
                }
                
            }

            image.transform.position = worldPos;
            return;
        }
        
        // Customer is off-screen, find the nearest edge to display
        indicator.SetActive(true);
        if (worldPos.x < 0)
        {
            // Display on left edge unless it is behind, which requires flipping
            worldPos.x = isBehind ? maxX : minX;
            worldPos.y = Mathf.Clamp(worldPos.y, minY, maxY);
        }
        else if (worldPos.x > Screen.width)
        {
            // Display on right edge unless it is behind, which requires flipping
            worldPos.x = maxX;
            worldPos.x = isBehind ? minX : maxX;
            worldPos.y = Mathf.Clamp(worldPos.y, minY, maxY);
        }
        else
        {
            // Display on bottom edge and flip x coordinate
            worldPos.y = minY;
            worldPos.x = Screen.width - worldPos.x;
        }

        image.transform.position = worldPos;
    }

    private void OnDestroy()
    {
        Destroy(indicator);
    }
}
