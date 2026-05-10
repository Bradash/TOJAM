using UnityEngine;
using UnityEditor;

public class BoxColliderEditorUtils
{
    [MenuItem("CONTEXT/BoxCollider/Fit to Children")]
    private static void FitToChildren(MenuCommand command)
    {
        BoxCollider boxCollider = (BoxCollider)command.context;
        GameObject parent = boxCollider.gameObject;
        
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;


        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (hasBounds)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            else
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
        }

        if (hasBounds)
        {
            Undo.RecordObject(boxCollider, "Fit Box Collider to Children");
            
            boxCollider.center = parent.transform.InverseTransformPoint(bounds.center);
            
            Vector3 worldSize = bounds.size;
            Vector3 localSize = new Vector3(
                worldSize.x / parent.transform.lossyScale.x,
                worldSize.y / parent.transform.lossyScale.y,
                worldSize.z / parent.transform.lossyScale.z
            );
            
            boxCollider.size = localSize;
        }
        else
        {
            Debug.LogWarning("No renderers found in children to fit the BoxCollider to.");
        }
    }
}