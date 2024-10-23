using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public RectTransform[] menuItems;
    public RectTransform selectionHighlight;
    private bool isPaused = false;
    private int currentIndex = 0;
    private bool needsInitialPosition = false;

    void Start()
    {
        // Ensure menu is hidden at start
        pauseMenuUI.SetActive(false);
    }

    void LateUpdate()
    {
        // Check if we need to set initial position
        if (needsInitialPosition)
        {
            UpdateSelectionHighlight();
            needsInitialPosition = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentIndex = (currentIndex + 1) % menuItems.Length;
                UpdateSelectionHighlight();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
                UpdateSelectionHighlight();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                SelectMenuItem();
            }
        }
    }

    void UpdateSelectionHighlight()
    {
        if (menuItems == null || menuItems.Length == 0) return;
        selectionHighlight.position = menuItems[currentIndex].position;
        selectionHighlight.sizeDelta = menuItems[currentIndex].sizeDelta;
    }

    void SelectMenuItem()
    {
        switch (currentIndex)
        {
            case 0:
                Time.timeScale = 1f;
                SceneManager.LoadScene("StartScene");
                break;
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Canvas.ForceUpdateCanvases();
        Time.timeScale = 0f;
        isPaused = true;
        currentIndex = 0;
        needsInitialPosition = true;
    }

    void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
