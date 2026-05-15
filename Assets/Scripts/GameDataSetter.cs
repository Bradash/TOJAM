using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDataSetter : MonoBehaviour
{
    [SerializeField] int[] npcAmount;
    [SerializeField] int[] timerAmount;
    [SerializeField] int[] livesAmount;
    [SerializeField] int[] quotaAmount;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Slider difficultySlider;


    private void Start()
    {
        SetDifficulty();
    }
    public void SetDifficulty()
    {
        int difficulty = (int) difficultySlider.value;
        GameData.npcAmount = npcAmount[difficulty];
        GameData.timerAmount = timerAmount[difficulty];
        GameData.livesAmount = livesAmount[difficulty];
        GameData.quotaAmount = quotaAmount[difficulty];
        switch (difficultySlider.value)
        {
            case 0:
                descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString();
                difficultyText.text = "Difficulty:Very Easy";
                break;
            case 1:
                descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString();
                difficultyText.text = "Difficulty:Easy";
                break;
            case 2:
                descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString();
                difficultyText.text = "Difficulty:Normal";
                break;
            case 3:
                descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString();
                difficultyText.text = "Difficulty:Hard";
                break;
            case 4:
                descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString();
                difficultyText.text = "Difficulty:Impossible";
                break;
        }
    }
}
