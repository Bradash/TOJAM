using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ItemSystem))]
public class ItemSystemEditor : Editor
{
    private readonly ItemSetSection storeSection = new ItemSetSection();
    private readonly ItemSetSection swappedSection = new ItemSetSection();

    private sealed class ItemSetSection
    {
        public ItemSet asset;
        public SerializedObject serializedAsset;
        public ReorderableList list;
        public bool expanded;
        public bool scenesExpanded;

        public void Dispose()
        {
            serializedAsset?.Dispose();
            serializedAsset = null;
            list = null;
            asset = null;
        }

        public void SetAsset(ItemSet value)
        {
            if (asset == value && serializedAsset != null) return;

            Dispose();
            asset = value;
            serializedAsset = value != null ? new SerializedObject(value) : null;
            list = null;
            if (serializedAsset == null) return;

            SerializedProperty items = serializedAsset.FindProperty("items");
            list = new ReorderableList(serializedAsset, items, true, false, true, true);
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, items.GetArrayElementAtIndex(index), GUIContent.none);
            };
            list.elementHeight = EditorGUIUtility.singleLineHeight + 4;
        }
    }

    private void OnEnable()
    {
        EditorApplication.projectChanged += Repaint;
        Undo.undoRedoPerformed += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= Repaint;
        Undo.undoRedoPerformed -= Repaint;
        storeSection.Dispose();
        swappedSection.Dispose();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

        DrawItemSet(serializedObject.FindProperty("storeItems"), storeSection);
        EditorGUILayout.Space();
        DrawItemSet(serializedObject.FindProperty("swappedItems"), swappedSection);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawItemSet(SerializedProperty property, ItemSetSection section)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(property);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        ItemSet itemSet = property.objectReferenceValue as ItemSet;
        section.SetAsset(itemSet);
        if (itemSet == null) return;

        List<string> scenes = ItemSetSceneUsage.GetScenesUsing(itemSet);
        bool shared = scenes.Count > 1;
        section.scenesExpanded = EditorGUILayout.Foldout(section.scenesExpanded,
            $"Used by {scenes.Count} scene{(scenes.Count == 1 ? "" : "s")}", true);
        if (section.scenesExpanded)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (string scene in scenes)
                    EditorGUILayout.LabelField(new GUIContent(Path.GetFileNameWithoutExtension(scene), scene));
            }
        }

        if (shared)
        {
            EditorGUILayout.HelpBox("This set is shared. Make a copy for this scene to edit its item list.", MessageType.Info);
            ItemSystem system = (ItemSystem)target;
            bool sceneInstance = !EditorUtility.IsPersistent(system) && system.gameObject.scene.IsValid()
                && !EditorSceneManager.IsPreviewScene(system.gameObject.scene);
            using (new EditorGUI.DisabledScope(!sceneInstance || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Make Copy for This Scene"))
                {
                    MakeSceneCopy(property, itemSet);
                    section.expanded = true;
                    GUIUtility.ExitGUI();
                }
            }
        }

        section.serializedAsset.Update();
        section.expanded = EditorGUILayout.Foldout(section.expanded,
            $"Items ({section.list.serializedProperty.arraySize})", true);
        if (section.expanded)
        {
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(shared || EditorApplication.isPlayingOrWillChangePlaymode))
                section.list.DoLayoutList();
        }
        section.serializedAsset.ApplyModifiedProperties();
    }

    private void MakeSceneCopy(SerializedProperty property, ItemSet source)
    {
        ItemSystem system = (ItemSystem)target;
        string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(source));
        if (string.IsNullOrEmpty(folder)) folder = "Assets";
        string sceneName = string.IsNullOrEmpty(system.gameObject.scene.name) ? "Scene" : system.gameObject.scene.name;
        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{folder}/{sceneName} {source.name}.asset".Replace('\\', '/'));

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        const string undoName = "Make Item Set Copy for Scene";
        Undo.SetCurrentGroupName(undoName);

        ItemSet copy = Instantiate(source);
        copy.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(copy, path);
        Undo.RegisterCreatedObjectUndo(copy, undoName);
        property.objectReferenceValue = copy;
        serializedObject.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(system.gameObject.scene);
        AssetDatabase.SaveAssetIfDirty(copy);
        Undo.CollapseUndoOperations(undoGroup);
        ItemSetSceneUsage.Invalidate();
        EditorGUIUtility.PingObject(copy);
    }
}

[InitializeOnLoad]
internal static class ItemSetSceneUsage
{
    private static readonly Dictionary<string, HashSet<string>> sceneDependencies = new Dictionary<string, HashSet<string>>();
    private static bool dirty = true;

    static ItemSetSceneUsage()
    {
        EditorApplication.projectChanged += Invalidate;
        EditorSceneManager.sceneSaved += OnSceneSaved;
        Undo.undoRedoPerformed += Invalidate;
    }

    internal static void Invalidate()
    {
        dirty = true;
    }

    private static void OnSceneSaved(Scene scene)
    {
        Invalidate();
    }

    internal static List<string> GetScenesUsing(ItemSet itemSet)
    {
        if (dirty)
        {
            sceneDependencies.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                sceneDependencies[path] = new HashSet<string>(AssetDatabase.GetDependencies(path, true));
            }
            dirty = false;
        }

        string assetPath = AssetDatabase.GetAssetPath(itemSet);
        HashSet<string> scenes = new HashSet<string>();
        if (!string.IsNullOrEmpty(assetPath))
        {
            foreach (KeyValuePair<string, HashSet<string>> scene in sceneDependencies)
            {
                if (scene.Value.Contains(assetPath)) scenes.Add(scene.Key);
            }
        }

        // Loaded scenes may have unsaved assignments, so their live references take priority.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || EditorSceneManager.IsPreviewScene(scene)) continue;
            string key = string.IsNullOrEmpty(scene.path) ? $"Unsaved scene {scene.name} ({scene.handle})" : scene.path;
            scenes.Remove(key);
            bool found = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (ItemSystem system in root.GetComponentsInChildren<ItemSystem>(true))
                {
                    if (system.storeItems == itemSet || system.swappedItems == itemSet)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
            if (found) scenes.Add(key);
        }

        List<string> result = new List<string>(scenes);
        result.Sort(System.StringComparer.OrdinalIgnoreCase);
        return result;
    }
}
