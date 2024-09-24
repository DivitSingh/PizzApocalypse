using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void Show(int score)
    {
        scoreText.text = $"You made {score} {(score == 1 ? "delivery" : "deliveries")}.";
        gameObject.SetActive(true);
    }

    public void HandleRestart()
    {
        GameManager.Instance.Restart();
    }
}
