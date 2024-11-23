using System;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Manages the soundtrack during round gameplay.
/// </summary>
public class SoundTrackManager: MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioClip shopClip;
    [SerializeField] private RoundManager roundManager;

    private int clipIndex = 0;

    private void Awake()
    {
        ChangeTrack();
        roundManager.OnNewRound += _ => ChangeTrack();
        roundManager.OnRoundEnd += passed =>
        {
            if (passed)
            {
                audioSource.clip = shopClip;
                audioSource.Play();
            }
        };
    }

    private void ChangeTrack()
    {
        audioSource.Stop();
        clipIndex = (clipIndex + 1) % clips.Length;
        var clip = clips[clipIndex];
        audioSource.clip = clip;
        audioSource.Play();
    }
}