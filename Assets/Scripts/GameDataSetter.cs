using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-27)]
public class GameDataSetter : MonoBehaviour
{
    [SerializeField] Difficulty[] _difficulties;
    [SerializeField,HideInInspector] int[] npcAmount;
    [SerializeField,HideInInspector] int[] timerAmount;
    [SerializeField,HideInInspector] int[] livesAmount;
    [SerializeField,HideInInspector] int[] quotaAmount;
    [SerializeField,HideInInspector] bool[] deOrganizedAiles;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Slider difficultySlider;
    int diffIndex;

    [SerializeField] GameObject[] selectedLevel;


    private void Start()
    {
        difficultySlider.maxValue = _difficulties.Length-1;
        SetDifficulty();
    }
    public void SetDifficulty()
    {
        diffIndex = (int)difficultySlider.value;
        Difficulty difficulty = _difficulties[diffIndex];
        GameData.difficulty  = difficulty;
        
        SetText(difficulty);
        difficultyText.text = $"Difficulty:{difficulty.name}";
        
    }
    void SetText(Difficulty difficulty)
    {
        descriptionText.text = $"NPCs: {difficulty.npcAmount,2} | Timer: {difficulty.timerAmount,2} | Lives: {difficulty.livesAmount,2} | Quota: {difficulty.quotaAmount,2} | Aisle Labels: {difficulty.lableType.ToString()}";
    }
    public void level(int level) 
    { 
        GameData.currentLevel = level;
        for (int i = 0; i < selectedLevel.Length; i++)
        {
            if (i == level)
            {
                selectedLevel[i].SetActive(true);
            }
            else
            {
                selectedLevel[i].SetActive(false);
            }
        }
    }

}
