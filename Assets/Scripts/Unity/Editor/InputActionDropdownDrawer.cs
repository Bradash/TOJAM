using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.Editor
{
    [CustomPropertyDrawer(typeof(InputActionDropdownAttribute))]
    public class InputActionDropdownDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            // Base height is now just 1 line instead of 2
            float height = EditorGUIUtility.singleLineHeight;

            string error = GetValidationError(property, (InputActionDropdownAttribute)attribute);
            if (!string.IsNullOrEmpty(error))
            {
                height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [InputActionDropdown] with InputActionReference.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float h = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;

            // The rect for the main line (excluding any warning box height)
            Rect lineRect = new Rect(position.x, position.y, position.width, h);

            // Draw the prefix label (e.g. "Move Action") and get the remaining right-side space
            Rect controlRect = EditorGUI.PrefixLabel(lineRect, label);

            // Split the remaining space in half with a small gap
            float gap = 4f;
            float halfWidth = (controlRect.width - gap) / 2f;

            Rect fieldRect = new Rect(controlRect.x, controlRect.y, halfWidth, h);
            Rect buttonRect = new Rect(controlRect.x + halfWidth + gap, controlRect.y, halfWidth, h);

            // Draw the standard object field (pass GUIContent.none so it doesn't draw a second label)
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

            string buttonText = property.objectReferenceValue != null
                ? property.objectReferenceValue.name
                : "Select Action...";

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(buttonText), FocusType.Keyboard))
            {
                ShowDropdown(property, buttonRect);
            }

            string error = GetValidationError(property, (InputActionDropdownAttribute)attribute);
            if (!string.IsNullOrEmpty(error))
            {
                Rect helpBoxRect = new Rect(position.x, lineRect.yMax + space, position.width, h * 2);
                EditorGUI.HelpBox(helpBoxRect, error, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Helper method to resolve Unity's implicit control types.
        /// If an Action Type is "Button", Unity leaves ExpectedControlType blank.
        /// </summary>
        private string GetEffectiveControlType(InputAction action)
        {
            if (!string.IsNullOrEmpty(action.expectedControlType))
            {
                return action.expectedControlType;
            }

            return action.type == InputActionType.Button ? "Button" : "Any";
        }

        private string GetValidationError(SerializedProperty property, InputActionDropdownAttribute filter)
        {
            if (property.objectReferenceValue == null) return null;

            var reference = property.objectReferenceValue as InputActionReference;
            if (reference == null) return null;

            var action = reference.action;
            if (action == null) return "Action reference is missing or invalid.";

            if (filter.FilterByActionType && action.type != filter.ActionType)
            {
                return $"Expected Action Type '{filter.ActionType}', but assigned action is '{action.type}'.";
            }

            if (filter.ControlType != InputActionControlType.Any)
            {
                string expectedStr = filter.ControlType.ToString();
                string actualStr = GetEffectiveControlType(action);

                // Note: Enum names match the internal control type names (e.g. DiscreteButton, Vector2)
                if (!string.Equals(actualStr, expectedStr, System.StringComparison.OrdinalIgnoreCase))
                {
                    return $"Expected Control Type '{expectedStr}', but assigned action is '{actualStr}'.";
                }
            }

            return null;
        }

        private void ShowDropdown(SerializedProperty property, Rect dropdownRect)
        {
            GenericMenu menu = new GenericMenu();
            InputActionDropdownAttribute filter = (InputActionDropdownAttribute)attribute;

            menu.AddItem(new GUIContent("None"), property.objectReferenceValue == null, () =>
            {
                property.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            InputActionAsset projectActions = null;

            var actionsProp = typeof(InputSystem).GetProperty("actions",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (actionsProp != null)
            {
                projectActions = actionsProp.GetValue(null) as InputActionAsset;
            }

            if (projectActions == null && InputSystem.settings != null)
            {
                SerializedObject settingsObj = new SerializedObject(InputSystem.settings);
                SerializedProperty projectWideActionsProp = settingsObj.FindProperty("m_ProjectWideActions");
                if (projectWideActionsProp != null)
                {
                    projectActions = projectWideActionsProp.objectReferenceValue as InputActionAsset;
                }
            }

            if (projectActions == null)
            {
                menu.AddItem(new GUIContent("No Project-wide Actions assigned in Project Settings"), false, null);
                menu.DropDown(dropdownRect);
                return;
            }

            string path = AssetDatabase.GetAssetPath(projectActions);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            var references = allAssets.OfType<InputActionReference>().ToList();

            if (references.Count == 0)
            {
                menu.AddItem(new GUIContent("Project-wide Actions asset is empty"), false, null);
                menu.DropDown(dropdownRect);
                return;
            }

            int addedCount = 0;

            foreach (var reference in references)
            {
                if (reference == null) continue;

                string mapName = "Unknown Map";
                string actionName = reference.name.Replace("/", "");

                var action = reference.action;
                if (action != null)
                {
                    actionName = action.name.Replace("/", "");
                    if (action.actionMap != null)
                    {
                        mapName = action.actionMap.name.Replace("/", "");
                    }

                    if (filter.FilterByActionType && action.type != filter.ActionType)
                    {
                        continue;
                    }

                    if (filter.ControlType != InputActionControlType.Any)
                    {
                        string expectedStr = filter.ControlType.ToString();
                        string actualStr = GetEffectiveControlType(action);

                        if (!string.Equals(actualStr, expectedStr, System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }

                string menuPath = $"{mapName}/{actionName}";
                bool isSelected = property.objectReferenceValue == reference;

                menu.AddItem(new GUIContent(menuPath), isSelected, () =>
                {
                    property.objectReferenceValue = reference;
                    property.serializedObject.ApplyModifiedProperties();
                });

                addedCount++;
            }

            if (addedCount == 0)
            {
                string filterDesc = "";
                if (filter.FilterByActionType) filterDesc += $"[{filter.ActionType}]";
                if (filter.ControlType != InputActionControlType.Any) filterDesc += $" ({filter.ControlType})";

                menu.AddDisabledItem(new GUIContent($"No actions matching filter: {filterDesc}"));
            }

            menu.DropDown(dropdownRect);
        }
    }
}