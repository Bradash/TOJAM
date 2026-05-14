using UnityEngine;

[CreateAssetMenu(fileName = "NewsOutlet", menuName = "Scriptable Objects/NewsOutlet")]
public class NewsOutlet : ScriptableObject
{
    public string[] outletText;
    public Texture[] outletImg;
    public AudioClip[] dialogueAudio;

}
