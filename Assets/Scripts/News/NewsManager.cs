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
        newsAnim.SetInteger("currentLevel", GameData.currentLevel);

        newsAudio.clip = newsOutlets[GameData.currentLevel].dialogueAudio[0];
        radioAudio.clip = newsOutlets[GameData.currentLevel].dialogueAudio[1];

        newsTexts[0].text = newsOutlets[GameData.currentLevel].outletText[0];
        newsTexts[1].text = newsOutlets[GameData.currentLevel].outletText[1];

        newsImages[0].texture = newsOutlets[GameData.currentLevel].outletImg[0];
        newsImages[1].texture = newsOutlets[GameData.currentLevel].outletImg[1];
    }
}
