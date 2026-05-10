using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SOItemCreator : EditorWindow
{
    public DefaultAsset targetFolder;
    public List<GameObject> prefabsToConvert = new List<GameObject>();
    
    private SerializedObject so;
    private SerializedProperty prefabsProperty;
    private SerializedProperty folderProperty;

    [MenuItem("Tools/SO Item Creator")]
    public static void ShowWindow()
    {
        GetWindow<SOItemCreator>("SO Creator");
    }

    private void OnEnable()
    {
        so = new SerializedObject(this);
        prefabsProperty = so.FindProperty("prefabsToConvert");
        folderProperty = so.FindProperty("targetFolder");
    }

    private void OnGUI()
    {
        so.Update();

        EditorGUILayout.Space();
        
        // Folder selection
        EditorGUILayout.LabelField("Step 1: Select Output Folder", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(folderProperty);
        
        if (targetFolder != null)
        {
            string path = AssetDatabase.GetAssetPath(targetFolder);
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorGUILayout.HelpBox("Selected asset is not a folder!", MessageType.Error);
            }
        }

        EditorGUILayout.Space();

        // Prefab list
        EditorGUILayout.LabelField("Step 2: Drag prefabs into the list", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(prefabsProperty, true);
        
        EditorGUILayout.Space();

        // Process button
        GUI.enabled = prefabsToConvert.Count > 0;
        if (GUILayout.Button("Generate ScriptableObjects", GUILayout.Height(30)))
        {
            CreateAssets();
        }
        GUI.enabled = true;

        so.ApplyModifiedProperties();
    }

    private void CreateAssets()
    {
        // Determine the root path
        string folderPath = "Assets";
        if (targetFolder != null)
        {
            string selectedPath = AssetDatabase.GetAssetPath(targetFolder);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                folderPath = selectedPath;
            }
        }

        int count = 0;
        foreach (GameObject prefab in prefabsToConvert)
        {
            if (prefab == null) continue;

            // Build the specific file path
            string assetPath = Path.Combine(folderPath, prefab.name + ".asset");

            // Instance creation
            ItemData asset = ScriptableObject.CreateInstance<ItemData>();
            asset.itemPrefab = prefab;
            asset.itemName= prefab.name;

            // Save and ensure unique name (prevents overwriting)
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Generation Complete", $"Created {count} assets in {folderPath}", "OK");
    }
}