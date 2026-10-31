using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

public class SOItemCreator : EditorWindow
{
    public enum CreationMode { CreateMissingOnly, CreateOrUpdate }

    public DefaultAsset targetFolder;
    public List<GameObject> prefabsToConvert = new List<GameObject>();
    public bool generatePreviewSprites = true;
    [SerializeField] private CreationMode mode;
    [SerializeField] private bool regenerateExistingIcons;
    [SerializeField] private DefaultAsset searchFolder;
    [SerializeField] private bool onlyItemPrefabs = true;

    private ReorderableList prefabList;
    private Vector2 scroll;
    private bool reviewDirty = true;
    private List<Entry> review = new List<Entry>();
    private readonly List<string> results = new List<string>();
    private readonly Dictionary<GameObject, List<ItemData>> existingData = new Dictionary<GameObject, List<ItemData>>();
    private List<Entry> queue;
    private string outputFolder;
    private int queueIndex, created, updated, skipped, failed, componentsAdded;
    private double previewStarted = -1;
    private bool running;
    private string summary;
    private const double PreviewTimeout = 5;
    private const string IconOwnerPrefix = "SOItemCreator:";

    private sealed class Entry
    {
        internal GameObject prefab;
        internal string path;
        internal ItemData data;
        internal string status;
        internal bool valid, skip, addComponent;
    }

    [MenuItem("Tools/SO Item Creator")]
    public static void ShowWindow()
    {
        GetWindow<SOItemCreator>("SO Creator");
    }

    private void OnEnable()
    {
        minSize = new Vector2(440, 440);
        if (prefabsToConvert == null) prefabsToConvert = new List<GameObject>();
        prefabList = new ReorderableList(prefabsToConvert, typeof(GameObject), true, true, true, true);
        prefabList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Item prefabs");
        prefabList.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            GameObject value = (GameObject)EditorGUI.ObjectField(rect, prefabsToConvert[index], typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                prefabsToConvert[index] = value;
                InvalidateReview();
            }
        };
        prefabList.onAddCallback = list => { prefabsToConvert.Add(null); InvalidateReview(); };
        prefabList.onChangedCallback = list => InvalidateReview();
        EditorApplication.projectChanged += InvalidateReview;
        Undo.undoRedoPerformed += InvalidateReview;
        AssemblyReloadEvents.beforeAssemblyReload += CancelBatch;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        InvalidateReview();
    }

    private void OnDisable()
    {
        CancelBatch();
        EditorApplication.projectChanged -= InvalidateReview;
        Undo.undoRedoPerformed -= InvalidateReview;
        AssemblyReloadEvents.beforeAssemblyReload -= CancelBatch;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) CancelBatch();
    }

    private void InvalidateReview()
    {
        reviewDirty = true;
        Repaint();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        using (new EditorGUI.DisabledScope(running || EditorApplication.isPlayingOrWillChangePlaymode))
        {
            EditorGUI.BeginChangeCheck();
            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", targetFolder, typeof(DefaultAsset), false);
            mode = (CreationMode)EditorGUILayout.EnumPopup("Mode", mode);
            generatePreviewSprites = EditorGUILayout.Toggle("Generate Preview Sprites", generatePreviewSprites);
            if (mode == CreationMode.CreateOrUpdate && generatePreviewSprites)
                regenerateExistingIcons = EditorGUILayout.Toggle("Regenerate Existing Icons", regenerateExistingIcons);
            if (EditorGUI.EndChangeCheck()) InvalidateReview();

            bool validFolder = TryGetOutputFolder(out string folder, out string folderError);
            EditorGUILayout.LabelField("New assets go to", folder);
            if (!validFolder) EditorGUILayout.HelpBox(folderError, MessageType.Error);
            EditorGUILayout.HelpBox("Existing item names, transforms, and swap lists are preserved. Sprites are kept unless regeneration is selected. Missing root Item components are added automatically.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected Prefabs")) AddPrefabs(Selection.objects);
            if (GUILayout.Button("Clear List", GUILayout.Width(90)))
            {
                prefabsToConvert.Clear();
                InvalidateReview();
            }
            EditorGUILayout.EndHorizontal();
            searchFolder = (DefaultAsset)EditorGUILayout.ObjectField("Search Folder (Optional)", searchFolder, typeof(DefaultAsset), false);
            if (searchFolder != null)
                onlyItemPrefabs = EditorGUILayout.Toggle("Only Prefabs With Item", onlyItemPrefabs);
            using (new EditorGUI.DisabledScope(searchFolder != null && !IsAssetsFolder(AssetDatabase.GetAssetPath(searchFolder))))
            {
                if (GUILayout.Button("Find Prefabs Missing ItemData")) FindUnassignedItemPrefabs();
            }
            string searchInfo = searchFolder == null
                ? "Searches all of Assets for prefabs with a root Item component."
                : onlyItemPrefabs ? "Only prefabs with a root Item component are included."
                : "Includes prefabs without Item; missing root components are added when processing.";
            EditorGUILayout.LabelField(searchInfo, EditorStyles.wordWrappedMiniLabel);
            DrawDragAndDropZone();
            prefabList.DoLayoutList();

            if (reviewDirty) RefreshReview();
            int ready = 0;
            foreach (Entry entry in review)
            {
                if (entry.valid && !entry.skip) ready++;
                EditorGUILayout.LabelField(entry.prefab != null ? entry.prefab.name : "Empty entry", entry.status);
            }
            using (new EditorGUI.DisabledScope(!validFolder || ready == 0))
            {
                if (GUILayout.Button($"Process {ready} Prefab{(ready == 1 ? "" : "s")}", GUILayout.Height(30))) BeginBatch();
            }
        }

        if (running && queueIndex < queue.Count)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            string currentName = queue[queueIndex].prefab != null ? queue[queueIndex].prefab.name : "Empty entry";
            EditorGUI.ProgressBar(rect, (float)queueIndex / queue.Count, $"{queueIndex}/{queue.Count}: {currentName}");
            if (GUILayout.Button("Cancel Remaining Items")) CancelBatch();
        }
        if (!string.IsNullOrEmpty(summary)) EditorGUILayout.HelpBox(summary, failed > 0 ? MessageType.Warning : MessageType.Info);
        foreach (string result in results) EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();
    }

    private static bool IsAssetsFolder(string path)
    {
        return (path == "Assets" || path.StartsWith("Assets/", StringComparison.Ordinal)) && AssetDatabase.IsValidFolder(path);
    }

    private bool TryGetOutputFolder(out string folder, out string error)
    {
        folder = targetFolder != null ? AssetDatabase.GetAssetPath(targetFolder) : "Assets";
        error = IsAssetsFolder(folder) ? null : "Choose an output folder inside Assets.";
        return error == null;
    }

    private static bool IsWritable(string path)
    {
        return path.StartsWith("Assets/", StringComparison.Ordinal)
            && (!File.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReadOnly) == 0)
            && AssetDatabase.IsOpenForEdit(path, StatusQueryOptions.UseCachedIfPossible);
    }

    private void IndexExistingData()
    {
        existingData.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:ItemData", new[] { "Assets" }))
        {
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null || data.itemPrefab == null) continue;
            if (!existingData.TryGetValue(data.itemPrefab, out List<ItemData> matches))
                existingData.Add(data.itemPrefab, matches = new List<ItemData>());
            matches.Add(data);
        }
    }

    private Entry ReviewPrefab(GameObject prefab, HashSet<string> seen)
    {
        Entry entry = new Entry { prefab = prefab, status = "Choose a prefab asset." };
        if (prefab == null) return entry;
        entry.path = AssetDatabase.GetAssetPath(prefab);
        if (!entry.path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
            || AssetDatabase.LoadAssetAtPath<GameObject>(entry.path) != prefab
            || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Model)
        {
            entry.status = "Requires a regular prefab or prefab variant root.";
            return entry;
        }
        if (!seen.Add(entry.path))
        {
            entry.status = "Duplicate; skipped.";
            entry.skip = true;
            return entry;
        }
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (!IsWritable(entry.path) || (stage != null && stage.assetPath == entry.path))
        {
            entry.status = "Prefab is read-only or currently open in Prefab Mode.";
            return entry;
        }
        Item component = prefab.GetComponent<Item>();
        entry.addComponent = component == null;
        // Variants can inherit another prefab's data: never overwrite that shared asset.
        if (component != null && component.itemData != null
            && (component.itemData.itemPrefab == prefab || component.itemData.itemPrefab == null))
        {
            entry.data = component.itemData;
        }
        else if (existingData.TryGetValue(prefab, out List<ItemData> matches))
        {
            if (matches.Count != 1)
            {
                entry.status = "Multiple ItemData assets match; assign the intended one to Item first.";
                return entry;
            }
            entry.data = matches[0];
        }
        if (entry.data != null && !IsWritable(AssetDatabase.GetAssetPath(entry.data)))
        {
            entry.status = "Linked ItemData is read-only.";
            return entry;
        }
        entry.valid = true;
        entry.skip = mode == CreationMode.CreateMissingOnly && entry.data != null
            && component != null && component.itemData == entry.data && entry.data.itemPrefab == prefab;
        entry.status = entry.skip ? "Already linked; skipped."
            : entry.data == null ? "Create ItemData." : "Reuse existing ItemData.";
        if (entry.addComponent) entry.status += " Add root Item.";
        return entry;
    }

    private void RefreshReview()
    {
        IndexExistingData();
        review.Clear();
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (GameObject prefab in prefabsToConvert) review.Add(ReviewPrefab(prefab, seen));
        reviewDirty = false;
    }

    private void AddPrefabs(Object[] objects)
    {
        foreach (Object value in objects)
        {
            string path = AssetDatabase.GetAssetPath(value);
            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root != null && !prefabsToConvert.Contains(root)) prefabsToConvert.Add(root);
        }
        InvalidateReview();
    }

    private void FindUnassignedItemPrefabs()
    {
        string path = searchFolder != null ? AssetDatabase.GetAssetPath(searchFolder) : "Assets";
        if (!IsAssetsFolder(path)) return;
        bool requireItem = searchFolder == null || onlyItemPrefabs;
        IndexExistingData();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { path }))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            Item component = prefab != null ? prefab.GetComponent<Item>() : null;
            if (requireItem && component == null) continue;
            Entry entry = ReviewPrefab(prefab, new HashSet<string>());
            if (entry.valid && (component == null || component.itemData != entry.data || entry.data == null)
                && !prefabsToConvert.Contains(prefab)) prefabsToConvert.Add(prefab);
        }
        InvalidateReview();
    }

    private void DrawDragAndDropZone()
    {
        Rect area = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(area, "Drag & Drop Prefabs Here", EditorStyles.helpBox);
        Event current = Event.current;
        if (!GUI.enabled || !area.Contains(current.mousePosition)) return;
        if (current.type != EventType.DragUpdated && current.type != EventType.DragPerform) return;
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            AddPrefabs(DragAndDrop.objectReferences);
        }
        current.Use();
    }

    private void BeginBatch()
    {
        if (running || EditorApplication.isPlayingOrWillChangePlaymode || !TryGetOutputFolder(out outputFolder, out _)) return;
        RefreshReview();
        queue = new List<Entry>(review);
        queueIndex = created = updated = skipped = failed = componentsAdded = 0;
        previewStarted = -1;
        results.Clear();
        summary = null;
        running = true;
        EditorApplication.update += ProcessNext;
    }

    private void ProcessNext()
    {
        if (!running || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (queueIndex >= queue.Count) { FinishBatch(false); return; }
        Entry entry = queue[queueIndex];
        if (!entry.valid || entry.skip)
        {
            skipped++;
            results.Add($"Skipped: {(entry.prefab != null ? entry.prefab.name : "Empty entry")} — {entry.status}");
            Advance();
            return;
        }
        try
        {
            Entry current = ReviewPrefab(entry.prefab, new HashSet<string>());
            if (!current.valid) throw new InvalidOperationException(current.status);
            if (current.skip) { skipped++; results.Add($"Skipped: {entry.prefab.name} — already linked."); Advance(); return; }
            bool wantsIcon = generatePreviewSprites && (current.data == null || current.data.itemSprite == null || regenerateExistingIcons);
            Texture2D preview = null;
            bool fallback = false;
            if (wantsIcon)
            {
                if (previewStarted < 0) previewStarted = EditorApplication.timeSinceStartup;
                preview = AssetPreview.GetAssetPreview(current.prefab);
                if (preview == null && EditorApplication.timeSinceStartup - previewStarted < PreviewTimeout) return;
                if (preview == null)
                {
                    preview = AssetPreview.GetMiniThumbnail(current.prefab);
                    fallback = true;
                }
            }
            ProcessEntry(current, preview);
            if (current.data == null) created++; else updated++;
            if (current.addComponent) componentsAdded++;
            string iconNote = wantsIcon && preview == null ? " No preview available; sprite preserved."
                : fallback ? " Used thumbnail fallback." : "";
            results.Add($"{(current.data == null ? "Created" : "Updated")}: {current.prefab.name}.{iconNote}");
        }
        catch (Exception exception)
        {
            failed++;
            results.Add($"Failed: {(entry.prefab != null ? entry.prefab.name : entry.path)} — {exception.Message}");
            Debug.LogException(exception);
        }
        Advance();
    }

    private void Advance()
    {
        queueIndex++;
        previewStarted = -1;
        Repaint();
        if (queueIndex >= queue.Count) FinishBatch(false);
    }

    private void CancelBatch()
    {
        if (running) FinishBatch(true);
    }

    private void FinishBatch(bool cancelled)
    {
        EditorApplication.update -= ProcessNext;
        running = false;
        summary = $"{(cancelled ? "Cancelled. " : "Complete. ")}{created} created, {updated} updated, {skipped} skipped, {failed} failed. "
            + $"{componentsAdded} Item component(s) added."
            + (cancelled ? $" {queue.Count - queueIndex} remaining item(s) were not processed." : "");
        InvalidateReview();
    }

    private void ProcessEntry(Entry entry, Texture2D preview)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        string operation = $"Create/update item: {entry.prefab.name}";
        Undo.SetCurrentGroupName(operation);
        ItemData data = entry.data;
        string newDataPath = null, iconPath = null;
        byte[] previousIcon = null;
        bool newIcon = false;
        try
        {
            if (data == null)
            {
                data = CreateInstance<ItemData>();
                data.itemName = entry.prefab.name;
                data.itemPrefab = entry.prefab;
                newDataPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{SafeName(entry.prefab.name)}.asset");
                AssetDatabase.CreateAsset(data, newDataPath);
                if (!AssetDatabase.Contains(data)) throw new IOException("Could not create the ItemData asset.");
                Undo.RegisterCreatedObjectUndo(data, operation);
            }
            else Undo.RecordObject(data, operation);

            if (preview != null)
            {
                iconPath = GetIconPath(data, entry.prefab.name);
                newIcon = !File.Exists(iconPath);
                if (!newIcon) previousIcon = File.ReadAllBytes(iconPath);
                data.itemSprite = SavePreviewSprite(preview, iconPath, data);
            }
            data.itemPrefab = entry.prefab;
            Item component = entry.prefab.GetComponent<Item>();
            if (component == null) component = Undo.AddComponent<Item>(entry.prefab);
            Undo.RecordObject(component, operation);
            component.itemData = data;
            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(data);
            PrefabUtility.SavePrefabAsset(entry.prefab, out bool saved);
            if (!saved) throw new IOException("Could not save the prefab.");
            AssetDatabase.SaveAssetIfDirty(data);
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(group);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            if (newDataPath != null) AssetDatabase.DeleteAsset(newDataPath);
            if (entry.data == null && data != null && !AssetDatabase.Contains(data)) DestroyImmediate(data);
            if (newIcon && iconPath != null) AssetDatabase.DeleteAsset(iconPath);
            else if (previousIcon != null)
            {
                File.WriteAllBytes(iconPath, previousIcon);
                AssetDatabase.ImportAsset(iconPath);
            }
            if (entry.prefab != null) PrefabUtility.SavePrefabAsset(entry.prefab);
            if (entry.data != null) AssetDatabase.SaveAssetIfDirty(entry.data);
            throw;
        }
    }

    private string GetIconPath(ItemData data, string prefabName)
    {
        string currentPath = AssetDatabase.GetAssetPath(data.itemSprite);
        string owner = IconOwnerPrefix + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));
        if (!string.IsNullOrEmpty(currentPath) && currentPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            && IsWritable(currentPath) && AssetImporter.GetAtPath(currentPath)?.userData == owner)
            return currentPath;
        return AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{SafeName(prefabName)}_Icon.png");
    }

    private static string SafeName(string value)
    {
        string safe = string.Join("_", value.Split(Path.GetInvalidFileNameChars()));
        return string.IsNullOrWhiteSpace(safe) ? "Item" : safe;
    }

    private static Sprite SavePreviewSprite(Texture2D source, string path, ItemData owner)
    {
        Texture2D readable = null;
        RenderTexture temporary = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;
            readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();
            File.WriteAllBytes(path, readable.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            if (temporary != null) RenderTexture.ReleaseTemporary(temporary);
            if (readable != null) DestroyImmediate(readable);
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new IOException("Could not import the generated icon.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.sRGBTexture = true;
        importer.alphaIsTransparency = true;
        importer.userData = IconOwnerPrefix + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner));
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new IOException("Generated icon did not import as a Sprite.");
        return sprite;
    }
}
