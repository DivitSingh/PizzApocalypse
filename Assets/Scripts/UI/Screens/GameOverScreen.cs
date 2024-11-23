using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    public void Show(int round)
    {
        if (round != -1)
            scoreText.text = $"You made it to Round {round} \nCongratulations! See if you can beat that score next time.";
        else
            scoreText.text = "Congratulations! You're ready to make some deliveries!";
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
    }
}