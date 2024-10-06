using System;
using TMPro;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text progressText;

    private void Start()
    {
        roundManager.OnTimeRemainingChanged += UpdateTimer;
        roundManager.OnProgressChanged += UpdateProgress;
    }

    private void UpdateTimer(float timeRemaining)
    {
        if (timeRemaining < 0)
        {
            timerText.text = "0:00";
            return;
        }
        
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"{minutes:0}:{seconds:00}";
    }

    private void UpdateProgress(int score, int passScore)
    {
        // TODO: Update UI for this later (color and format)?
        progressText.text = $"{score} / {passScore}";
    }
}