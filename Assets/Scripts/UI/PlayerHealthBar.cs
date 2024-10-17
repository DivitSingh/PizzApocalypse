using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Player playerHealth;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        playerHealth = FindObjectOfType<Player>();

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