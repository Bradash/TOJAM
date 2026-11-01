using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    float currentTime;
    string timerText;
    bool expired;
    private void Start()
    {
        currentTime = GameData.difficulty.timerAmount * 60;
    }
    private void Update()
    {
        if (expired || (GameManager.Instance != null && GameManager.Instance.GameOver)) return;
        timerFormat();
        currentTime -= Time.deltaTime;
        textMeshProUGUI.text = "Time:" + timerText;
        if (currentTime <= 0)
        {
            expired = true;
            if (GameManager.Instance != null) GameManager.Instance.TimeExpired();
            else PowerUpShop.Open(SceneManager.GetActiveScene().name);
        }
    }
    void timerFormat()
    {
        float minutes = Mathf.Floor(currentTime / 60);
        float seconds = Mathf.Floor(currentTime % 60);
        timerText = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
