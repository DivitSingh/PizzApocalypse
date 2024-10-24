using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    public Slider healthBar;
    public Image timerImage;
    [NonSerialized] public float maxTime = -1.0f;
    private float currentTime;

    private void Start()
    {
        currentTime = maxTime;
        timerImage.fillAmount = 1f;
    }

    private void Update()
    {
        timerImage.transform.LookAt(GameObject.Find("Player").transform);
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
}
