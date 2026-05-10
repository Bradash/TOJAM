using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop this on a UI Button GameObject — clicking the button quits the game.
/// In the editor, exits play mode instead.
/// </summary>
[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Quit);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
