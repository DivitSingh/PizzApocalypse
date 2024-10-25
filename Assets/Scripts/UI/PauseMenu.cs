using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public RectTransform[] menuItems;
    public RectTransform selectionHighlight;
    [SerializeField] private InputAction pauseControls;
    private bool isPaused = false;
    private int currentIndex = 0;
    private bool needsInitialPosition = false;

    void Start()
    {
        // Ensure menu is hidden at start
        pauseMenuUI.SetActive(false);
    }

    private void OnEnable()
    {
        pauseControls.Enable();
        pauseControls.performed += PausePerformed;
    }

    private void OnDisable()
    {
        pauseControls.Disable();
    }

    private void PausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    void LateUpdate()
    {
        // Check if we need to set initial position
        if (needsInitialPosition)
        {
            UpdateSelectionHighlight();
            needsInitialPosition = false;
        }

        if (isPaused)
        {
            // Allow both keyboard and controller vertical input
            float verticalInput = Input.GetKeyDown(KeyCode.DownArrow) ? 1 : (Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 0);

            // If no keyboard input, check controller
            if (verticalInput == 0 && Input.GetAxisRaw("Vertical") != 0)
            {
                // Only trigger once when the stick/dpad is pushed
                if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f)
                    verticalInput = -Mathf.Sign(Input.GetAxisRaw("Vertical"));
            }

            if (verticalInput > 0)
            {
                currentIndex = (currentIndex + 1) % menuItems.Length;
                UpdateSelectionHighlight();
            }
            else if (verticalInput < 0)
            {
                currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
                UpdateSelectionHighlight();
            }

            // Check for both keyboard Return and controller Submit button
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
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
