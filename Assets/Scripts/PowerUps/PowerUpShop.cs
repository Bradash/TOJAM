using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class PowerUpShop : MonoBehaviour
{
    public TMP_Text heading;
    public TMP_Text balance;
    public TMP_Text scouterLabel;
    public TMP_Text phoneLabel;
    public TMP_Text totalLabel;
    public TMP_Text message;
    public Button scouterButton;
    public Button phoneButton;
    public Button startButton;
    public Button cancelButton;
    private static PowerUpShop current;
    private PowerUpSelection selection;
    private string destination;
    private float previousTimeScale;
    private CursorLockMode previousCursor;
    private bool previousCursorVisible;
    private bool launching;
    private GameObject previousSelection;
    public static bool IsOpen => current != null;

    public static void Open(string scene)
    {
        if (current != null) return;
        PowerUpShop prefab = Resources.Load<PowerUpShop>("PowerUps/PowerUpShop");
        if (prefab == null || PowerUpProgress.Catalog == null)
        {
            Debug.LogError("Power-up shop assets are missing. Run Tools/Power Ups/Build Shop Assets in the editor.");
            return;
        }
        current = Instantiate(prefab);
        current.destination = scene;
        current.previousTimeScale = Time.timeScale;
        current.previousCursor = Cursor.lockState;
        current.previousCursorVisible = Cursor.visible;
        current.previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        current.scouterButton.onClick.AddListener(() => current.Toggle(PowerUpSelection.Scouter));
        current.phoneButton.onClick.AddListener(() => current.Toggle(PowerUpSelection.PhoneDistraction));
        current.startButton.onClick.AddListener(current.StartAttempt);
        current.cancelButton.onClick.AddListener(current.Cancel);
        PowerUpProgress.Changed += current.Refresh;
        current.heading.text = "GEAR UP";
        current.message.text = PowerUpProgress.LastWinPoints > 0
            ? $"Last win: +{PowerUpProgress.LastWinPoints} points. Choose gear for this attempt."
            : "Win levels to earn points. Gear is optional and lasts one attempt.";
        current.Refresh();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(current.startButton.gameObject);
    }

    private void Toggle(PowerUpSelection powerUp)
    {
        if (launching) return;
        PowerUpSelection next = selection ^ powerUp;
        if (PowerUpProgress.Catalog.Price(next) > PowerUpProgress.Points) return;
        selection = next;
        Refresh();
    }

    private void Refresh()
    {
        PowerUpCatalog catalog = PowerUpProgress.Catalog;
        int total = catalog.Price(selection);
        balance.text = $"{PowerUpProgress.Points} POINTS";
        scouterLabel.text = (Has(PowerUpSelection.Scouter) ? "REMOVE" : "EQUIP") + $"  /  {catalog.scouterPrice} PTS";
        phoneLabel.text = (Has(PowerUpSelection.PhoneDistraction) ? "REMOVE" : "EQUIP") + $"  /  {catalog.phonePrice} PTS";
        scouterButton.interactable = !launching && (Has(PowerUpSelection.Scouter)
            || total + catalog.scouterPrice <= PowerUpProgress.Points);
        phoneButton.interactable = !launching && (Has(PowerUpSelection.PhoneDistraction)
            || total + catalog.phonePrice <= PowerUpProgress.Points);
        totalLabel.text = $"LOADOUT: {total} PTS   /   AFTER START: {PowerUpProgress.Points - total} PTS";
        startButton.interactable = !launching && total <= PowerUpProgress.Points;
        cancelButton.interactable = !launching;
    }

    private bool Has(PowerUpSelection powerUp) => (selection & powerUp) != 0;

    public void StartAttempt()
    {
        if (launching) return;
        if (!Application.CanStreamedLevelBeLoaded(destination))
        {
            message.text = "This level is unavailable. No points have been spent.";
            return;
        }
        if (!PowerUpProgress.PreparePurchase(destination, selection))
        {
            message.text = "Your balance changed. Remove some gear and try again.";
            Refresh();
            return;
        }
        launching = true;
        Refresh();
        Time.timeScale = 1;
        try { SceneManager.LoadScene(destination); }
        catch (System.Exception exception)
        {
            launching = false;
            PowerUpProgress.ClearPendingPurchase();
            Time.timeScale = 0;
            message.text = "Could not start the level. No points have been spent.";
            Debug.LogException(exception);
            Refresh();
        }
    }

    public void Cancel()
    {
        if (launching) return;
        PowerUpProgress.ClearPendingPurchase();
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursor;
        Cursor.visible = previousCursorVisible;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!launching && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Cancel();
    }

    private void OnDestroy()
    {
        PowerUpProgress.Changed -= Refresh;
        if (current == this) current = null;
    }
}
