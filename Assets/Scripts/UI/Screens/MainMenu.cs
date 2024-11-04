using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Add this for UI event handling

public class MainMenu : MonoBehaviour
{
    public RectTransform[] menuItems;
    public RectTransform selectionHighlight;
    private int currentIndex = 0;
    private bool hasInitialized = false;
    private bool wasVerticalPressed = false;

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        currentIndex = 0;

        // Add event triggers to each menu item
        for (int i = 0; i < menuItems.Length; i++)
        {
            int index = i; // Capture the index for use in lambda expressions

            // Add EventTrigger component if it doesn't exist
            EventTrigger trigger = menuItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = menuItems[i].gameObject.AddComponent<EventTrigger>();
            }

            // Add pointer enter (hover) event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                currentIndex = index;
                UpdateSelectionHighlight();
            });
            trigger.triggers.Add(enterEntry);

            // Add click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => {
                currentIndex = index;
                SelectMenuItem();
            });
            trigger.triggers.Add(clickEntry);
        }
    }

    void LateUpdate()
    {
        if (!hasInitialized)
        {
            UpdateSelectionHighlight();
            hasInitialized = true;
        }

        // Get gamepad/keyboard vertical input
        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetAxisRaw("Vertical") > 0.5f && !wasVerticalPressed);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetAxisRaw("Vertical") < -0.5f && !wasVerticalPressed);

        // Track if vertical was pressed this frame
        wasVerticalPressed = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f;

        if (downPressed)
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateSelectionHighlight();
        }
        else if (upPressed)
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateSelectionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
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
            case 0:
                SceneManager.LoadScene("LevelScene");
                break;
            case 1:
                SceneManager.LoadScene("OptionsScene");
                break;
            case 2:
                Application.Quit();
                Debug.Log("Exit selected");
                break;
        }
    }
}