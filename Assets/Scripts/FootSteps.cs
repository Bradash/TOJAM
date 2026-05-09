using UnityEngine;

public class FootSteps : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip[] audioClips;
    public bool isWalking = false;
    private void Awake()
    {
       audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isWalking)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.Play();
            }
        }
        else
        {
            audioSource.Stop();
        }
    }






}
