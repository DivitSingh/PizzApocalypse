using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Handles displaying a damage splash screen when the player loses health.
/// </summary>
public class PlayerDamageSplash : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    
    private Volume volume;
    private Vignette vignette;

    // Config values
    private const float HitIntensity = 0.62f;
    private const float LowHealthRestIntensity = 0.7f;
    private const float LowHealthHitIntensity = 0.78f; // Intensity when health is low but hit again
    private const float LowHealthThreshold = 0.3f;
    
    // State values
    private float intensity = 0.61f;
    private bool isShowingLowHealth = false;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        vignette.intensity.Override(0);
        playerHealth.OnDamageTaken += HandleDamageTaken;
        playerHealth.OnHpPctChanged += HandleHpPctChanged;
    }

    private void HandleDamageTaken()
    {
        StartCoroutine(isShowingLowHealth ? PlayLowHealthEffect() : PlayEffect());
    }

    private void HandleHpPctChanged(float hpPct)
    {
        if (hpPct < LowHealthThreshold)
        {
            if (!isShowingLowHealth) vignette.intensity.Override(LowHealthRestIntensity);
            isShowingLowHealth = true;
        }
        else
        {
            // If currently showing low health splash, slowly animate it back to normal
            if (isShowingLowHealth) StartCoroutine(PlayRecoverEffect());
            isShowingLowHealth = false;
        }
    }

    /// <summary>
    /// Plays a damage splash screen effect for a regular hit.
    /// </summary>
    private IEnumerator PlayEffect()
    {
        vignette.intensity.Override(HitIntensity);
        intensity = HitIntensity;
        yield return new WaitForSeconds(0.5f);

        // Slowly change intensity for fade animation
        while (intensity > 0)
        {
            intensity = Mathf.Max(intensity - 0.02f, 0);
            vignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Plays a damage splash screen effect when the player is already at low health.
    /// </summary>
    private IEnumerator PlayLowHealthEffect()
    {
        vignette.intensity.Override(LowHealthHitIntensity);
        intensity = LowHealthHitIntensity;
        yield return new WaitForSeconds(0.5f);

        // Return effect to that of resting low effect
        while (intensity > LowHealthRestIntensity)
        {
            intensity = Mathf.Max(intensity - 0.02f, LowHealthRestIntensity);
            vignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator PlayRecoverEffect()
    {
        while (intensity > 0)
        {
            intensity = Mathf.Max(intensity - 0.02f, 0);
            vignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.05f);
        }
    }
}