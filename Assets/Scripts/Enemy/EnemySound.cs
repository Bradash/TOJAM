using UnityEngine;

public class EnemySound : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] foundClips;
    [SerializeField] private AudioClip[] pickupClips;
    [SerializeField] private AudioClip[] suspiciousClips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void foundSound()
    {
        if (foundClips.Length == 0)
        {
            Debug.LogWarning("No enemy clips assigned.");
            return;
        }
        int randomIndex = Random.Range(0, foundClips.Length);
        audioSource.PlayOneShot(foundClips[randomIndex]);
    }
    public void pickupSound()
    {
        if (pickupClips.Length == 0)
        {
            Debug.LogWarning("No pickup clips assigned.");
            return;
        }
        int randomIndex = Random.Range(0, pickupClips.Length);
        audioSource.PlayOneShot(pickupClips[randomIndex]);
    }
    public void suspiciousSound()
    {
        if (suspiciousClips.Length == 0)
        {
            Debug.LogWarning("No suspicious clips assigned.");
            return;
        }
        int randomIndex = Random.Range(0, suspiciousClips.Length);
        audioSource.PlayOneShot(suspiciousClips[randomIndex]);
    }
}
