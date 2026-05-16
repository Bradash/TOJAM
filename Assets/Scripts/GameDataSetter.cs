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
        // GameData.npcAmount = npcAmount[diffIndex];
        // GameData.timerAmount = timerAmount[diffIndex];
        // GameData.livesAmount = livesAmount[diffIndex];
        // GameData.quotaAmount = quotaAmount[diffIndex];
        // GameData.randomizedLabels = deOrganizedAiles[diffIndex];
        
        SetText(difficulty);
        difficultyText.text = $"Difficulty:{difficulty.name}";
        
    }
    void SetText(Difficulty difficulty)
    {
        descriptionText.text = $"NPCs: {difficulty.npcAmount,2} | Timer: {difficulty.timerAmount,2} | Lives: {difficulty.livesAmount,2} | Quota: {difficulty.quotaAmount,2} | Aisle Labels: {difficulty.lableType.ToString()}";
    }

}
