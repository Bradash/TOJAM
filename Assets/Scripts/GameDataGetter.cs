using UnityEngine;

[DefaultExecutionOrder(-27)]
public class GameDataGetter : MonoBehaviour
{
    [SerializeField] GameObject[] quotaItems;
    [SerializeField] GameObject[] npcs;
    [SerializeField] Difficulty difficulty;
    private LocationNamer[] ailes;
    private void Start()
    {
        GameData.difficulty ??= difficulty;
        ailes = FindObjectsByType<LocationNamer>(FindObjectsSortMode.None);
        for (int i = 0; i < GameData.difficulty.quotaAmount; i++)
        {
            quotaItems[i].SetActive(true);
        }
        for (int i = 0; i < npcs.Length; i++)
        {
            npcs[i].SetActive(i < GameData.difficulty.npcAmount);
        }

        foreach (LocationNamer locationNamer in ailes)
        {
            locationNamer.Init();
        }
        ItemSystem.Instance.Setup();
    }
}
