using System;   
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFill;

    private static readonly Color LowProgressColor = new(238f / 255, 85f / 255, 95f / 225);
    private static readonly Color MidProgressColor = new(254f / 255, 215f / 255, 50f / 255);
    private static readonly Color HighProgressColor = new(57f / 255, 229f / 255, 5f / 255);

    private void Awake()
    {
        progressSlider.value = 0;
        roundManager.OnTimeRemainingChanged += UpdateTimer;
        roundManager.OnProgressChanged += UpdateProgress;
        roundManager.OnNewRound += UpdateRound;
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (timeRemaining < 0)
        {
            timerText.text = "0:00";
            return;
        }
        
        var minutes = Mathf.FloorToInt(timeRemaining / 60);
        var seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"{minutes:0}:{seconds:00}";
    }

    private void UpdateProgress(int score, int passScore)
    {
        progressText.text = $"{score}/{passScore}";
        var progress = Mathf.InverseLerp(0, passScore, score);
        progressSlider.value = progress;

        if (progress <= 0.3)
        {
            progressFill.color = LowProgressColor;
        } else if (progress <= 0.7)
        {
            progressFill.color = MidProgressColor;
        }
        else
        {
            progressFill.color = HighProgressColor;
        }
    }

    private void UpdateRound(int round)
    {
        gameObject.SetActive(true);
        roundText.text = $"Round {round}";
    }
}