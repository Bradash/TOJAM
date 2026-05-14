using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// First scene of the build. Animated splash:
///   1) fades the splash group in,
///   2) holds for minSplashTime,
///   3) fades in the "Press Any Key" prompt and pulses it,
///   4) on any input, fades everything out and loads the next scene.
///
/// All timing uses unscaled time so it works even if a previous game-over
/// left Time.timeScale at 0 (also defensively reset in Start).
///
/// Persistent settings (audio volume etc.) are applied before this scene
/// runs via GameSettings' [RuntimeInitializeOnLoadMethod].
/// </summary>
public class BootLoader : MonoBehaviour
{
    [Header("Flow")]
    [Tooltip("Scene name to load after the splash. Must be in Build Settings.")]
    public string nextScene = "MainMenu";
    [Tooltip("Seconds the splash must be visible before the prompt arms input.")]
    public float minSplashTime = 1.5f;

    [Header("Auto-Advance (optional)")]
    [Tooltip("If true, advances to the next scene after maxSplashTime even without input.")]
    public bool  autoAdvance   = false;
    public float maxSplashTime = 8f;

    [Header("Splash Fade")]
    [Tooltip("CanvasGroup on the root splash panel — fades the whole splash in and out. " +
             "Add a CanvasGroup component to your splash panel and drag it here.")]
    public CanvasGroup splashGroup;
    public float fadeInDuration  = 0.7f;
    public float fadeOutDuration = 0.5f;

    [Header("Splash Scale Punch")]
    [Tooltip("RectTransform of the splash image — scales from punchFromScale, overshoots past " +
             "its editor scale by punchOvershootScale, then settles. Leave empty to disable.")]
    public RectTransform splashImageTransform;
    [Tooltip("Starting scale, as a fraction of the editor scale. 0.7 = 70% start.")]
    [Range(0.1f, 1f)] public float punchFromScale       = 0.7f;
    [Tooltip("Peak scale, as a fraction of editor scale. 1.15 = 15% overshoot at the peak.")]
    [Range(1f, 1.5f)] public float punchOvershootScale  = 1.15f;
    [Tooltip("How long the scale-up to overshoot takes, as a fraction of fadeInDuration. " +
             "Remainder is spent settling back to 1.")]
    [Range(0.1f, 0.9f)] public float punchPeakAt        = 0.6f;

    [Header("Press Any Key")]
    [Tooltip("The 'Press any key to continue' label. A CanvasGroup is auto-added if missing.")]
    public GameObject pressAnyKeyText;
    [Tooltip("Pulse cycles per second.")]
    public float pulseSpeed = 1.2f;
    [Range(0f, 1f)] public float pulseMinAlpha = 0.25f;
    [Range(0f, 1f)] public float pulseMaxAlpha = 1f;
    [Tooltip("How long the prompt takes to fade in once armed.")]
    public float promptFadeInDuration = 0.35f;

    CanvasGroup _pressAnyKeyGroup;
    bool        _inputArmed;
    bool        _exiting;

    void Start()
    {
        // Defensive resets — Boot is the entry scene, but if something brought
        // us here mid-game, restore sensible global state.
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (pressAnyKeyText != null)
        {
            _pressAnyKeyGroup = pressAnyKeyText.GetComponent<CanvasGroup>();
            if (_pressAnyKeyGroup == null)
                _pressAnyKeyGroup = pressAnyKeyText.AddComponent<CanvasGroup>();
            _pressAnyKeyGroup.alpha = 0f;
            pressAnyKeyText.SetActive(true); // alpha controls visibility now
        }

        if (splashGroup != null)
            splashGroup.alpha = 0f;

        StartCoroutine(RunFlow());
    }

    void Update()
    {
        if (_exiting || !_inputArmed) return;
        if (Input.anyKeyDown) BeginExit();
    }

    IEnumerator RunFlow()
    {
        // 1. Fade the whole splash group in (with a parallel scale punch on the image).
        if (splashImageTransform != null)
            StartCoroutine(ScalePunch(splashImageTransform, punchFromScale, fadeInDuration));
        if (splashGroup != null)
            yield return Fade(splashGroup, 0f, 1f, fadeInDuration);

        // 2. Hold.
        if (minSplashTime > 0f)
            yield return new WaitForSecondsRealtime(minSplashTime);

        // 3. Fade in the prompt.
        if (_pressAnyKeyGroup != null)
            yield return Fade(_pressAnyKeyGroup, 0f, pulseMaxAlpha, promptFadeInDuration);

        _inputArmed = true;

        // 4. Pulse the prompt until the player taps a key.
        float armedAt = Time.unscaledTime;
        while (!_exiting)
        {
            if (_pressAnyKeyGroup != null)
            {
                float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                _pressAnyKeyGroup.alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, wave);
            }

            if (autoAdvance && Time.unscaledTime - armedAt > maxSplashTime)
                BeginExit();

            yield return null;
        }
    }

    void BeginExit()
    {
        if (_exiting) return;
        _exiting = true;
        StartCoroutine(ExitFlow());
    }

    IEnumerator ExitFlow()
    {
        // Fade every group we know about in parallel — robust to any hierarchy.
        CanvasGroup imageCG = splashImageTransform != null
            ? splashImageTransform.GetComponent<CanvasGroup>()
            : null;

        float splashStart = splashGroup       != null ? splashGroup.alpha       : 0f;
        float promptStart = _pressAnyKeyGroup != null ? _pressAnyKeyGroup.alpha : 0f;
        float imageStart  = imageCG           != null ? imageCG.alpha           : 0f;
        float dur = Mathf.Max(0.0001f, fadeOutDuration);

        for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
        {
            float k = t / dur;
            if (splashGroup       != null) splashGroup.alpha       = Mathf.Lerp(splashStart, 0f, k);
            if (_pressAnyKeyGroup != null) _pressAnyKeyGroup.alpha = Mathf.Lerp(promptStart, 0f, k);
            if (imageCG           != null) imageCG.alpha           = Mathf.Lerp(imageStart,  0f, k);
            yield return null;
        }

        if (splashGroup       != null) splashGroup.alpha       = 0f;
        if (_pressAnyKeyGroup != null) _pressAnyKeyGroup.alpha = 0f;
        if (imageCG           != null) imageCG.alpha           = 0f;

        SceneManager.LoadScene(nextScene);
    }

    static IEnumerator Fade(CanvasGroup g, float from, float to, float duration)
    {
        if (duration <= 0f) { g.alpha = to; yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        g.alpha = to;
    }

    IEnumerator ScalePunch(RectTransform target, float fromScale, float duration)
    {
        // Make sure the image has its own CanvasGroup so we can fade it directly,
        // regardless of whether the parent splashGroup is wired correctly.
        var imageCG = target.GetComponent<CanvasGroup>();
        if (imageCG == null) imageCG = target.gameObject.AddComponent<CanvasGroup>();

        // Preserve the editor-set scale (e.g. 4,4,4) and animate relative to it.
        Vector3 baseScale     = target.localScale;
        Vector3 startScale    = baseScale * fromScale;
        Vector3 overshootScale = baseScale * punchOvershootScale;

        target.localScale = startScale;
        imageCG.alpha     = 0f;

        if (duration <= 0f)
        {
            target.localScale = baseScale;
            imageCG.alpha     = 1f;
            yield break;
        }

        float peakT    = Mathf.Clamp(punchPeakAt, 0.05f, 0.95f) * duration;
        float settleT  = duration - peakT;

        // Phase 1 — start → overshoot (ease-out, gives a fast-but-decelerating rise).
        float t = 0f;
        while (t < peakT)
        {
            t += Time.unscaledDeltaTime;
            float k     = Mathf.Clamp01(t / peakT);
            float eased = 1f - (1f - k) * (1f - k); // ease-out quadratic
            target.localScale = Vector3.LerpUnclamped(startScale, overshootScale, eased);
            imageCG.alpha     = Mathf.Clamp01(t / duration);
            yield return null;
        }
        target.localScale = overshootScale;

        // Phase 2 — overshoot → base scale (smoothstep, gentle bounce-back).
        float t2 = 0f;
        while (t2 < settleT)
        {
            t2 += Time.unscaledDeltaTime;
            float k     = Mathf.Clamp01(t2 / settleT);
            float eased = k * k * (3f - 2f * k); // smoothstep
            target.localScale = Vector3.LerpUnclamped(overshootScale, baseScale, eased);
            imageCG.alpha     = Mathf.Clamp01((peakT + t2) / duration);
            yield return null;
        }
        target.localScale = baseScale;
        imageCG.alpha     = 1f;
    }
}
