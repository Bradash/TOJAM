using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop on a UI Button — clicking sets <see cref="targetPanel"/> active state.
/// Use mode = Show to open a panel, Hide to close, Toggle to flip.
/// Optionally also affects a second panel (e.g. open Settings while hiding Main Menu).
/// </summary>
[RequireComponent(typeof(Button))]
public class PanelToggleButton : MonoBehaviour
{
    public enum Mode { Show, Hide, Toggle }

    [Header("Primary Panel")]
    public GameObject targetPanel;
    public Mode       mode = Mode.Toggle;

    [Header("Secondary Panel (optional)")]
    [Tooltip("If set, this panel's active state is flipped relative to the primary action " +
             "(useful for Settings ⟷ MainMenu swapping).")]
    public GameObject otherPanel;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Apply);
    }

    public void Apply()
    {
        if (targetPanel == null) return;

        bool newState = mode switch
        {
            Mode.Show   => true,
            Mode.Hide   => false,
            _           => !targetPanel.activeSelf,
        };

        targetPanel.SetActive(newState);
        if (otherPanel != null) otherPanel.SetActive(!newState);
    }
}
