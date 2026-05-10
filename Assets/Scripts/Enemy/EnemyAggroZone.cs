using UnityEngine;

/// <summary>
/// Marks an area where an enemy is allowed to aggro on the player.
/// If an EnemyVision references one or more of these, the player must be
/// inside at least one zone to be detected. Uses the attached Collider's
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyAggroZone : MonoBehaviour
{
    [Tooltip("Color used to draw the zone in the scene view.")]
    public Color gizmoColor = new Color(1f, 0.35f, 0.35f, 0.18f);

    Collider _col;
    Collider Col => _col != null ? _col : (_col = GetComponent<Collider>());

    public bool Contains(Vector3 worldPoint)
    {
        return Col != null && Col.bounds.Contains(worldPoint);
    }


// Gizmos so you can see stuff n things
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Collider c = GetComponent<Collider>();
        if (c == null) return;

        Bounds b = c.bounds;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(b.center, b.size);

        Color outline = gizmoColor; outline.a = 1f;
        Gizmos.color = outline;
        Gizmos.DrawWireCube(b.center, b.size);

        UnityEditor.Handles.color = outline;
        UnityEditor.Handles.Label(b.center + Vector3.up * (b.extents.y + 0.2f), $"Aggro Zone: {name}");
    }
#endif
}
