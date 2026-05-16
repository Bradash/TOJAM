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

public enum LableType
{
    Organized,
    OldStyle,
    RandomEasy,
    RandomHard
}
[System.Serializable]
public struct Difficulty
{
    public string name;
    public int npcAmount;
    public int timerAmount;
    public int livesAmount;
    public int quotaAmount;
    [FormerlySerializedAs("deOrganizedAiles")] public bool disOrganizedAisles;
    public LableType lableType;

}