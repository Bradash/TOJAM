using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Place this above an InputActionReference to show a dropdown menu of all project-wide actions.
/// Optionally filters by Action Type, Control Type, or both.
/// <para>Examples:</para>
/// <para><c>[InputActionDropdown]</c> (No filter)</para>
/// <para><c>[InputActionDropdown(InputActionType.Value)]</c> (Filter by Action Type)</para>
/// <para><c>[InputActionDropdown(InputActionControlType.Vector2)]</c> (Filter by Control Type)</para>
/// <para><c>[InputActionDropdown(InputActionType.Value, InputActionControlType.Vector2)]</c> (Filter by both)</para>
/// </summary>
public class InputActionDropdownAttribute : PropertyAttribute
{
    public bool FilterByActionType { get; }
    public InputActionType ActionType { get; }
    public InputActionControlType ControlType { get; }

    /// <summary> Shows all project-wide Input Actions without filtering. </summary>
    public InputActionDropdownAttribute()
    {
        FilterByActionType = false;
        ControlType = InputActionControlType.Any;
    }

    /// <summary> Filters project-wide Input Actions by a specific Action Type. </summary>
    public InputActionDropdownAttribute(InputActionType actionType)
    {
        FilterByActionType = true;
        ActionType = actionType;
        ControlType = InputActionControlType.Any;
    }

    /// <summary> Filters project-wide Input Actions by a specific Control Type. </summary>
    public InputActionDropdownAttribute(InputActionControlType controlType)
    {
        FilterByActionType = false;
        ControlType = controlType;
    }

    /// <summary> Filters project-wide Input Actions by both Action Type and Control Type. </summary>
    public InputActionDropdownAttribute(InputActionType actionType, InputActionControlType controlType)
    {
        FilterByActionType = true;
        ActionType = actionType;
        ControlType = controlType;
    }
}