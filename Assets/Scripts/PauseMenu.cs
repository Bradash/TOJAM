using UnityEngine;

/// <summary>
/// In-game pause controller. Place this on the pause panel root (or any
/// always-active GameObject) and assign the panel. Press <see cref="pauseKey"/>
/// to toggle.
///
/// Pausing sets Time.timeScale = 0 (the existing FPSController and
/// ItemInteraction scripts already gate on this) and unlocks the cursor.
/// While the game is over (GameManager.GameOver == true), pause input is
/// ignored so the win/lose panel can stay in control.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("Panel shown when paused. Should start inactive.")]
    public GameObject pausePanel;
    [Tooltip("Optional Settings sub-panel that opens from the pause menu. Will be closed when un-pausing.")]
    public GameObject settingsSubPanel;
    [Tooltip("Key that toggles pause.")]
    public KeyCode pauseKey = KeyCode.Escape;

    bool _paused;
    public bool IsPaused => _paused;

    void Start()
    {
        if (pausePanel)       pausePanel.SetActive(false);
        if (settingsSubPanel) settingsSubPanel.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;
        if (GameManager.Instance != null && GameManager.Instance.GameOver) return;

        if (_paused) Resume();
        else         Pause();
    }

    public void Pause()
    {
        _paused = true;
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    /// <summary>Called by the Resume button or by pressing pauseKey again.</summary>
    public void Resume()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameOver) return;

        _paused = false;
        if (pausePanel)       pausePanel.SetActive(false);
        if (settingsSubPanel) settingsSubPanel.SetActive(false);
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}
