using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics")]
    [SerializeField] private Button lowQualityButton;
    [SerializeField] private Button mediumQualityButton;
    [SerializeField] private Button highQualityButton;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    [Header("Reset")]
    [SerializeField] private Button resetButton;
    [SerializeField] private ConfirmationDialog confirmationDialog;

    [Header("Navigation")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject[] mainMenuButtons;

    private void Awake()
    {
        GameSettings.SetAudioMixer(audioMixer);
    }

    private void OnEnable()
    {
        SetMainMenuButtons(false);

        GameSettings.Load();
        RefreshSettings();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (masterVolumeSlider != null && masterVolumeSlider.gameObject.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(masterVolumeSlider.gameObject);
        }
    }

    private void Start()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(GameSettings.SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(GameSettings.SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(GameSettings.SetSfxVolume);

        if (lowQualityButton != null)
            lowQualityButton.onClick.AddListener(SetLowQuality);

        if (mediumQualityButton != null)
            mediumQualityButton.onClick.AddListener(SetMediumQuality);

        if (highQualityButton != null)
            highQualityButton.onClick.AddListener(SetHighQuality);

        if (resetButton != null)
            resetButton.onClick.AddListener(ShowResetConfirmation);

        RefreshSettings();
    }

    private void OnDisable()
    {
        SetMainMenuButtons(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (settingsButton != null && settingsButton.gameObject.activeInHierarchy && settingsButton.interactable)
                EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(GameSettings.SetSfxVolume);

        if (lowQualityButton != null)
            lowQualityButton.onClick.RemoveListener(SetLowQuality);

        if (mediumQualityButton != null)
            mediumQualityButton.onClick.RemoveListener(SetMediumQuality);

        if (highQualityButton != null)
            highQualityButton.onClick.RemoveListener(SetHighQuality);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ShowResetConfirmation);
    }

    private void SetMainMenuButtons(bool active)
    {
        if (mainMenuButtons == null)
            return;

        foreach (GameObject button in mainMenuButtons)
        {
            if (button != null)
                button.SetActive(active);
        }
    }

    private void SetLowQuality()
    {
        GameSettings.SetGraphicsQuality(0);
        UpdateQualityButtons();
    }

    private void SetMediumQuality()
    {
        GameSettings.SetGraphicsQuality(1);
        UpdateQualityButtons();
    }

    private void SetHighQuality()
    {
        GameSettings.SetGraphicsQuality(2);
        UpdateQualityButtons();
    }

    private void ShowResetConfirmation()
    {
        if (confirmationDialog != null)
            confirmationDialog.Show("Are you sure you want to reset all settings to default?", ResetSettings);
    }

    private void ResetSettings()
    {
        GameSettings.ResetDefaults();

        if (GameInputManager.Instance != null)
            GameInputManager.Instance.ResetBindings();

        RefreshSettings();

        MobileControls[] controls = FindObjectsByType<MobileControls>(FindObjectsSortMode.None);

        foreach (MobileControls control in controls)
            control.ApplySettings();

        RebindButton[] rebindButtons = FindObjectsByType<RebindButton>(FindObjectsSortMode.None);

        foreach (RebindButton button in rebindButtons)
            button.Refresh();
    }

    private void RefreshSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);

        UpdateQualityButtons();
    }

    private void UpdateQualityButtons()
    {
        UpdateQualityButton(lowQualityButton, 0);
        UpdateQualityButton(mediumQualityButton, 1);
        UpdateQualityButton(highQualityButton, 2);
    }

    private void UpdateQualityButton(Button button, int quality)
    {
        if (button == null)
            return;

        button.interactable = true;
        button.transition = Selectable.Transition.None;

        if (button.targetGraphic != null)
            button.targetGraphic.color = GameSettings.GraphicsQuality == quality ? selectedColor : normalColor;
    }
}