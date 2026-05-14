using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds UI controls to <see cref="GameSettings"/>. Drop on the Settings panel
/// root and assign the references you actually use — every field is optional.
/// Re-enabling the panel re-reads the latest stored values, so this component
/// is safe to reuse in both the main menu and the in-game pause menu.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("Mouse")]
    public Slider   sensitivitySlider;
    public TMP_Text sensitivityValueText;
    public Toggle   invertYToggle;

    [Header("Audio")]
    public Slider   masterVolumeSlider;
    public TMP_Text masterVolumeValueText;
    public Slider   musicVolumeSlider;
    public TMP_Text musicVolumeValueText;
    public Slider   sfxVolumeSlider;
    public TMP_Text sfxVolumeValueText;

    [Header("Display")]
    public Toggle        fullscreenToggle;
    public TMP_Dropdown  resolutionDropdown;

    [Header("Actions")]
    public Button resetButton;

    List<Vector2Int> _resolutions;

    void OnEnable()
    {
        BindSensitivity();
        BindInvertY();
        BindVolume(masterVolumeSlider, masterVolumeValueText, () => GameSettings.MasterVolume, v => GameSettings.MasterVolume = v);
        BindVolume(musicVolumeSlider,  musicVolumeValueText,  () => GameSettings.MusicVolume,  v => GameSettings.MusicVolume  = v);
        BindVolume(sfxVolumeSlider,    sfxVolumeValueText,    () => GameSettings.SfxVolume,    v => GameSettings.SfxVolume    = v);
        BindFullscreen();
        BindResolution();
        BindReset();
    }

    void OnDisable()
    {
        if (sensitivitySlider  != null) sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        if (invertYToggle      != null) invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (musicVolumeSlider  != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider    != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (resetButton        != null) resetButton.onClick.RemoveListener(OnResetClicked);
    }

    // ── Sensitivity ──

    void BindSensitivity()
    {
        if (sensitivitySlider == null) return;
        sensitivitySlider.minValue = GameSettings.MinMouseSensitivity;
        sensitivitySlider.maxValue = GameSettings.MaxMouseSensitivity;
        sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        UpdateLabel(sensitivityValueText, sensitivitySlider.value, "0.0");
    }

    void OnSensitivityChanged(float v)
    {
        GameSettings.MouseSensitivity = v;
        UpdateLabel(sensitivityValueText, v, "0.0");
    }

    // ── Invert Y ──

    void BindInvertY()
    {
        if (invertYToggle == null) return;
        invertYToggle.SetIsOnWithoutNotify(GameSettings.InvertY);
        invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
    }

    void OnInvertYChanged(bool v) => GameSettings.InvertY = v;

    // ── Volume (generic) ──

    void BindVolume(Slider slider, TMP_Text label, System.Func<float> get, System.Action<float> set)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(get());
        slider.onValueChanged.AddListener(v =>
        {
            set(v);
            UpdateVolumeLabel(label, v);
        });
        UpdateVolumeLabel(label, slider.value);
    }

    // ── Fullscreen ──

    void BindFullscreen()
    {
        if (fullscreenToggle == null) return;
        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    void OnFullscreenChanged(bool v) => GameSettings.Fullscreen = v;

    // ── Resolution ──

    void BindResolution()
    {
        if (resolutionDropdown == null) return;

        _resolutions = GameSettings.GetUniqueResolutions();
        var options = new List<TMP_Dropdown.OptionData>(_resolutions.Count);
        Vector2Int current = GameSettings.Resolution;
        int currentIndex = 0;

        for (int i = 0; i < _resolutions.Count; i++)
        {
            Vector2Int r = _resolutions[i];
            options.Add(new TMP_Dropdown.OptionData($"{r.x} x {r.y}"));
            if (r.x == current.x && r.y == current.y) currentIndex = i;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    void OnResolutionChanged(int index)
    {
        if (_resolutions == null || index < 0 || index >= _resolutions.Count) return;
        GameSettings.Resolution = _resolutions[index];
    }

    // ── Reset ──

    void BindReset()
    {
        if (resetButton == null) return;
        resetButton.onClick.AddListener(OnResetClicked);
    }

    void OnResetClicked()
    {
        GameSettings.ResetToDefaults();
        // Re-bind everything so the UI reflects the freshly cleared values.
        OnDisable();
        OnEnable();
    }

    // ── Labels ──

    static void UpdateLabel(TMP_Text label, float v, string fmt)
    {
        if (label != null) label.text = v.ToString(fmt);
    }

    static void UpdateVolumeLabel(TMP_Text label, float v)
    {
        if (label != null) label.text = $"{Mathf.RoundToInt(v * 100f)}%";
    }
}
