using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Returns directly to the main menu. Binds the button on Awake.</summary>
[RequireComponent(typeof(Button))]
public class ReturnMainMenu : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(GoToMainMenu);
    }

    public void GoToMainMenu()
    {
        PowerUpProgress.ClearPendingPurchase();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    // Preserve existing Inspector events that use the previous method name.
    public void Retry() => GoToMainMenu();
}
