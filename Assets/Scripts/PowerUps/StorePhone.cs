using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StorePhone : MonoBehaviour
{
    public Transform answerPoint;
    public AudioSource ringSource;
    public Renderer ringIndicator;
    private readonly PhoneCallBudget budget = new PhoneCallBudget();
    private EnemyAI clerk;
    private bool purchased;
    private float ringDeadline;
    private float ringTimeout = 20;
    private AudioClip generatedRing;
    private MaterialPropertyBlock indicatorBlock;
    public event Action Changed;
    public int CallsRemaining => budget.Remaining;
    public bool NumberBlocked => budget.Blocked;
    public bool Busy => budget.Busy;
    public string Status { get; private set; } = "Ready";

    public void Configure(bool enabledForAttempt, PowerUpCatalog settings)
    {
        purchased = enabledForAttempt;
        if (settings != null) ringTimeout = settings.phoneRingTimeout;
        if (ringSource == null) ringSource = gameObject.AddComponent<AudioSource>();
        ringSource.playOnAwake = false;
        ringSource.loop = true;
        ringSource.spatialBlend = 1;
        ringSource.minDistance = 2;
        ringSource.maxDistance = 35;
        if (ringSource.clip == null)
        {
            const int sampleRate = 22050;
            float[] samples = new float[sampleRate * 2];
            for (int i = 0; i < sampleRate; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Min(1, t * 100) * Mathf.Min(1, (1 - t) * 100);
                samples[i] = (Mathf.Sin(2 * Mathf.PI * 440 * t) + Mathf.Sin(2 * Mathf.PI * 480 * t)) * 0.12f * envelope;
            }
            generatedRing = AudioClip.Create("Store phone ring", samples.Length, 1, sampleRate, false);
            generatedRing.SetData(samples, 0);
            ringSource.clip = generatedRing;
        }
        Refresh();
    }

    public bool TryCall(out string message)
    {
        if (!purchased) { message = "Phone Distraction is not equipped."; return false; }
        if (budget.Blocked) { message = "This location has blocked your number."; return false; }
        if (budget.Busy) { message = "The phone line is busy."; return false; }
        if (Time.timeScale <= 0 || (GameManager.Instance != null && GameManager.Instance.GameOver))
        { message = "Calls are unavailable right now."; return false; }

        EnemyAI[] candidates = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        Array.Sort(candidates, (a, b) => (a.transform.position - transform.position).sqrMagnitude
            .CompareTo((b.transform.position - transform.position).sqrMagnitude));
        if (!budget.TryRing()) { message = "The phone line is unavailable."; return false; }
        ringDeadline = Time.time + Mathf.Max(1, ringTimeout);
        foreach (EnemyAI candidate in candidates)
        {
            if (candidate.gameObject.scene != gameObject.scene || !candidate.CanAnswerPhone) continue;
            clerk = candidate;
            if (!candidate.TryAnswerPhone(this)) { clerk = null; continue; }
            Status = "Ringing...";
            if (ringSource != null)
            {
                ringSource.volume = GameSettings.SfxVolume;
                ringSource.Play();
            }
            message = Status;
            Refresh();
            return true;
        }
        clerk = null;
        budget.EndCall();
        message = "No staff can reach the phone right now. No call used.";
        Status = message;
        Refresh();
        return false;
    }

    public bool HasCallFor(EnemyAI candidate) => candidate != null && clerk == candidate && budget.Busy;

    public bool TryAnswer(EnemyAI candidate)
    {
        if (candidate == null || clerk != candidate || !budget.TryAnswer()) return false;
        if (ringSource != null) ringSource.Stop();
        Status = budget.Blocked ? "Third call answered. Number blocked." : $"Call answered. {budget.Remaining} calls left.";
        Refresh();
        return true;
    }

    public void ReleaseClerk(EnemyAI candidate)
    {
        if (clerk != candidate) return;
        bool unanswered = budget.Ringing;
        budget.EndCall();
        clerk = null;
        if (ringSource != null) ringSource.Stop();
        Status = budget.Blocked ? "Number blocked" : unanswered ? "No answer. No call used." : "Ready";
        Refresh();
    }

    private void Update()
    {
        if (ringSource != null && budget.Ringing)
        {
            if (Time.timeScale <= 0 && ringSource.isPlaying) ringSource.Pause();
            else if (Time.timeScale > 0 && !ringSource.isPlaying) ringSource.UnPause();
        }
        if (budget.Busy && (clerk == null || !clerk.isActiveAndEnabled
            || (budget.Ringing && Time.time >= ringDeadline))) ReleaseClerk(clerk);
        if (ringIndicator != null && budget.Ringing) UpdateIndicator();
    }

    private void Refresh()
    {
        UpdateIndicator();
        Changed?.Invoke();
    }

    private void UpdateIndicator()
    {
        if (ringIndicator == null) return;
        if (indicatorBlock == null) indicatorBlock = new MaterialPropertyBlock();
        Color color = budget.Ringing && Mathf.Sin(Time.time * 8) > 0 ? Color.yellow
            : budget.Blocked ? new Color(0.6f, 0.08f, 0.08f) : new Color(0.1f, 0.6f, 0.4f);
        indicatorBlock.SetColor("_BaseColor", color);
        indicatorBlock.SetColor("_Color", color);
        ringIndicator.SetPropertyBlock(indicatorBlock);
    }

    private void OnDisable()
    {
        if (budget.Busy) ReleaseClerk(clerk);
    }

    private void OnDestroy()
    {
        if (generatedRing != null) Destroy(generatedRing);
    }
}
