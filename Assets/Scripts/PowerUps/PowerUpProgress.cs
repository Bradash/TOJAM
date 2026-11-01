using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PowerUpProgress
{
    private const string SaveKey = "powerups.progress.v1";
    private static PowerUpWallet wallet;
    private static PowerUpCatalog catalog;
    private static string pendingScene;
    private static PowerUpSelection pendingSelection;
    private static int pendingPrice;
    private static string activeScene;
    private static string activeAttempt;
    private static PowerUpSelection activeSelection;
    public static event Action Changed;
    public static int LastWinPoints { get; private set; }
    public static int Points => Wallet.Points;
    public static PowerUpCatalog Catalog => catalog != null ? catalog : catalog = Resources.Load<PowerUpCatalog>("PowerUps/PowerUpCatalog");

    private static PowerUpWallet Wallet
    {
        get
        {
            if (wallet != null) return wallet;
            PowerUpSaveData data = null;
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { data = JsonUtility.FromJson<PowerUpSaveData>(json); }
                catch (ArgumentException) { Debug.LogWarning("Could not read power-up progress; starting with an empty wallet."); }
            }
            wallet = new PowerUpWallet(data, value =>
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(value));
                PlayerPrefs.Save();
            });
            return wallet;
        }
    }

    public static bool PreparePurchase(string scene, PowerUpSelection selection)
    {
        const PowerUpSelection all = PowerUpSelection.Scouter | PowerUpSelection.PhoneDistraction;
        if (Catalog == null || (selection & ~all) != 0 || string.IsNullOrEmpty(scene)) return false;
        scene = System.IO.Path.GetFileNameWithoutExtension(scene);
        int price = Catalog.Price(selection);
        if (price > Points) return false;
        pendingScene = scene;
        pendingSelection = selection;
        pendingPrice = price;
        return true;
    }

    public static void BeginLevel(string scene)
    {
        PowerUpSelection selection = pendingScene == scene ? pendingSelection : PowerUpSelection.None;
        int price = pendingScene == scene ? pendingPrice : 0;
        ClearPendingPurchase();
        if (!Wallet.TryBeginAttempt(scene, price, out activeAttempt))
        {
            selection = PowerUpSelection.None;
            Wallet.TryBeginAttempt(scene, 0, out activeAttempt);
        }
        activeScene = scene;
        activeSelection = selection;
        Changed?.Invoke();
    }

    public static bool IsEquipped(PowerUpSelection selection)
    {
        return activeScene == SceneManager.GetActiveScene().name && !string.IsNullOrEmpty(activeAttempt)
            && (activeSelection & selection) == selection;
    }

    public static bool AwardWin(string scene)
    {
        if (scene != activeScene || Catalog == null) return false;
        int reward = Mathf.Max(1, Catalog.pointsPerWin);
        if (!Wallet.TryAwardWin(activeAttempt, reward)) return false;
        LastWinPoints = reward;
        Changed?.Invoke();
        return true;
    }

    public static void ClearPendingPurchase()
    {
        pendingScene = null;
        pendingSelection = PowerUpSelection.None;
        pendingPrice = 0;
    }

    public static void EndLevel(string scene)
    {
        if (scene != activeScene) return;
        activeScene = activeAttempt = null;
        activeSelection = PowerUpSelection.None;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        wallet = null;
        catalog = null;
        activeScene = activeAttempt = null;
        activeSelection = PowerUpSelection.None;
        LastWinPoints = 0;
        Changed = null;
        ClearPendingPurchase();
    }
}
