using UnityEngine;

public class NewsDebugger : MonoBehaviour
{
    [SerializeField] int currentLevel;
    private void Awake()
    {
        GameData.currentLevel = currentLevel;
    }
}
