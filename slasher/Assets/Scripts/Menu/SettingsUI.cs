using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button lowQualityButton;
    [SerializeField] private Button mediumQualityButton;
    [SerializeField] private Button highQualityButton;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    private void OnEnable()
    {
        GameSettings.Load();
        RefreshSettings();
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

        RefreshSettings();
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