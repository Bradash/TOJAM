using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central place for user-configurable settings. Persists via PlayerPrefs
/// and applies values that affect global state (AudioListener.volume,
/// Screen resolution / fullscreen) at startup and whenever a setter is called.
///
/// Listeners (e.g. MixedAudioSource, FPSController) subscribe to <see cref="Changed"/>
/// to react when any setting changes.
/// </summary>
public static class GameSettings
{
    // ── PlayerPrefs keys ──
    const string KEY_MOUSE_SENSITIVITY = "settings.mouseSensitivity";
    const string KEY_MASTER_VOLUME     = "settings.masterVolume";
    const string KEY_MUSIC_VOLUME      = "settings.musicVolume";
    const string KEY_SFX_VOLUME        = "settings.sfxVolume";
    const string KEY_INVERT_Y          = "settings.invertY";
    const string KEY_FULLSCREEN        = "settings.fullscreen";
    const string KEY_RES_WIDTH         = "settings.resWidth";
    const string KEY_RES_HEIGHT        = "settings.resHeight";

    // ── Defaults ──
    public const float DefaultMouseSensitivity = 2f;
    public const float DefaultMasterVolume     = 1f;
    public const float DefaultMusicVolume      = 1f;
    public const float DefaultSfxVolume        = 1f;
    public const bool  DefaultInvertY          = false;
    public const bool  DefaultFullscreen       = true;

    // ── Ranges ──
    public const float MinMouseSensitivity = 0.1f;
    public const float MaxMouseSensitivity = 10f;

    /// <summary>Fires after any setting value is written. Listeners should refresh from getters.</summary>
    public static event Action Changed;

    // ── Properties ──

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(KEY_MOUSE_SENSITIVITY, DefaultMouseSensitivity);
        set => SetFloat(KEY_MOUSE_SENSITIVITY, Mathf.Clamp(value, MinMouseSensitivity, MaxMouseSensitivity));
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, DefaultMasterVolume);
        set
        {
            float v = Mathf.Clamp01(value);
            SetFloat(KEY_MASTER_VOLUME, v);
            AudioListener.volume = v;
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, DefaultMusicVolume);
        set => SetFloat(KEY_MUSIC_VOLUME, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(KEY_SFX_VOLUME, DefaultSfxVolume);
        set => SetFloat(KEY_SFX_VOLUME, Mathf.Clamp01(value));
    }

    public static bool InvertY
    {
        get => PlayerPrefs.GetInt(KEY_INVERT_Y, DefaultInvertY ? 1 : 0) != 0;
        set => SetInt(KEY_INVERT_Y, value ? 1 : 0);
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(KEY_FULLSCREEN, DefaultFullscreen ? 1 : 0) != 0;
        set
        {
            SetInt(KEY_FULLSCREEN, value ? 1 : 0);
            ApplyScreen();
        }
    }

    public static Vector2Int Resolution
    {
        get
        {
            int w = PlayerPrefs.GetInt(KEY_RES_WIDTH,  Screen.currentResolution.width);
            int h = PlayerPrefs.GetInt(KEY_RES_HEIGHT, Screen.currentResolution.height);
            return new Vector2Int(w, h);
        }
        set
        {
            PlayerPrefs.SetInt(KEY_RES_WIDTH,  value.x);
            PlayerPrefs.SetInt(KEY_RES_HEIGHT, value.y);
            PlayerPrefs.Save();
            ApplyScreen();
            Changed?.Invoke();
        }
    }

    // ── Helpers ──

    static void SetFloat(string key, float v)
    {
        PlayerPrefs.SetFloat(key, v);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    static void SetInt(string key, int v)
    {
        PlayerPrefs.SetInt(key, v);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    /// <summary>Returns the unique (width, height) resolutions supported by the display, sorted.</summary>
    public static List<Vector2Int> GetUniqueResolutions()
    {
        var seen = new HashSet<long>();
        var list = new List<Vector2Int>();
        foreach (var r in Screen.resolutions)
        {
            long key = ((long)r.width << 32) | (uint)r.height;
            if (seen.Add(key))
                list.Add(new Vector2Int(r.width, r.height));
        }
        list.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        return list;
    }

    /// <summary>Wipes all stored values and re-applies global state. Listeners get a single Changed event.</summary>
    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(KEY_MOUSE_SENSITIVITY);
        PlayerPrefs.DeleteKey(KEY_MASTER_VOLUME);
        PlayerPrefs.DeleteKey(KEY_MUSIC_VOLUME);
        PlayerPrefs.DeleteKey(KEY_SFX_VOLUME);
        PlayerPrefs.DeleteKey(KEY_INVERT_Y);
        PlayerPrefs.DeleteKey(KEY_FULLSCREEN);
        PlayerPrefs.DeleteKey(KEY_RES_WIDTH);
        PlayerPrefs.DeleteKey(KEY_RES_HEIGHT);
        PlayerPrefs.Save();

        AudioListener.volume = MasterVolume;
        ApplyScreen();
        Changed?.Invoke();
    }

    static void ApplyScreen()
    {
        Vector2Int res = Resolution;
        FullScreenMode mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(res.x, res.y, mode);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyAtStartup()
    {
        AudioListener.volume = MasterVolume;
        // Resolution / fullscreen: only apply if the player previously set one explicitly
        if (PlayerPrefs.HasKey(KEY_RES_WIDTH) || PlayerPrefs.HasKey(KEY_FULLSCREEN))
            ApplyScreen();
    }
}
