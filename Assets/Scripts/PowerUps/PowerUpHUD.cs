using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PowerUpHUD : MonoBehaviour
{
    public TMP_Text scouterText;
    public TMP_Text phoneText;
    public TMP_Text statusText;
    private StorePhone phone;
    private InputAction call;
    private string feedback;
    private float feedbackUntil;

    public void Initialize(StorePhone storePhone)
    {
        phone = storePhone;
        scouterText.gameObject.SetActive(PowerUpProgress.IsEquipped(PowerUpSelection.Scouter));
        phoneText.gameObject.SetActive(PowerUpProgress.IsEquipped(PowerUpSelection.PhoneDistraction));
        scouterText.text = "SCOUTER ONLINE";
        call = new InputAction("Call store phone", InputActionType.Button, "<Keyboard>/p");
        call.performed += OnCall;
        call.Enable();
        if (phone != null) phone.Changed += Refresh;
        Refresh();
    }

    private void OnCall(InputAction.CallbackContext context)
    {
        if (phone == null || !PowerUpProgress.IsEquipped(PowerUpSelection.PhoneDistraction)
            || Time.timeScale <= 0 || (GameManager.Instance != null && GameManager.Instance.GameOver)) return;
        phone.TryCall(out feedback);
        feedbackUntil = Time.unscaledTime + 3;
        Refresh();
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(feedback) && Time.unscaledTime >= feedbackUntil)
        {
            feedback = null;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (phone == null) phoneText.text = "PHONE UNAVAILABLE";
        else phoneText.text = phone.NumberBlocked ? "PHONE / NUMBER BLOCKED"
            : phone.Busy ? $"PHONE / LINE BUSY / {phone.CallsRemaining} LEFT" : $"[P] CALL LOCATION / {phone.CallsRemaining} LEFT";
        statusText.text = !string.IsNullOrEmpty(feedback) ? feedback
            : PowerUpProgress.IsEquipped(PowerUpSelection.PhoneDistraction) && phone != null ? phone.Status : "";
    }

    private void OnDestroy()
    {
        if (phone != null) phone.Changed -= Refresh;
        if (call != null) { call.performed -= OnCall; call.Dispose(); }
    }
}
