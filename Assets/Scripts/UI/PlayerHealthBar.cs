using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHpPctChanged += UpdateHealthBar;
        }
        else
        {
            Debug.LogError("PlayerHealth component not found!");
        }
    }

    private void UpdateHealthBar(float healthPct)
    {
        healthSlider.value = healthPct;
    }
}