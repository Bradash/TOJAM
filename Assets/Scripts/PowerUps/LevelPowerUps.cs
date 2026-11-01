using UnityEngine;

[DefaultExecutionOrder(40)]
public sealed class LevelPowerUps : MonoBehaviour
{
    private void Start()
    {
        foreach (GameObject pickup in GameObject.FindGameObjectsWithTag("Scouter"))
        {
            if (pickup.GetComponent<Scouter>() == null) pickup.SetActive(false);
        }
        StorePhone phone = FindFirstObjectByType<StorePhone>();
        if (phone != null) phone.Configure(PowerUpProgress.IsEquipped(PowerUpSelection.PhoneDistraction), PowerUpProgress.Catalog);
        if (!PowerUpProgress.IsEquipped(PowerUpSelection.Scouter)
            && !PowerUpProgress.IsEquipped(PowerUpSelection.PhoneDistraction)) return;
        PowerUpHUD prefab = Resources.Load<PowerUpHUD>("PowerUps/PowerUpHUD");
        if (prefab != null) Instantiate(prefab).Initialize(phone);
        else Debug.LogError("Power-up HUD prefab is missing.", this);
    }
}
