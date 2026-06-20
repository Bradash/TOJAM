using UnityEngine;
public enum AudioSet { Chase, Wander, Grab, Throw }
public class EnemySound : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] AudioSource playerAudioSource;
    [SerializeField] VoiceLine[] voiceLineChase;
    [SerializeField] VoiceLine[] voiceLineWander;
    [SerializeField] VoiceLine[] voiceLineGrab;
    [SerializeField] VoiceLine[] voiceLineThrow;

    bool isPlayed;

    [SerializeField] private AudioClip grabSFX;
    [SerializeField] private AudioClip ThrowSFX;

    Subtitles subtitles;

    [System.Serializable] public class VoiceLine
    {
        public string subtitleSet;
        public AudioClip voiceSet;
    }


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        subtitles = FindFirstObjectByType<Subtitles>();
    }

    public void playVoiceLine(AudioSet set)
    {
        VoiceLine voiceLine = null;
switch (set)
        {
            case AudioSet.Chase:
                if (audioSource.isPlaying) return;
                voiceLine = voiceLineChase[Random.Range(0, voiceLineChase.Length)];
                break;
            case AudioSet.Wander:
                voiceLine = voiceLineWander[Random.Range(0, voiceLineWander.Length)];
                break;
            case AudioSet.Grab:
                voiceLine = voiceLineGrab[Random.Range(0, voiceLineGrab.Length)];
                playerAudioSource.PlayOneShot(grabSFX);
                break;
            case AudioSet.Throw:
                voiceLine = voiceLineThrow[Random.Range(0, voiceLineThrow.Length)];
                playerAudioSource.PlayOneShot(ThrowSFX);
                break;
        }
        if (voiceLine != null)
        {
            audioSource.PlayOneShot(voiceLine.voiceSet);
            subtitles.SetSubtitle(voiceLine.subtitleSet);
            isPlayed = true;
        }
    }
    private void Update()
    {
        if (isPlayed)
        {
            if(!audioSource.isPlaying && (playerAudioSource == null || !playerAudioSource.isPlaying))
            {
                subtitles.SetSubtitle("");
                isPlayed = false;
            }  
        }
    }
}
