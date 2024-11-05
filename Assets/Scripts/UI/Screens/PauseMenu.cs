using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu Objects")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("Pause Menu Elements")]
    public RectTransform[] pauseMenuItems;
    public RectTransform pauseSelectionHighlight;

    [Header("Options Menu Elements")]
    [Header("Sliders")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    [Header("Sensitivity Settings")]
    [SerializeField] private float minSensitivity = 50f;
    [SerializeField] private float maxSensitivity = 500f;
    [SerializeField] private float defaultSensitivity = 200f;

    [Header("Titles")]
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI sensitivityText;

    [Header("Input Display")]
    public Button inputToggleButton;
    public Image controllerMappingImage;
    public Image keyboardMappingImage;
    public TextMeshProUGUI toggleButtonText;

    [Header("Options Layout")]
    public RectTransform[] optionsItems;
    public RectTransform optionsSelectionHighlight;
    public float constantHighlightHeight = 60f;
    public float constantHighlightWidth = 300f;
    public float sliderSpeed = 1f;

    // Input System
    private PlayerInput playerInput;
    private InputAction pauseAction;
    private InputAction navigationAction;
    private InputAction submitAction;
    private InputAction horizontalAction;
    private InputAction backAction;
    private InputAction mousePositionAction;
    private InputAction mouseClickAction;

    private bool isPaused = false;
    private bool inOptionsMenu = false;
    private int currentPauseIndex = 0;
    private int currentOptionsIndex = 0;
    private bool isSliderSelected => inOptionsMenu && (currentOptionsIndex == 0 || currentOptionsIndex == 1);
    private bool isDraggingSlider = false;
    private bool showingController = true;
    private float lastNavigationTime;
    private float navigationBuffer = 0.25f;
    public float controllerSliderSpeed = 5f;
    public float mouseKeyboardSliderSpeed = 1f;

    private void Awake()
    {
        // Setup input actions
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");

        navigationAction = new InputAction("Navigate", InputActionType.Value);
        navigationAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Keyboard>/downArrow")
            .With("Negative", "<Keyboard>/upArrow");
        navigationAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Gamepad>/dpad/down")
            .With("Negative", "<Gamepad>/dpad/up");
        navigationAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Gamepad>/leftStick/down")
            .With("Negative", "<Gamepad>/leftStick/up");

        horizontalAction = new InputAction("Horizontal", InputActionType.Value);
        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Keyboard>/rightArrow")
            .With("Negative", "<Keyboard>/leftArrow");
        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Gamepad>/dpad/right")
            .With("Negative", "<Gamepad>/dpad/left");
        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Gamepad>/leftStick/right")
            .With("Negative", "<Gamepad>/leftStick/left");

        submitAction = new InputAction("Submit", InputActionType.Button);
        submitAction.AddBinding("<Keyboard>/enter");
        submitAction.AddBinding("<Gamepad>/buttonSouth");

        backAction = new InputAction("Back", InputActionType.Button);
        backAction.AddBinding("<Keyboard>/backspace");
        backAction.AddBinding("<Gamepad>/buttonEast");

        mousePositionAction = new InputAction("MousePosition", InputActionType.Value);
        mousePositionAction.AddBinding("<Mouse>/position");

        mouseClickAction = new InputAction("MouseClick", InputActionType.Button);
        mouseClickAction.AddBinding("<Mouse>/leftButton");

        // Set initial states
        isPaused = false;
        inOptionsMenu = false;
        sliderSpeed = mouseKeyboardSliderSpeed;
    }

    private void HandleMouseHover()
    {
        Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Check pause menu items
            if (!inOptionsMenu)
            {
                for (int i = 0; i < pauseMenuItems.Length; i++)
                {
                    if (result.gameObject == pauseMenuItems[i].gameObject)
                    {
                        currentPauseIndex = i;
                        UpdatePauseHighlight();
                        return;
                    }
                }
            }
            // Check options menu items
            else
            {
                for (int i = 0; i < optionsItems.Length; i++)
                {
                    if (result.gameObject == optionsItems[i].gameObject)
                    {
                        currentOptionsIndex = i;
                        UpdateOptionsHighlight();
                        return;
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        // Enable all actions
        pauseAction.Enable();
        navigationAction.Enable();
        submitAction.Enable();
        horizontalAction.Enable();
        backAction.Enable();
        mousePositionAction.Enable();
        mouseClickAction.Enable();

        // Add callbacks
        pauseAction.performed += OnPausePressed;
        navigationAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        horizontalAction.performed += OnHorizontal;
        backAction.performed += OnBack;
        mouseClickAction.performed += OnMouseClick;
    }

    private void OnDisable()
    {
        // Remove callbacks
        pauseAction.performed -= OnPausePressed;
        navigationAction.performed -= OnNavigate;
        submitAction.performed -= OnSubmit;
        horizontalAction.performed -= OnHorizontal;
        backAction.performed -= OnBack;
        mouseClickAction.performed -= OnMouseClick;

        // Disable actions
        pauseAction.Disable();
        navigationAction.Disable();
        submitAction.Disable();
        horizontalAction.Disable();
        backAction.Disable();
        mousePositionAction.Disable();
        mouseClickAction.Disable();
    }

    private void Update()
    {
        if (isPaused && !isDraggingSlider)
        {
            HandleMouseHover();
        }
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!isPaused) return;

        if (inOptionsMenu)
        {
            HandleOptionsSelection();
        }
        else
        {
            HandlePauseSelection();
        }
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        if (!isPaused) return;

        if (inOptionsMenu)
        {
            ShowPauseMenu();
        }
        else
        {
            Resume();
        }
    }

    private void Start()
    {
        // Ensure both menus are hidden at start
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        InitializeSliders();
        SetupEventTriggers();
    }

    private void InitializeSliders()
    {
        // Volume slider setup
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // Sensitivity slider setup
        sensitivitySlider.minValue = 0f;
        sensitivitySlider.maxValue = 1f;
        sensitivitySlider.wholeNumbers = false;

        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", defaultSensitivity);
        float sliderValue = Mathf.InverseLerp(minSensitivity, maxSensitivity, savedSensitivity);
        sensitivitySlider.value = sliderValue;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        UpdatePlayerSensitivity(savedSensitivity);

        if (inputToggleButton != null)
        {
            inputToggleButton.onClick.AddListener(ToggleInputDisplay);
        }

        UpdateInputDisplay();
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        Debug.Log("Pause key pressed"); // Debug log

        if (inOptionsMenu)
        {
            ShowPauseMenu();
        }
        else if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private bool needsPauseHighlightInit = false;
    private bool needsOptionsHighlightInit = false;

    private void Pause()
    {
        Debug.Log("Pausing game");
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        inOptionsMenu = false;
        currentPauseIndex = 0;

        // Force immediate highlight update
        Canvas.ForceUpdateCanvases();
        UpdatePauseHighlight();
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!isPaused) return;

        // Add device-specific buffer timing
        float currentBuffer = context.control.device is Gamepad ? navigationBuffer * 1.5f : navigationBuffer;
        if (Time.unscaledTime - lastNavigationTime < currentBuffer) return;

        float value = context.ReadValue<float>();

        // Add deadzone for analog stick to prevent accidental navigation
        if (context.control.device is Gamepad && Mathf.Abs(value) < 0.5f) return;

        if (value != 0)
        {
            if (inOptionsMenu)
            {
                NavigateOptions(value > 0);
            }
            else
            {
                NavigatePauseMenu(value > 0);
            }
            lastNavigationTime = Time.unscaledTime;
        }
    }

    private void OnHorizontal(InputAction.CallbackContext context)
    {
        if (!isPaused || !inOptionsMenu || !isSliderSelected) return;

        float value = context.ReadValue<float>();

        // Significantly increased slider speed for controller
        float adjustedSpeed = context.control.device is Gamepad ? controllerSliderSpeed : mouseKeyboardSliderSpeed;

        // Add deadzone for analog stick
        if (context.control.device is Gamepad && Mathf.Abs(value) < 0.15f) return;

        // Increased base multiplier for faster response
        float deltaChange = value * adjustedSpeed * 0.01f;  // Increased from 0.005f to 0.01f

        if (currentOptionsIndex == 0)  // Volume slider
        {
            float newValue = Mathf.Clamp01(volumeSlider.value + deltaChange);
            volumeSlider.value = newValue;
            OnVolumeChanged(newValue);
        }
        else if (currentOptionsIndex == 1)  // Sensitivity slider
        {
            float newValue = Mathf.Clamp01(sensitivitySlider.value + deltaChange);
            sensitivitySlider.value = newValue;
            OnSensitivityChanged(newValue);
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isPaused) return;

        if (inOptionsMenu)
        {
            HandleOptionsSelection();
        }
        else
        {
            HandlePauseSelection();
        }
    }

    private void NavigatePauseMenu(bool moveDown)
    {
        if (moveDown)
            currentPauseIndex = (currentPauseIndex + 1) % pauseMenuItems.Length;
        else
            currentPauseIndex = (currentPauseIndex - 1 + pauseMenuItems.Length) % pauseMenuItems.Length;

        UpdatePauseHighlight();
    }

    private void UpdatePauseHighlight()
    {
        if (pauseMenuItems == null || pauseMenuItems.Length == 0) return;
        pauseSelectionHighlight.position = pauseMenuItems[currentPauseIndex].position;
        pauseSelectionHighlight.sizeDelta = pauseMenuItems[currentPauseIndex].sizeDelta;
    }

    private void NavigateOptions(bool moveDown)
    {
        if (moveDown)
            currentOptionsIndex = (currentOptionsIndex + 1) % optionsItems.Length;
        else
            currentOptionsIndex = (currentOptionsIndex - 1 + optionsItems.Length) % optionsItems.Length;

        UpdateOptionsHighlight();
    }

    private void HandlePauseSelection()
    {
        switch (currentPauseIndex)
        {
            case 0: // Resume
                ShowOptionsMenu();
                break;
            case 1: // Main Menu
                Time.timeScale = 1f;
                SceneManager.LoadScene("StartScene");
                break;
        }
    }

    private void HandleOptionsSelection()
    {
        if (!isSliderSelected)
        {
            if (currentOptionsIndex == optionsItems.Length - 1) // Back button
            {
                ShowPauseMenu();
            }
            else if (currentOptionsIndex == 2) // Input toggle
            {
                ToggleInputDisplay();
            }
        }
    }

    private void ShowOptionsMenu()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        inOptionsMenu = true;
        currentOptionsIndex = 0;

        // Force immediate highlight update
        Canvas.ForceUpdateCanvases();
        UpdateOptionsHighlight();
    }

    private void ShowPauseMenu()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        inOptionsMenu = false;
        currentPauseIndex = 0;
        NavigatePauseMenu(false); // Update highlight position
    }

    private void UpdateOptionsHighlight()
    {
        if (optionsItems == null || optionsItems.Length == 0) return;

        if (isSliderSelected)
        {
            optionsSelectionHighlight.sizeDelta = new Vector2(constantHighlightWidth, constantHighlightHeight);
        }
        else
        {
            optionsSelectionHighlight.sizeDelta = optionsItems[currentOptionsIndex].sizeDelta;
        }
        optionsSelectionHighlight.position = optionsItems[currentOptionsIndex].position;
    }

    void SetupEventTriggers()
    {
        // Setup pause menu triggers
        for (int i = 0; i < pauseMenuItems.Length; i++)
        {
            int index = i; // Capture the index for use in lambda expressions
            EventTrigger trigger = pauseMenuItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = pauseMenuItems[i].gameObject.AddComponent<EventTrigger>();
            }

            // Add hover event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((data) => {
                currentPauseIndex = index;
                UpdatePauseHighlight();
            });
            trigger.triggers.Add(enterEntry);

            // Add click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            clickEntry.callback.AddListener((data) => {
                currentPauseIndex = index;
                HandlePauseSelection();
            });
            trigger.triggers.Add(clickEntry);
        }

        // Setup options menu triggers
        for (int i = 0; i < optionsItems.Length; i++)
        {
            int index = i;
            EventTrigger trigger = optionsItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = optionsItems[i].gameObject.AddComponent<EventTrigger>();
            }

            // Add hover event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((data) => {
                currentOptionsIndex = index;
                UpdateOptionsHighlight();
            });
            trigger.triggers.Add(enterEntry);

            // Add click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            clickEntry.callback.AddListener((data) => {
                currentOptionsIndex = index;
                HandleOptionsSelection();
            });
            trigger.triggers.Add(clickEntry);

            // Special handling for sliders
            if (i == 0 || i == 1) // Volume and Sensitivity sliders
            {
                Slider slider = optionsItems[i].GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    // Add EventTrigger to the slider's handle
                    var handleRect = slider.handleRect;
                    if (handleRect != null)
                    {
                        EventTrigger sliderTrigger = handleRect.gameObject.GetComponent<EventTrigger>();
                        if (sliderTrigger == null)
                        {
                            sliderTrigger = handleRect.gameObject.AddComponent<EventTrigger>();
                        }

                        // Add begin drag event
                        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry
                        {
                            eventID = EventTriggerType.BeginDrag
                        };
                        beginDragEntry.callback.AddListener((data) => {
                            isDraggingSlider = true;
                            currentOptionsIndex = index;
                            UpdateOptionsHighlight();
                        });
                        sliderTrigger.triggers.Add(beginDragEntry);

                        // Add end drag event
                        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
                        {
                            eventID = EventTriggerType.EndDrag
                        };
                        endDragEntry.callback.AddListener((data) => {
                            isDraggingSlider = false;
                        });
                        sliderTrigger.triggers.Add(endDragEntry);
                    }
                }
            }
        }
    }


    void OnSensitivityChanged(float sliderValue)
    {
        float actualSensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, sliderValue);
        if (sensitivityText != null)
        {
            sensitivityText.text = $"Mouse Sensitivity: {actualSensitivity:F0}";
        }
        PlayerPrefs.SetFloat("CameraSensitivity", actualSensitivity);
        PlayerPrefs.Save();
        UpdatePlayerSensitivity(actualSensitivity);
    }

    void UpdatePlayerSensitivity(float sensitivity)
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            if (player != null)
            {
                player.SetSensitivity(sensitivity);
            }
        }
    }

    void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(value);
        }
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
    }

    void ToggleInputDisplay()
    {
        showingController = !showingController;
        UpdateInputDisplay();
    }

    void UpdateInputDisplay()
    {
        if (controllerMappingImage != null) controllerMappingImage.gameObject.SetActive(showingController);
        if (keyboardMappingImage != null) keyboardMappingImage.gameObject.SetActive(!showingController);
        if (toggleButtonText != null)
        {
            toggleButtonText.text = showingController ? "Show Keyboard Controls" : "Show Controller Controls";
        }
    }

    private void Resume()
    {
        Debug.Log("Resuming game"); // Debug log
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        inOptionsMenu = false;
    }
}