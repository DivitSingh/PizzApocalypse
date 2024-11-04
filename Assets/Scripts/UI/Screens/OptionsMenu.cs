using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
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

    [Header("Layout")]
    public RectTransform[] selectableItems;
    public RectTransform selectionHighlight;
    public float constantHighlightHeight = 60f;
    public float constantHighlightWidth = 300f;
    public float sliderSpeed = 1f;

    private int currentIndex = 0;
    private bool hasInitialized = false;
    private bool isSliderSelected => currentIndex == 0 || currentIndex == 1;
    private bool isDraggingSlider = false;
    private bool showingController = true;
    private bool wasVerticalPressed = false;
    private bool wasHorizontalPressed = false;
    private float horizontalInputTimer = 0f;
    private const float INPUT_DELAY = 0.2f;

    void Awake()
    {
        // Force all layouts to update immediately
        Canvas.ForceUpdateCanvases();
        foreach (RectTransform item in selectableItems)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }

        // Set initial position
        currentIndex = 0;
        UpdateSelectionHighlight();
    }

    void Start()
    {
        // Initialize volume
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // Set up sensitivity slider range
        sensitivitySlider.minValue = 0f;  // We'll use 0-1 range and interpolate
        sensitivitySlider.maxValue = 1f;
        sensitivitySlider.wholeNumbers = false;

        // Initialize sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", defaultSensitivity);
        // Convert actual sensitivity to slider value (0-1 range)
        float sliderValue = Mathf.InverseLerp(minSensitivity, maxSensitivity, savedSensitivity);
        sensitivitySlider.value = sliderValue;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        UpdatePlayerSensitivity(savedSensitivity);

        if (inputToggleButton != null)
        {
            inputToggleButton.onClick.AddListener(ToggleInputDisplay);
        }

        UpdateInputDisplay();

        // Force another update after all components are initialized
        Canvas.ForceUpdateCanvases();
        UpdateSelectionHighlight();
        hasInitialized = true;

        SetupEventTriggers();
    }

    void LateUpdate()
    {
        if (!hasInitialized)
        {
            UpdateSelectionHighlight();
            hasInitialized = true;
        }

        HandleNavigationInput();
        HandleSliderInput();
        HandleSelectionInput();
    }

    void InitializeUI()
    {
        Canvas.ForceUpdateCanvases();

        // Initialize volume
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // Initialize sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 100f);
        sensitivitySlider.value = savedSensitivity;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        if (inputToggleButton != null)
        {
            inputToggleButton.onClick.AddListener(ToggleInputDisplay);
        }

        UpdateInputDisplay();
    }

    void SetupEventTriggers()
    {
        for (int i = 0; i < selectableItems.Length; i++)
        {
            int index = i;
            EventTrigger trigger = selectableItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = selectableItems[i].gameObject.AddComponent<EventTrigger>();
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
                HandleSelection();
            });
            trigger.triggers.Add(clickEntry);
        }
    }

    void HandleNavigationInput()
    {
        if (isDraggingSlider) return;

        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetAxisRaw("Vertical") > 0.5f && !wasVerticalPressed);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetAxisRaw("Vertical") < -0.5f && !wasVerticalPressed);
        wasVerticalPressed = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f;

        if (downPressed)
        {
            currentIndex = (currentIndex + 1) % selectableItems.Length;
            UpdateSelectionHighlight();
        }
        else if (upPressed)
        {
            currentIndex = (currentIndex - 1 + selectableItems.Length) % selectableItems.Length;
            UpdateSelectionHighlight();
        }
    }

    void HandleSliderInput()
    {
        if (!isSliderSelected) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool isPressed = Mathf.Abs(horizontalInput) > 0.5f;

        if (isPressed)
        {
            if (!wasHorizontalPressed || horizontalInputTimer <= 0)
            {
                // Increased base slider speed multiplier
                float deltaChange = Mathf.Sign(horizontalInput) * (sliderSpeed * 10f * Time.deltaTime);

                if (currentIndex == 0)
                {
                    // Volume slider
                    volumeSlider.value += deltaChange * 4f;
                }
                else if (currentIndex == 1)
                {
                    // Sensitivity slider
                    float sensitivityDelta = deltaChange * 4f; 
                    float newValue = Mathf.Clamp01(sensitivitySlider.value + sensitivityDelta);
                    sensitivitySlider.value = newValue;
                }

                horizontalInputTimer = INPUT_DELAY;
            }
            horizontalInputTimer -= Time.deltaTime;
        }
        else
        {
            horizontalInputTimer = 0;
        }

        wasHorizontalPressed = isPressed;
    }

    void HandleSelection()
    {
        if (!isSliderSelected)
        {
            if (currentIndex == 3)
                BackToMainMenu();
            else if (currentIndex == 2)
                ToggleInputDisplay();
        }
    }

    void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
        {
            HandleSelection();
        }
    }

    void OnSensitivityChanged(float sliderValue)
    {
        // Convert slider value (0-1) to actual sensitivity value
        float actualSensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, sliderValue);

        // Update the sensitivity text if you have one
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
        if (controllerMappingImage != null)
        {
            controllerMappingImage.gameObject.SetActive(showingController);
        }

        if (keyboardMappingImage != null)
        {
            keyboardMappingImage.gameObject.SetActive(!showingController);
        }

        if (toggleButtonText != null)
        {
            toggleButtonText.text = showingController ? "Show Keyboard Controls" : "Show Controller Controls";
        }
    }

    void UpdateSelectionHighlight()
    {
        if (selectableItems == null || selectableItems.Length == 0 || selectionHighlight == null) return;

        // Force layout update on the current item
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectableItems[currentIndex]);

        // Set position and force immediate update
        selectionHighlight.position = selectableItems[currentIndex].position;
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectionHighlight);

        // Set size
        if (isSliderSelected)
        {
            selectionHighlight.sizeDelta = new Vector2(constantHighlightWidth, constantHighlightHeight);
        }
        else
        {
            selectionHighlight.sizeDelta = selectableItems[currentIndex].sizeDelta;
        }

        // Force final update
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectionHighlight);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("StartScene");
    }
}