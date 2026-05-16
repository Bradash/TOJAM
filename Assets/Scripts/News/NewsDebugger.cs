using UnityEngine;

public class NewsDebugger : MonoBehaviour
{
    [SerializeField] int currentLevel;
    void Start()
    {
        GameData.currentLevel = currentLevel;
    }

}
