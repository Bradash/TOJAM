using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SOItemCreator : EditorWindow
{
    public DefaultAsset targetFolder;
    public List<GameObject> prefabsToConvert = new List<GameObject>();
    public bool generatePreviewSprites = true;
    
    private SerializedObject so;
    private SerializedProperty prefabsProperty;
    private SerializedProperty folderProperty;
    private SerializedProperty previewToggleProperty;

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
        previewToggleProperty = so.FindProperty("generatePreviewSprites");
    }

    private void OnGUI()
    {
        so.Update();

        EditorGUILayout.Space();
        
        // Step 1: Output Folder Selection
        EditorGUILayout.LabelField("Step 1: Select Output Folder", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(folderProperty);
        
        bool isValidFolder = true;
        if (targetFolder != null)
        {
            string path = AssetDatabase.GetAssetPath(targetFolder);
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorGUILayout.HelpBox("Selected asset is not a valid folder!", MessageType.Error);
                isValidFolder = false;
            }
        }

        EditorGUILayout.Space();

        // Step 2: Prefab Assignment & Options
        EditorGUILayout.LabelField("Step 2: Assign Prefabs & Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(previewToggleProperty, new GUIContent("Generate Preview Sprites"));

        EditorGUILayout.Space(5);
        
        // Auto-Find & Clear Controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Prefabs Missing ItemData", GUILayout.Height(25)))
        {
            FindUnassignedItemPrefabs();
        }
        if (GUILayout.Button("Clear List", GUILayout.Width(80), GUILayout.Height(25)))
        {
            prefabsToConvert.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        DrawDragAndDropZone();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(prefabsProperty, true);
        
        EditorGUILayout.Space();

        // Step 3: Process Button
        bool hasValidPrefabs = prefabsToConvert.Exists(p => p != null);
        GUI.enabled = hasValidPrefabs && isValidFolder;

        if (GUILayout.Button("Generate ScriptableObjects", GUILayout.Height(30)))
        {
            CreateAssets();
        }
        
        GUI.enabled = true;

        so.ApplyModifiedProperties();
    }

    private void FindUnassignedItemPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int addedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            if (prefab.TryGetComponent<Item>(out Item itemComponent))
            {
                if (itemComponent.itemData == null && !prefabsToConvert.Contains(prefab))
                {
                    prefabsToConvert.Add(prefab);
                    addedCount++;
                }
            }
        }

        Debug.Log($"[SO Creator] Found and added {addedCount} prefab(s) with unassigned ItemData.");
    }

    private void DrawDragAndDropZone()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0f, 40f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Prefabs Here", EditorStyles.helpBox);

        Event currentEvent = Event.current;
        if (!dropArea.Contains(currentEvent.mousePosition)) return;

        if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go))
                    {
                        if (!prefabsToConvert.Contains(go))
                        {
                            prefabsToConvert.Add(go);
                        }
                    }
                }
            }
            currentEvent.Use();
        }
    }

    private void CreateAssets()
    {
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

            // 1. Generate icon Sprite FIRST before creating the ScriptableObject
            Sprite iconSprite = null;
            if (generatePreviewSprites)
            {
                iconSprite = GeneratePreviewSprite(prefab, folderPath);
            }

            // 2. Create ScriptableObject instance
            ItemData asset = ScriptableObject.CreateInstance<ItemData>();

            // 3. Create the asset on disk
            string rawPath = Path.Combine(folderPath, $"{prefab.name}.asset").Replace('\\', '/');
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(rawPath);
            AssetDatabase.CreateAsset(asset, uniquePath);

            // 4. Use SerializedObject to guarantee field assignment persistence
            SerializedObject serializedAsset = new SerializedObject(asset);
            serializedAsset.Update();
            
            SerializedProperty prefabProp = serializedAsset.FindProperty("itemPrefab");
            if (prefabProp != null) prefabProp.objectReferenceValue = prefab;

            SerializedProperty nameProp = serializedAsset.FindProperty("itemName");
            if (nameProp != null) nameProp.stringValue = prefab.name;

            SerializedProperty spriteProp = serializedAsset.FindProperty("itemSprite");
            if (spriteProp != null) spriteProp.objectReferenceValue = iconSprite;

            serializedAsset.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);

            // 5. Link new ItemData back to the prefab's Item component
            if (prefab.TryGetComponent<Item>(out Item itemComponent))
            {
                SerializedObject serializedItem = new SerializedObject(itemComponent);
                serializedItem.Update();
                
                SerializedProperty itemDataProp = serializedItem.FindProperty("itemData");
                if (itemDataProp != null)
                {
                    itemDataProp.objectReferenceValue = asset;
                }
                
                serializedItem.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
            }

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Generation Complete", $"Created {count} ItemData asset(s) in:\n{folderPath}", "OK");
    }

    private Sprite GeneratePreviewSprite(GameObject prefab, string folderPath)
    {
        int assetInstanceID = prefab.GetInstanceID();
        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefab);

        int attempts = 0;
        while (AssetPreview.IsLoadingAssetPreview(assetInstanceID) && attempts < 100)
        {
            System.Threading.Thread.Sleep(15);
            previewTexture = AssetPreview.GetAssetPreview(prefab);
            attempts++;
        }

        if (previewTexture == null)
        {
            previewTexture = AssetPreview.GetMiniThumbnail(prefab);
        }

        if (previewTexture == null) return null;

        // Save PNG file
        string iconPath = Path.Combine(folderPath, $"{prefab.name}_Icon.png").Replace('\\', '/');
        string uniqueIconPath = AssetDatabase.GenerateUniqueAssetPath(iconPath);

        Texture2D readableTexture = MakeTextureReadable(previewTexture);
        byte[] pngData = readableTexture.EncodeToPNG();
        File.WriteAllBytes(uniqueIconPath, pngData);
        DestroyImmediate(readableTexture);

        // Initial import to register the file
        AssetDatabase.ImportAsset(uniqueIconPath, ImportAssetOptions.ForceSynchronousImport);

        // Configure TextureImporter settings
        TextureImporter importer = AssetImporter.GetAtPath(uniqueIconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        // Force synchronous re-import AFTER applying importer settings
        AssetDatabase.ImportAsset(uniqueIconPath, ImportAssetOptions.ForceSynchronousImport);

        // Load generated Sprite asset
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(uniqueIconPath);

        // Sub-asset fallback search
        if (sprite == null)
        {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(uniqueIconPath);
            foreach (Object obj in allAssets)
            {
                if (obj is Sprite s)
                {
                    sprite = s;
                    break;
                }
            }
        }

        return sprite;
    }

    private static Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);

        Graphics.Blit(source, renderTex);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableText;
    }
}