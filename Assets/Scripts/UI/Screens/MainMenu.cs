using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public RectTransform[] menuItems;
    public RectTransform selectionHighlight;

    // Add these new audio-related fields
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip selectSound;

    private int currentIndex = 0;
    private bool hasInitialized = false;
    private bool wasVerticalPressed = false;

    [Header("Transition")]
    [SerializeField] private GameObject transitionCanvas;
    public Animator transition;
    private float transitionTime;
    public Text loadingText;

    void Start()
    {
        transitionCanvas.SetActive(false);
        // If audioSource isn't assigned in inspector, add it automatically
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Canvas.ForceUpdateCanvases();
        currentIndex = 0;

        for (int i = 0; i < menuItems.Length; i++)
        {
            int index = i;
            EventTrigger trigger = menuItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = menuItems[i].gameObject.AddComponent<EventTrigger>();
            }

            // Modify hover event to include sound
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) =>
            {
                if (currentIndex != index) // Only play sound if hovering over a new item
                {
                    PlayHoverSound();
                }
                currentIndex = index;
                UpdateSelectionHighlight();
            });
            trigger.triggers.Add(enterEntry);

            // Modify click event to include sound
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) =>
            {
                currentIndex = index;
                PlaySelectSound();
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

        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetAxisRaw("Vertical") > 0.5f && !wasVerticalPressed);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetAxisRaw("Vertical") < -0.5f && !wasVerticalPressed);

        wasVerticalPressed = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f;

        if (downPressed || upPressed)
        {
            // Play hover sound when using keyboard/gamepad navigation
            PlayHoverSound();

            if (downPressed)
            {
                currentIndex = (currentIndex + 1) % menuItems.Length;
            }
            else
            {
                currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            }
            UpdateSelectionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
        {
            PlaySelectSound();
            SelectMenuItem();
        }
    }

    // Add these new methods for playing sounds
    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectSound);
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
                StartCoroutine(LoadLevelSceneWithTransition());
                // old code below
                // SceneManager.LoadScene("LevelScene");
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

    IEnumerator LoadLevelSceneWithTransition()
    {
        transitionCanvas.SetActive(true);
        transitionTime = 2.5f;
        transition.SetTrigger("Start");
        Time.timeScale = 1;
        loadingText.text = "Round starts in 3.. 2.. 1..";
        yield return new WaitForSeconds(transitionTime);
        loadingText.text = "";
        SceneManager.LoadScene("LevelScene");
    }
}