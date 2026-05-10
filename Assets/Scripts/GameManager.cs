using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks win/lose state for the run.
/// Win:  the player completes <see cref="swapsToWin"/> store swaps.
/// Lose: the player is caught <see cref="maxCatches"/> times.
///
/// Listens to PlayerRespawn.OnRespawn (catches) and ItemDisplay.OnStoreSwapCompleted (swaps).
/// Pauses the scene via Time.timeScale = 0 and unlocks the cursor on game over.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Loss Condition")]
    [Tooltip("How many times the player can be caught before they lose.")]
    public int maxCatches = 3;

    [Header("Win Condition")]
    [Tooltip("How many successful store swaps are needed to win.")]
    public int swapsToWin = 5;

    [Header("UI (all optional)")]
    [Tooltip("Panel shown when the player wins. Should start disabled in the scene.")]
    public GameObject winPanel;
    [Tooltip("Panel shown when the player loses. Should start disabled in the scene.")]
    public GameObject losePanel;
    [Tooltip("Live counter for catches, e.g. 'Caught: 1 / 3'.")]
    public TMP_Text catchCounterText;
    [Tooltip("Live counter for swaps, e.g. 'Swaps: 2 / 5'.")]
    public TMP_Text swapCounterText;

    public int  CatchCount { get; private set; }
    public int  SwapCount  { get; private set; }
    public bool GameOver   { get; private set; }

    public event Action OnWin;
    public event Action OnLose;

    PlayerRespawn _playerRespawn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _playerRespawn = FindAnyObjectByType<PlayerRespawn>();
        if (_playerRespawn != null)
            _playerRespawn.OnRespawn += HandleCaught;
        else
            Debug.LogWarning("[GameManager] No PlayerRespawn found — catch tracking disabled.");

        ItemDisplay.OnStoreSwapCompleted += HandleSwap;

        if (winPanel)  winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        UpdateUI();
    }

    void OnDestroy()
    {
        if (_playerRespawn != null) _playerRespawn.OnRespawn -= HandleCaught;
        ItemDisplay.OnStoreSwapCompleted -= HandleSwap;
        if (Instance == this) Instance = null;
    }

    void HandleCaught()
    {
        if (GameOver) return;
        CatchCount++;
        UpdateUI();
        if (CatchCount >= maxCatches) Lose();
    }

    void HandleSwap(ItemDisplay display)
    {
        if (GameOver) return;
        SwapCount++;
        UpdateUI();
        if (SwapCount >= swapsToWin) Win();
    }

    void Win()
    {
        GameOver = true;
        if (winPanel) winPanel.SetActive(true);
        FreezeAndUnlockCursor();
        OnWin?.Invoke();
    }

    void Lose()
    {
        GameOver = true;
        if (losePanel) losePanel.SetActive(true);
        FreezeAndUnlockCursor();
        OnLose?.Invoke();
    }

    static void FreezeAndUnlockCursor()
    {
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    /// <summary>Wire this to a Restart button's OnClick.</summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (catchCounterText) catchCounterText.text = $"Caught: {CatchCount} / {maxCatches}";
        if (swapCounterText)  swapCounterText.text  = $"Quota {SwapCount}/{swapsToWin}";
    }
}
