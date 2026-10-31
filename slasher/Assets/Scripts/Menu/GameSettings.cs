using UnityEngine;
using UnityEngine.Audio;

public static class GameSettings
{
    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 0.7f;
    public static float SfxVolume { get; private set; } = 1f;
    public static int GraphicsQuality { get; private set; } = 2;

    public static Vector2 MovementPosition { get; private set; } = new Vector2(74f, 58f);
    public static Vector2 AimPosition { get; private set; } = new Vector2(-116f, 58f);

    public static float MovementScale { get; private set; } = 1f;
    public static float AimScale { get; private set; } = 1f;

    private static Vector2[] actionButtonPositions = new Vector2[0];

    private static AudioMixer audioMixer;
    private static bool initialized;

    private const string MasterKey = "Settings_MasterVolume";
    private const string MusicKey = "Settings_MusicVolume";
    private const string SfxKey = "Settings_SfxVolume";
    private const string QualityKey = "Settings_GraphicsQuality";

    private const string MovementXKey = "Settings_MovementX";
    private const string MovementYKey = "Settings_MovementY";
    private const string AimXKey = "Settings_AimX";
    private const string AimYKey = "Settings_AimY";
    private const string MovementScaleKey = "Settings_MovementScale";
    private const string AimScaleKey = "Settings_AimScale";
    private const string ActionButtonCountKey = "Settings_ActionButtonCount";

    public static void Load()
    {
        if (initialized)
            return;

        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterKey, 1f));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 0.7f));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 1f));
        GraphicsQuality = Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, 2), 0, 2);

        MovementPosition = new Vector2(
            PlayerPrefs.GetFloat(MovementXKey, 74f),
            PlayerPrefs.GetFloat(MovementYKey, 58f)
        );

        AimPosition = new Vector2(
            PlayerPrefs.GetFloat(AimXKey, -116f),
            PlayerPrefs.GetFloat(AimYKey, 58f)
        );

        MovementScale = Mathf.Clamp(PlayerPrefs.GetFloat(MovementScaleKey, 1f), 0.5f, 2f);
        AimScale = Mathf.Clamp(PlayerPrefs.GetFloat(AimScaleKey, 1f), 0.5f, 2f);

        int count = PlayerPrefs.GetInt(ActionButtonCountKey, 0);
        actionButtonPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            actionButtonPositions[i] = new Vector2(
                PlayerPrefs.GetFloat($"Settings_ActionButton_{i}_X"),
                PlayerPrefs.GetFloat($"Settings_ActionButton_{i}_Y")
            );
        }

        ApplyGraphicsQuality();
        ApplyAudio();

        initialized = true;
    }

    public static void SetAudioMixer(AudioMixer mixer)
    {
        audioMixer = mixer;
        Load();
        ApplyAudio();
    }

    public static void SetMasterVolume(float value)
    {
        Load();
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        ApplyAudio();
        Save();
    }

    public static void SetMusicVolume(float value)
    {
        Load();
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        ApplyAudio();
        Save();
    }

    public static void SetSfxVolume(float value)
    {
        Load();
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        ApplyAudio();
        Save();
    }

    private static void ApplyAudio()
    {
        SetMixerVolume("MasterVolume", MasterVolume);
        SetMixerVolume("MusicVolume", MusicVolume);
        SetMixerVolume("SfxVolume", SfxVolume);
    }

    private static void SetMixerVolume(string parameter, float value)
    {
        if (audioMixer == null)
            return;

        float decibels = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameter, decibels);
    }

    public static void SetGraphicsQuality(int value)
    {
        Load();
        GraphicsQuality = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        ApplyGraphicsQuality();
        PlayerPrefs.SetInt(QualityKey, GraphicsQuality);
        Save();
    }

    private static void ApplyGraphicsQuality()
    {
        if (QualitySettings.names.Length == 0)
            return;

        GraphicsQuality = Mathf.Clamp(GraphicsQuality, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(GraphicsQuality, true);
    }

    public static Vector2 GetActionButtonPosition(int index, Vector2 defaultPosition)
    {
        Load();

        if (index < 0 || index >= actionButtonPositions.Length)
            return defaultPosition;

        return actionButtonPositions[index];
    }

    public static void SetMobileLayout(Vector2 movementPosition, Vector2 aimPosition, float movementScale, float aimScale, Vector2[] buttonPositions)
    {
        Load();

        MovementPosition = movementPosition;
        AimPosition = aimPosition;
        MovementScale = Mathf.Clamp(movementScale, 0.5f, 2f);
        AimScale = Mathf.Clamp(aimScale, 0.5f, 2f);

        actionButtonPositions = buttonPositions != null ? (Vector2[])buttonPositions.Clone() : new Vector2[0];

        PlayerPrefs.SetFloat(MovementXKey, MovementPosition.x);
        PlayerPrefs.SetFloat(MovementYKey, MovementPosition.y);
        PlayerPrefs.SetFloat(AimXKey, AimPosition.x);
        PlayerPrefs.SetFloat(AimYKey, AimPosition.y);
        PlayerPrefs.SetFloat(MovementScaleKey, MovementScale);
        PlayerPrefs.SetFloat(AimScaleKey, AimScale);

        PlayerPrefs.SetInt(ActionButtonCountKey, actionButtonPositions.Length);

        for (int i = 0; i < actionButtonPositions.Length; i++)
        {
            PlayerPrefs.SetFloat($"Settings_ActionButton_{i}_X", actionButtonPositions[i].x);
            PlayerPrefs.SetFloat($"Settings_ActionButton_{i}_Y", actionButtonPositions[i].y);
        }

        Save();
    }

    public static void ResetDefaults()
    {
        PlayerPrefs.DeleteKey(MasterKey);
        PlayerPrefs.DeleteKey(MusicKey);
        PlayerPrefs.DeleteKey(SfxKey);
        PlayerPrefs.DeleteKey(QualityKey);

        PlayerPrefs.DeleteKey(MovementXKey);
        PlayerPrefs.DeleteKey(MovementYKey);
        PlayerPrefs.DeleteKey(AimXKey);
        PlayerPrefs.DeleteKey(AimYKey);
        PlayerPrefs.DeleteKey(MovementScaleKey);
        PlayerPrefs.DeleteKey(AimScaleKey);

        int count = PlayerPrefs.GetInt(ActionButtonCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"Settings_ActionButton_{i}_X");
            PlayerPrefs.DeleteKey($"Settings_ActionButton_{i}_Y");
        }

        PlayerPrefs.DeleteKey(ActionButtonCountKey);

        MasterVolume = 1f;
        MusicVolume = 0.7f;
        SfxVolume = 1f;
        GraphicsQuality = 2;

        MovementPosition = new Vector2(74f, 58f);
        AimPosition = new Vector2(-116f, 58f);
        MovementScale = 1f;
        AimScale = 1f;
        actionButtonPositions = new Vector2[0];

        ApplyGraphicsQuality();
        ApplyAudio();

        if (GameInputManager.Instance != null)
            GameInputManager.Instance.ResetBindings();

        Save();
    }

    private static void Save()
    {
        PlayerPrefs.Save();
    }
}