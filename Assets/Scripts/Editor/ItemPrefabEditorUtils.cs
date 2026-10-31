using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ItemPrefabEditorUtils
{
    public static bool NeedsRepair(GameObject prefab)
    {
        return HasRootMesh(prefab) || (prefab.GetComponent<BoxCollider>() == null
            && BoxColliderEditorUtils.TryGetLocalBounds(prefab, out _));
    }

    private static bool HasRootMesh(GameObject root)
    {
        return root.GetComponent<MeshFilter>() != null || root.GetComponent<MeshRenderer>() != null
            || root.GetComponent<SkinnedMeshRenderer>() != null;
    }

    public static void Prepare(GameObject prefab, bool alignBottomCenter)
    {
        if (!alignBottomCenter && !NeedsRepair(prefab)) return;
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("A prefab asset is required.");

        Undo.RegisterFullObjectHierarchyUndo(prefab, "Prepare Item Prefab");
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            MoveRootMeshToChild(contents);
            if (alignBottomCenter && BoxColliderEditorUtils.TryGetLocalBounds(contents, out Bounds bounds))
            {
                if (contents.GetComponent<MeshCollider>() != null)
                    throw new InvalidOperationException("Bottom-center alignment requires replacing the root MeshCollider with a BoxCollider first.");
                Vector3 offset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                foreach (Transform child in contents.transform) child.localPosition -= offset;
            }

            if (BoxColliderEditorUtils.TryGetLocalBounds(contents, out _))
            {
                BoxCollider collider = contents.GetComponent<BoxCollider>();
                if (collider == null) collider = contents.AddComponent<BoxCollider>();
                BoxColliderEditorUtils.FitToChildren(collider, false);
            }
            PrefabUtility.SaveAsPrefabAsset(contents, path, out bool saved);
            if (!saved) throw new IOException("Could not save the prepared item prefab.");
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
    }

    private static void MoveRootMeshToChild(GameObject root)
    {
        if (!HasRootMesh(root)) return;
        MeshFilter filter = root.GetComponent<MeshFilter>();
        MeshRenderer renderer = root.GetComponent<MeshRenderer>();
        SkinnedMeshRenderer skinned = root.GetComponent<SkinnedMeshRenderer>();
        if (PrefabUtility.IsPartOfPrefabInstance(root)
            && ((filter != null && PrefabUtility.GetCorrespondingObjectFromSource(filter) != null)
                || (renderer != null && PrefabUtility.GetCorrespondingObjectFromSource(renderer) != null)
                || (skinned != null && PrefabUtility.GetCorrespondingObjectFromSource(skinned) != null)))
            throw new InvalidOperationException("Move the base prefab's root mesh to a child before processing this variant.");

        string name = "Visuals";
        for (int suffix = 1; root.transform.Find(name) != null; suffix++) name = "Visuals " + suffix;
        GameObject visuals = new GameObject(name);
        visuals.transform.SetParent(root.transform, false);
        visuals.layer = root.layer;
        visuals.tag = root.tag;
        GameObjectUtility.SetStaticEditorFlags(visuals, GameObjectUtility.GetStaticEditorFlags(root));
        if (filter != null) EditorUtility.CopySerialized(filter, visuals.AddComponent<MeshFilter>());
        if (renderer != null) EditorUtility.CopySerialized(renderer, visuals.AddComponent<MeshRenderer>());
        if (skinned != null) EditorUtility.CopySerialized(skinned, visuals.AddComponent<SkinnedMeshRenderer>());
        if (renderer != null) UnityEngine.Object.DestroyImmediate(renderer);
        if (skinned != null) UnityEngine.Object.DestroyImmediate(skinned);
        if (filter != null) UnityEngine.Object.DestroyImmediate(filter);
    }
}
