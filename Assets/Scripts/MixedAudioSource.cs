using UnityEngine;

/// <summary>
/// Tags an AudioSource as Music or SFX and keeps its volume in sync with
/// GameSettings.MusicVolume / GameSettings.SfxVolume.
///
/// Master volume is applied globally via AudioListener.volume, so the final
/// audible volume is: baseVolume * categoryVolume * MasterVolume.
///
/// Drop this on any GameObject that has an AudioSource. The component
/// captures the inspector-set volume on Awake as the "base" — your
/// per-source mix is preserved.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MixedAudioSource : MonoBehaviour
{
    public enum Category { Sfx, Music }

    [Tooltip("Routing category. SFX = footsteps, doors, voice, effects. Music = background tracks.")]
    public Category category = Category.Sfx;

    AudioSource _source;
    float       _baseVolume;

    void Awake()
    {
        _source     = GetComponent<AudioSource>();
        _baseVolume = _source.volume;
    }

    void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    void OnDisable()
    {
        GameSettings.Changed -= Apply;
    }

    void Apply()
    {
        float catVol = category == Category.Music ? GameSettings.MusicVolume : GameSettings.SfxVolume;
        _source.volume = _baseVolume * catVol;
    }
}
