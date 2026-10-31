using UnityEngine;
using UnityEditor;

public class BoxColliderEditorUtils
{
    [MenuItem("CONTEXT/BoxCollider/Fit to Children")]
    private static void FitToChildren(MenuCommand command)
    {
        if (!FitToChildren((BoxCollider)command.context))
            Debug.LogWarning("No renderers found in children to fit the BoxCollider to.");
    }

    public static bool FitToChildren(BoxCollider collider, bool recordUndo = true)
    {
        if (collider == null || !TryGetLocalBounds(collider.gameObject, out Bounds bounds)) return false;
        if (recordUndo) Undo.RecordObject(collider, "Fit Box Collider to Children");
        collider.center = bounds.center;
        collider.size = bounds.size;
        EditorUtility.SetDirty(collider);
        return true;
    }

    public static bool TryGetLocalBounds(GameObject root, out Bounds bounds)
    {
        bounds = new Bounds();
        bool found = false;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is MeshRenderer && (renderer.GetComponent<MeshFilter>() == null
                || renderer.GetComponent<MeshFilter>().sharedMesh == null)) continue;
            if (renderer is SkinnedMeshRenderer skinned && skinned.sharedMesh == null) continue;
            if (renderer is SpriteRenderer sprite && sprite.sprite == null) continue;

            Bounds local = renderer.localBounds;
            Matrix4x4 toRoot = root.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            // Transform each corner directly into root space; world AABBs inflate rotated meshes.
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 point = toRoot.MultiplyPoint3x4(new Vector3(
                    (corner & 1) == 0 ? local.min.x : local.max.x,
                    (corner & 2) == 0 ? local.min.y : local.max.y,
                    (corner & 4) == 0 ? local.min.z : local.max.z));
                if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                else bounds.Encapsulate(point);
            }
        }
        return found;
    }
}
