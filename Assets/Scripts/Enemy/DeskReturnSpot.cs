using UnityEngine;

/// <summary>
/// Drop on the GameObject you want the enemy's "deskReturn" transform to point at.
/// Draws an always-visible gizmo: small desk box + forward arrow showing which way
/// the clerk will face after returning.
/// </summary>
[DisallowMultipleComponent]
public class DeskReturnSpot : MonoBehaviour
{
#if UNITY_EDITOR
    static readonly Color BoxColor   = new Color(0.3f, 0.8f, 1f, 1f);
    static readonly Color FillColor  = new Color(0.3f, 0.8f, 1f, 0.15f);
    static readonly Color ArrowColor = new Color(0.2f, 0.6f, 1f, 1f);

    void OnDrawGizmos()
    {
        Vector3 right = transform.right;
        Vector3 fwd   = transform.forward;

        // Desk-shaped wire box just behind the marker.
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + Vector3.up * 0.4f,
            transform.rotation,
            Vector3.one);

        Vector3 size = new Vector3(1.2f, 0.8f, 0.6f);
        Gizmos.color = FillColor;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = BoxColor;
        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = prevMatrix;

        // Forward arrow showing facing direction post-return.
        Gizmos.color = ArrowColor;
        Vector3 arrowStart = transform.position + Vector3.up * 0.05f;
        Vector3 arrowTip   = arrowStart + fwd * 1.2f;
        Gizmos.DrawLine(arrowStart, arrowTip);
        Vector3 ah1 = arrowTip - fwd * 0.25f + right * 0.15f;
        Vector3 ah2 = arrowTip - fwd * 0.25f - right * 0.15f;
        Gizmos.DrawLine(arrowTip, ah1);
        Gizmos.DrawLine(arrowTip, ah2);

        UnityEditor.Handles.color = BoxColor;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.0f, "Desk Return");
    }
#endif
}
