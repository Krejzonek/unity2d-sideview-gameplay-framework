using UnityEngine;

public static class GameSettings
{
    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 0.7f;
    public static float SfxVolume { get; private set; } = 1f;
    public static int GraphicsQuality { get; private set; } = 2;

    private const string MasterVolumeKey = "Settings_MasterVolume";
    private const string MusicVolumeKey = "Settings_MusicVolume";
    private const string SfxVolumeKey = "Settings_SfxVolume";
    private const string GraphicsQualityKey = "Settings_GraphicsQuality";

    private static bool initialized;

    public static void Load()
    {
        if (initialized)
            return;

        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
        GraphicsQuality = Mathf.Clamp(PlayerPrefs.GetInt(GraphicsQualityKey, 2), 0, 2);

        ApplyGraphicsQuality();
        initialized = true;
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();
    }

    public static void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
    }

    public static void SetGraphicsQuality(int value)
    {
        GraphicsQuality = Mathf.Clamp(value, 0, 2);
        ApplyGraphicsQuality();
        PlayerPrefs.SetInt(GraphicsQualityKey, GraphicsQuality);
        PlayerPrefs.Save();
    }

    private static void ApplyGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(GraphicsQuality, true);
    }
}