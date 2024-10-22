using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public RectTransform[] menuItems;
    public RectTransform selectionHighlight;
    private int currentIndex = 0;
    private bool hasInitialized = false;

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        // Set initial index
        currentIndex = 0;
    }

    void LateUpdate()
    {
        // Initialize position on the first frame after all UI elements are properly positioned
        if (!hasInitialized)
        {
            UpdateSelectionHighlight();
            hasInitialized = true;
        }

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
            case 0: // Play
                SceneManager.LoadScene("SampleScene");
                break;
            case 1: // Options
                Debug.Log("Options selected");
                break;
            case 2: // Exit
                Application.Quit();
                Debug.Log("Exit selected");
                break;
        }
    }
}