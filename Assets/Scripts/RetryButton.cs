using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drop this on a UI Button GameObject — clicking the button reloads the
/// current scene. No inspector wiring required; the listener is added on Awake.
/// </summary>
[RequireComponent(typeof(Button))]
public class RetryButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Retry);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
