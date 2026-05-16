using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDataSetter : MonoBehaviour
{
    [SerializeField] int[] npcAmount;
    [SerializeField] int[] timerAmount;
    [SerializeField] int[] livesAmount;
    [SerializeField] int[] quotaAmount;
    [SerializeField] bool[] deOrganizedAiles;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Slider difficultySlider;
    int difficulty;


    private void Start()
    {
        SetDifficulty();
    }
    public void SetDifficulty()
    {
        difficulty = (int)difficultySlider.value;
        GameData.npcAmount = npcAmount[difficulty];
        GameData.timerAmount = timerAmount[difficulty];
        GameData.livesAmount = livesAmount[difficulty];
        GameData.quotaAmount = quotaAmount[difficulty];
        GameData.randomizedLabels = deOrganizedAiles[difficulty];
        switch (difficultySlider.value)
        {
            case 0:
                setText();
                difficultyText.text = "Difficulty:Peaceful";
                break;
            case 1:
                setText();
                difficultyText.text = "Difficulty:Very Easy";
                break;
            case 2:
                setText();
                difficultyText.text = "Difficulty:Easy";
                break;
            case 3:
                setText();
                difficultyText.text = "Difficulty:Normal";
                break;
            case 4:
                setText();
                difficultyText.text = "Difficulty:Spicy Normal";
                break;
            case 5:
                setText();
                difficultyText.text = "Difficulty:Spicier Normal";
                break;
            case 6:
                setText();
                difficultyText.text = "Difficulty:Hard";
                break;
            case 7:
                setText();
                difficultyText.text = "Difficulty:Impossible";
                break;
        }
    }
    void setText()
    {
        descriptionText.text = "NPCS:" + npcAmount[difficulty].ToString() + " Timer:" + timerAmount[difficulty].ToString() + " Lives:" + livesAmount[difficulty].ToString() + " Quota:" + quotaAmount[difficulty].ToString() + " Deorganized Ailes: " + deOrganizedAiles[difficulty].ToString();
    }

}
