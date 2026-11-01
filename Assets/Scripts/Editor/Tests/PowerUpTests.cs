using System;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerUpTests
{
    [Test]
    public void NewWalletStartsEmptyAndAWinPaysExactlyOnce()
    {
        PowerUpWallet wallet = new PowerUpWallet(null, data => { });
        Assert.That(wallet.Points, Is.Zero);
        Assert.That(wallet.TryBeginAttempt("MainGame", 0, out string attempt), Is.True);
        Assert.That(wallet.TryAwardWin(attempt, 100), Is.True);
        Assert.That(wallet.TryAwardWin(attempt, 100), Is.False);
        Assert.That(wallet.Points, Is.EqualTo(100));
    }

    [Test]
    public void PurchasesChargeOnceAndAStaleAttemptCannotEarnPoints()
    {
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = 100 }, data => { });
        wallet.TryBeginAttempt("MainGame", 40, out string first);
        Assert.That(wallet.Points, Is.EqualTo(60));
        wallet.TryBeginAttempt("Mall", 60, out string second);
        Assert.That(wallet.Points, Is.Zero);
        Assert.That(wallet.TryAwardWin(first, 100), Is.False);
        Assert.That(wallet.TryAwardWin(second, 100), Is.True);
        Assert.That(wallet.Points, Is.EqualTo(100));
    }

    [Test]
    public void OverspendingNegativePricesAndMissingScenesAreRejectedWithoutSaving()
    {
        int saves = 0;
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = 10 }, data => saves++);
        Assert.That(wallet.TryBeginAttempt("MainGame", 11, out _), Is.False);
        Assert.That(wallet.TryBeginAttempt("MainGame", -1, out _), Is.False);
        Assert.That(wallet.TryBeginAttempt("", 0, out _), Is.False);
        Assert.That(wallet.Points, Is.EqualTo(10));
        Assert.That(saves, Is.Zero);
    }

    [Test]
    public void BalanceAndClaimedRewardSurviveSerialization()
    {
        string saved = null;
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = 100 }, data => saved = JsonUtility.ToJson(data));
        wallet.TryBeginAttempt("MainGame", 40, out string attempt);
        wallet.TryAwardWin(attempt, 100);
        PowerUpWallet reloaded = new PowerUpWallet(JsonUtility.FromJson<PowerUpSaveData>(saved), data => saved = JsonUtility.ToJson(data));
        Assert.That(reloaded.Points, Is.EqualTo(160));
        Assert.That(reloaded.TryAwardWin(attempt, 100), Is.False);
    }

    [Test]
    public void FailedSaveDoesNotSpendPoints()
    {
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = 100 }, data => throw new IOException("Test write failure"));
        Assert.Throws<IOException>(() => wallet.TryBeginAttempt("MainGame", 40, out _));
        Assert.That(wallet.Points, Is.EqualTo(100));
    }

    [Test]
    public void FailedRewardSaveDoesNotMarkTheWinClaimed()
    {
        bool fail = false;
        PowerUpWallet wallet = new PowerUpWallet(null, data => { if (fail) throw new IOException("Test write failure"); });
        wallet.TryBeginAttempt("MainGame", 0, out string attempt);
        fail = true;
        Assert.Throws<IOException>(() => wallet.TryAwardWin(attempt, 100));
        Assert.That(wallet.Points, Is.Zero);
        fail = false;
        Assert.That(wallet.TryAwardWin(attempt, 100), Is.True);
    }

    [Test]
    public void InvalidRewardsCannotChangeBalanceAndLargeRewardsDoNotOverflow()
    {
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = int.MaxValue - 5 }, data => { });
        wallet.TryBeginAttempt("MainGame", 0, out string attempt);
        Assert.That(wallet.TryAwardWin(attempt, 0), Is.False);
        Assert.That(wallet.TryAwardWin(attempt, -100), Is.False);
        Assert.That(wallet.TryAwardWin("", 100), Is.False);
        Assert.That(wallet.TryAwardWin(attempt, 100), Is.True);
        Assert.That(wallet.Points, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void LossAndRetryGiveNoRewardAndDoNotReuseThePreviousPurchase()
    {
        PowerUpWallet wallet = new PowerUpWallet(new PowerUpSaveData { points = 100 }, data => { });
        wallet.TryBeginAttempt("MainGame", 60, out string lost);
        wallet.TryBeginAttempt("MainGame", 0, out string retry);
        Assert.That(wallet.Points, Is.EqualTo(40));
        Assert.That(wallet.TryAwardWin(lost, 100), Is.False);
        Assert.That(wallet.TryAwardWin(retry, 100), Is.True);
    }

    [Test]
    public void ThirdAnsweredCallStillWorksThenTheWholeLocationBlocksCalls()
    {
        PhoneCallBudget phone = new PhoneCallBudget();
        for (int answer = 1; answer <= 3; answer++)
        {
            Assert.That(phone.TryRing(), Is.True);
            Assert.That(phone.TryAnswer(), Is.True);
            Assert.That(phone.Remaining, Is.EqualTo(3 - answer));
            Assert.That(phone.Answered, Is.True);
            Assert.That(phone.Busy, Is.True);
            phone.EndCall();
        }
        Assert.That(phone.Blocked, Is.True);
        Assert.That(phone.TryRing(), Is.False);
    }

    [Test]
    public void UnansweredAndCancelledCallsDoNotUseAnAnswer()
    {
        PhoneCallBudget phone = new PhoneCallBudget();
        for (int i = 0; i < 8; i++)
        {
            Assert.That(phone.TryRing(), Is.True);
            phone.EndCall();
        }
        Assert.That(phone.Remaining, Is.EqualTo(3));
        Assert.That(phone.Blocked, Is.False);
    }

    [Test]
    public void ACallCannotBeStartedTwiceOrAnsweredByTwoClerks()
    {
        PhoneCallBudget phone = new PhoneCallBudget();
        Assert.That(phone.TryAnswer(), Is.False);
        Assert.That(phone.TryRing(), Is.True);
        Assert.That(phone.TryRing(), Is.False);
        Assert.That(phone.TryAnswer(), Is.True);
        Assert.That(phone.TryAnswer(), Is.False);
        Assert.That(phone.TryRing(), Is.False);
        Assert.That(phone.Remaining, Is.EqualTo(2));
    }

    [Test]
    public void ANewAttemptHasAFreshLocationCallBudget()
    {
        PhoneCallBudget previous = new PhoneCallBudget();
        previous.TryRing(); previous.TryAnswer(); previous.EndCall();
        Assert.That(new PhoneCallBudget().Remaining, Is.EqualTo(3));
    }

    [Test]
    public void ShopHudAndCatalogResourcesAreReadyForUse()
    {
        PowerUpCatalog catalog = Resources.Load<PowerUpCatalog>("PowerUps/PowerUpCatalog");
        PowerUpShop shop = Resources.Load<PowerUpShop>("PowerUps/PowerUpShop");
        PowerUpHUD hud = Resources.Load<PowerUpHUD>("PowerUps/PowerUpHUD");
        Assert.That(catalog, Is.Not.Null);
        Assert.That(shop, Is.Not.Null);
        Assert.That(hud, Is.Not.Null);
        Assert.That(shop.startButton, Is.Not.Null);
        Assert.That(shop.cancelButton, Is.Not.Null);
        Assert.That(shop.scouterButton, Is.Not.Null);
        Assert.That(shop.phoneButton, Is.Not.Null);
        Assert.That(shop.scouterLabel, Is.Not.Null);
        Assert.That(shop.phoneLabel, Is.Not.Null);
        Assert.That(hud.phoneText, Is.Not.Null);
        Assert.That(hud.statusText, Is.Not.Null);
        Assert.That(catalog.Price(PowerUpSelection.None), Is.Zero);
        Assert.That(catalog.Price(PowerUpSelection.Scouter | PowerUpSelection.PhoneDistraction),
            Is.EqualTo(catalog.scouterPrice + catalog.phonePrice));
    }

    [Test]
    public void ShopCardLabelsFitTheirBounds()
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/PowerUps/PowerUpShop.prefab");
            GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, scene);
            foreach (TMPro.TMP_Text text in instance.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                if (text.name != "Title" && text.name != "Description") continue;
                text.ForceMeshUpdate(true);
                Assert.That(text.isTextOverflowing, Is.False, text.transform.parent.name + "/" + text.name);
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [Test]
    public void EachGameplaySceneHasOneConfiguredLocationPhone()
    {
        foreach (string path in new[] { "Assets/Scenes/MainGame.unity", "Assets/Scenes/GarageSale.unity", "Assets/Scenes/Mall.unity" })
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                int count = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                foreach (StorePhone phone in root.GetComponentsInChildren<StorePhone>(true))
                {
                    count++;
                    Assert.That(phone.answerPoint, Is.Not.Null);
                    Assert.That(phone.ringSource, Is.Not.Null);
                    Assert.That(phone.ringIndicator, Is.Not.Null);
                    Assert.That(phone.answerPoint.gameObject.scene, Is.EqualTo(scene));
                }
                Assert.That(count, Is.EqualTo(1), path);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
