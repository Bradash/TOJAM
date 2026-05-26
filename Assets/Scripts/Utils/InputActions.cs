using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActions
{
    /// <summary>
    /// Helper to enable and subscribe a callback method to an InputActionReference.
    /// </summary>
    internal static void RegisterAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback, [CallerMemberName] string memberName = "")
    {
        if (actionRef && actionRef.action != null)
        {
            actionRef.action.Enable();
            actionRef.action.performed += callback;
        }
        else
        {
            Debug.LogWarning($"An action reference on {memberName} is missing or not assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Helper to unsubscribe and clean up an InputActionReference callback.
    /// </summary>
    internal static void UnregisterAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (!actionRef || actionRef.action == null) return;
        actionRef.action.performed -= callback;
        actionRef.action.Disable();
    }
}