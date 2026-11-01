using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpCatalog", menuName = "Power Ups/Catalog")]
public sealed class PowerUpCatalog : ScriptableObject
{
    [Min(1)] public int pointsPerWin = 100;
    [Min(0)] public int scouterPrice = 40;
    [Min(0)] public int phonePrice = 60;
    [Min(1)] public float phoneRingTimeout = 20;
    [Min(1)] public float phoneAnswerDuration = 6;

    public int Price(PowerUpSelection selection)
    {
        int price = 0;
        if ((selection & PowerUpSelection.Scouter) != 0) price += Mathf.Max(0, scouterPrice);
        if ((selection & PowerUpSelection.PhoneDistraction) != 0) price += Mathf.Max(0, phonePrice);
        return price;
    }
}
