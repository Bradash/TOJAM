using UnityEngine;

/// <summary>
/// Place this on any GameObject in the scene to mark it as a respawn point.
/// </summary>
public class RespawnPoint : MonoBehaviour
{
    [Tooltip("Optional display name shown in the editor gizmo.")]
    public string pointName = "Spawn";

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Base marker
        Gizmos.color = new Color(0.1f, 1f, 0.3f, 0.85f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.8f);

        // Forward arrow
        Gizmos.color = new Color(0.1f, 1f, 0.3f, 0.5f);
        Gizmos.DrawRay(transform.position + Vector3.up * 0.9f, transform.forward * 0.5f);

        // Label
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2.1f,
            string.IsNullOrEmpty(pointName) ? gameObject.name : pointName
        );
    }
#endif
}
