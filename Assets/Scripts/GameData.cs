using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-27)]
public class GameData : MonoBehaviour
{
    public static int currentLevel;
    public static Difficulty difficulty;
    //public static int npcAmount = 1;
    //public static int livesAmount = 3;
    //public static int quotaAmount = 1;
    //public static int timerAmount = 5;
    //public static bool randomizedLabels = false;
}


public enum LabelType
{
    Organized,
    OldStyle,
    RandomEasy,
    RandomHard
}
[System.Serializable]
public class Difficulty
{
    public string name;
    [Min(0)]
    public int npcAmount;
    [Min(1)] public int timerAmount;
    [Min(1)] public int livesAmount;
    [Min(1)] public int quotaAmount;
    [FormerlySerializedAs("lableType")] public LabelType labelType;

}