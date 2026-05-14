using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] float maxTimeinMinutes = 5f;
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    float currentTime;
    string timerText;
    private void Start()
    {
        currentTime = maxTimeinMinutes * 60;
    }
    private void Update()
    {
        timerFormat();
        currentTime -= Time.deltaTime;
        textMeshProUGUI.text = "Time:" + timerText;
        if (currentTime <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    void timerFormat()
    {
        float minutes = Mathf.Floor(currentTime / 60);
        float seconds = Mathf.Floor(currentTime % 60);
        timerText = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
