using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class OptionsMenu : MonoBehaviour
{
    public Slider volumeSlider;
    public RectTransform[] selectableItems;
    public RectTransform selectionHighlight;
    public float constantHighlightHeight = 60f;
    public float constantHighlightWidth = 300f;
    public float sliderSpeed = 0.1f;
    private int currentIndex = 0;
    private bool hasInitialized = false;
    private bool isSliderSelected => currentIndex == 0;

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        currentIndex = 0;
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

        if (isSliderSelected)
        {
            float horizontalInput = Input.GetKey(KeyCode.RightArrow) ? 1 : (Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetAxisRaw("Horizontal"));
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                volumeSlider.value += horizontalInput * sliderSpeed * Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
        {
            if (!isSliderSelected)
            {
                BackToMainMenu();
            }
        }
    }

    private bool wasVerticalPressed = false;

    void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(value);
        }
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
    }

    void UpdateSelectionHighlight()
    {
        if (selectableItems == null || selectableItems.Length == 0) return;
        selectionHighlight.position = selectableItems[currentIndex].position;
        if (isSliderSelected)
        {
            selectionHighlight.sizeDelta = new Vector2(constantHighlightWidth, constantHighlightHeight);
        }
        else
        {
            selectionHighlight.sizeDelta = selectableItems[currentIndex].sizeDelta;
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("StartScene");
    }
}