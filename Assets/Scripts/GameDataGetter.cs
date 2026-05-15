using UnityEngine;

public class GameDataGetter : MonoBehaviour
{
    [SerializeField] GameObject[] quotaItems;
    [SerializeField] GameObject[] npcs;
    private void Start()
    {
        for (int i = 0; i < GameData.quotaAmount; i++)
        {
            quotaItems[i].SetActive(true);
        }
        for (int i = 0; i < GameData.npcAmount; i++)
        {
            npcs[i].SetActive(true);
        }
    }
}
