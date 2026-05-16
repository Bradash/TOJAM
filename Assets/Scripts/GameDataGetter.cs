using UnityEngine;

public class GameDataGetter : MonoBehaviour
{
    [SerializeField] GameObject[] quotaItems;
    [SerializeField] GameObject[] npcs;
    LocationNamer[] ailes;
    private void Start()
    {
        ailes = FindObjectsOfType<LocationNamer>();
        for (int i = 0; i < GameData.quotaAmount; i++)
        {
            quotaItems[i].SetActive(true);
        }
        for (int i = 0; i < GameData.npcAmount; i++)
        {
            npcs[i].SetActive(true);
        }
        for (int i = 0; i < ailes.Length; i++)
        {
            if (i < GameData.currentLevel)
            {
                ailes[i].gameObject.SetActive(GameData.randomizedLabels);
            }
        }
    }
}
