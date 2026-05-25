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
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("Drag the 'Previous' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference previousAction;

    [Tooltip("Drag the 'Next' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference nextAction;

    [Tooltip("Drag the 'Select1' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference select1Action;

    [Tooltip("Drag the 'Select2' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference select2Action;

    [Tooltip("Drag the 'Select3' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference select3Action;

    [Tooltip("Drag the 'Select4' action reference from your Input Action asset here.")]
    [SerializeField] private InputActionReference select4Action;

    private void OnEnable()
    {
        // Safely enable actions and subscribe to their execution events
        RegisterAction(interactAction, OnInteract);
        RegisterAction(previousAction, OnPrevious);
        RegisterAction(nextAction, OnNext);
        RegisterAction(select1Action, OnSelect1);
        RegisterAction(select2Action, OnSelect2);
        RegisterAction(select3Action, OnSelect3);
        RegisterAction(select4Action, OnSelect4);
    }

    private void OnDisable()
    {
        // Unsubscribe from callbacks to prevent memory leaks and disable input checking
        UnregisterAction(interactAction, OnInteract);
        UnregisterAction(previousAction, OnPrevious);
        UnregisterAction(nextAction, OnNext);
        UnregisterAction(select1Action, OnSelect1);
        UnregisterAction(select2Action, OnSelect2);
        UnregisterAction(select3Action, OnSelect3);
        UnregisterAction(select4Action, OnSelect4);
    }

    /// <summary>
    /// Helper to enable and subscribe a callback method to an InputActionReference.
    /// </summary>
    private void RegisterAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Enable();
            actionRef.action.performed += callback;
        }
        else
        {
            Debug.LogWarning($"An action reference on {gameObject.name} is missing or not assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Helper to unsubscribe and clean up an InputActionReference callback.
    /// </summary>
    private void UnregisterAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.performed -= callback;
            actionRef.action.Disable();
        }
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