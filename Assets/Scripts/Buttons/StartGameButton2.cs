using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drop this on a UI Button GameObject — clicking the button loads the
/// scene named in <see cref="sceneToLoad"/>. Make sure that scene is added
/// to Build Settings (File → Build Settings…).
/// </summary>
[RequireComponent(typeof(Button))]
public class StartGameButton2 : MonoBehaviour
{
    [Tooltip("Name of the scene to load. Must be in Build Settings.")]
    public string[] scenesToLoad;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        if (scenesToLoad == null || GameData.currentLevel < 0 || GameData.currentLevel >= scenesToLoad.Length)
        {
            Debug.LogError("No scene is configured for the selected level.", this);
            return;
        }
        PowerUpShop.Open(scenesToLoad[GameData.currentLevel]);
    }
}
