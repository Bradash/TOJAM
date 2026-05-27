using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A controller script that handles Unity Input System callbacks for the actions:
/// Interact, Previous, Next, Select1, Select2, Select3, and Select4.
/// </summary>
public class InteractInputController : MonoBehaviour
{
    [SerializeField] private ItemInteraction interaction;
    [Header("Action References")]
    [Tooltip("Drag the 'Interact' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("Drag the 'Previous' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference previousAction;

    [Tooltip("Drag the 'Next' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference nextAction;

    [Tooltip("Drag the 'Select1' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference select1Action;

    [Tooltip("Drag the 'Select2' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference select2Action;

    [Tooltip("Drag the 'Select3' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference select3Action;

    [Tooltip("Drag the 'Select4' action reference from your Input Action asset here.")]
    [InputActionDropdown(InputActionType.Button)]
    [SerializeField] private InputActionReference select4Action;

    private void OnEnable()
    {
        // Safely enable actions and subscribe to their execution events
        InputActions.RegisterAction(interactAction, OnInteract);
        InputActions.RegisterAction(previousAction, OnPrevious);
        InputActions.RegisterAction(nextAction, OnNext);
        InputActions.RegisterAction(select1Action, OnSelect1);
        InputActions.RegisterAction(select2Action, OnSelect2);
        InputActions.RegisterAction(select3Action, OnSelect3);
        InputActions.RegisterAction(select4Action, OnSelect4);
    }

    private void OnDisable()
    {
        // Unsubscribe from callbacks to prevent memory leaks and disable input checking
        InputActions.UnregisterAction(interactAction, OnInteract);
        InputActions.UnregisterAction(previousAction, OnPrevious);
        InputActions.UnregisterAction(nextAction, OnNext);
        InputActions.UnregisterAction(select1Action, OnSelect1);
        InputActions.UnregisterAction(select2Action, OnSelect2);
        InputActions.UnregisterAction(select3Action, OnSelect3);
        InputActions.UnregisterAction(select4Action, OnSelect4);
    }

    // ==========================================
    // ACTION CALLBACK METHODS
    // ==========================================

    private void OnInteract(InputAction.CallbackContext context)
    {
        interaction.Interact();
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(false);
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(true);
    }

    private void OnSelect1(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(0);
    }

    private void OnSelect2(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(1);
    }

    private void OnSelect3(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(2);
    }

    private void OnSelect4(InputAction.CallbackContext context)
    {
        interaction.SelectSlot(3);
    }
}