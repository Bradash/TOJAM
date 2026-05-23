using UnityEngine;

[DefaultExecutionOrder(-27)]
public class GameDataGetter : MonoBehaviour
{
    [SerializeField] GameObject[] quotaItems;
    [SerializeField] GameObject[] npcs;
    [SerializeField] Difficulty difficulty;
    LocationNamer[] ailes;
    private void Start()
    {
        if (GameData.difficulty == null)
        {
            GameData.difficulty = difficulty;
        }
        ailes = FindObjectsOfType<LocationNamer>();
        for (int i = 0; i < GameData.difficulty.quotaAmount; i++)
        {
            quotaItems[i].SetActive(true);
        }
        for (int i = 0; i < npcs.Length; i++)
        {
            npcs[i].SetActive(i < GameData.difficulty.npcAmount);
        }
    }
}
