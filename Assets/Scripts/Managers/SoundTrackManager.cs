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
    private Random random = new Random();

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
        var clip = SelectRandomClip();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private AudioClip SelectRandomClip()
    {
        var index = random.Next(0, clips.Length);
        return clips[index];
    }
}