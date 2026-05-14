using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewsManager : MonoBehaviour
{
    [Header("News Outlets")]
    [SerializeField] NewsOutlet[] newsOutlets;
    [Header("Settings")]
    public int currentLevel;
    [SerializeField] Animator newsAnim;
    [SerializeField] TextMeshProUGUI[] newsTexts;
    [SerializeField] RawImage[] newsImages;
    [SerializeField] AudioSource newsAudio;
    [SerializeField] AudioSource radioAudio;
    private void Start()
    {
        setNews();
    }
    private void Update()
    {
        AnimatorStateInfo animStateInfo = newsAnim.GetCurrentAnimatorStateInfo(0);
        float NTime = animStateInfo.normalizedTime;
        if (NTime >= 0.99f)
        {
            endNews();
        }
    }
    public void endNews()
    {
        SceneManager.LoadScene("MainMenu");
    }
    void setNews()
    {
        newsAnim.SetInteger("currentLevel", currentLevel);

        newsAudio.clip = newsOutlets[currentLevel].dialogueAudio[0];
        radioAudio.clip = newsOutlets[currentLevel].dialogueAudio[1];

        newsTexts[0].text = newsOutlets[currentLevel].outletText[0];
        newsTexts[1].text = newsOutlets[currentLevel].outletText[1];

        newsImages[0].texture = newsOutlets[currentLevel].outletImg[0];
        newsImages[1].texture = newsOutlets[currentLevel].outletImg[1];
    }
}
