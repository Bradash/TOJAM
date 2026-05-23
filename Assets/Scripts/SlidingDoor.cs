using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    Transform playerPos;
    [SerializeField] float openDistance = 3f;
    [SerializeField] Animator animator;
    AudioSource audioSource;
    [SerializeField] AudioClip[] doorSounds;
    [SerializeField] AudioClip[] closeSounds;
    bool isSet = false;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        if (Vector3.Distance(transform.position, playerPos.position) < openDistance && !isSet)
        {
            animator.SetBool("isOpen", true);
            audioSource.PlayOneShot(doorSounds[Random.Range(0, doorSounds.Length)]);
            isSet = true;
        }
        else if (isSet && Vector3.Distance(transform.position, playerPos.position) >= openDistance)
        {
            animator.SetBool("isOpen", false);
            audioSource.PlayOneShot(closeSounds[Random.Range(0, closeSounds.Length)]);
            isSet = false;
        }
    }
}
