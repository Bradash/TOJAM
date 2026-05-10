using UnityEngine;

/// <summary>
/// Handles field-of-view cone and line-of-sight checks for the enemy.
/// Attach on the same GameObject as EnemyAI.
/// </summary>
public class EnemyVision : MonoBehaviour
{
    [Header("Vision")]
    [Tooltip("Maximum detection range in world units.")]
    public float viewDistance = 12f;
    public float soundDistance = 3f;

    [Tooltip("Horizontal cone width in degrees (e.g. 90 = 45 degrees left and right).")]
    [Range(0f, 360f)]
    public float fieldOfView = 90f;

    [Tooltip("Vertical cone height in degrees. Wider than horizontal so the enemy sees up ramps and stairs.")]
    [Range(0f, 180f)]
    public float verticalFieldOfView = 60f;

    [Tooltip("Eye height offset above the transform pivot.")]
    public float eyeHeight = 0.6f;

    [Tooltip("Layers treated as solid obstacles that block line-of-sight (walls, floors, props). Do NOT include the player layer here. Set to Nothing to rely purely on aggro zones.")]
    public LayerMask obstacleMask;

    [Header("Aggro Zones (optional)")]
    [Tooltip("If any zones are assigned, the enemy can only see the player while the player is inside at least one of them.")]
    public EnemyAggroZone[] aggroZones;

    /// <summary>
    /// Returns true when the target is inside the FOV cone and has an
    /// unobstructed line of sight from the enemy's eye position.
    /// </summary>
    public bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 origin   = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = target.position   + Vector3.up * eyeHeight;
        Vector3 toTarget = aimPoint - origin;
        float   distance = toTarget.magnitude;

        if (distance > viewDistance) return false;

        // ── Aggro zone gate ──
        // If zones are configured, the player must be inside one to be detectable.
        if (aggroZones != null && aggroZones.Length > 0)
        {
            bool inAnyZone = false;
            for (int i = 0; i < aggroZones.Length; i++)
            {
                if (aggroZones[i] != null && aggroZones[i].Contains(target.position))
                {
                    inAnyZone = true;
                    break;
                }
            }
            if (!inAnyZone) return false;
        }

        // Within soundDistance the enemy "hears" the player and skips FOV checks,
        // but walls still block detection (raycast below).
        bool withinSoundRange = distance < soundDistance;

        if (!withinSoundRange)
        {
            // ── Horizontal FOV check (XZ plane) ──
            Vector3 forwardFlat   = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Vector3 toTargetFlat  = Vector3.ProjectOnPlane(toTarget,          Vector3.up);
            if (forwardFlat.sqrMagnitude > 0.001f && toTargetFlat.sqrMagnitude > 0.001f)
            {
                if (Vector3.Angle(forwardFlat, toTargetFlat) > fieldOfView * 0.5f)
                    return false;
            }

            // ── Vertical FOV check ──
            float horizontalDist = toTargetFlat.magnitude;
            float verticalAngle  = Mathf.Abs(Mathf.Atan2(toTarget.y, horizontalDist) * Mathf.Rad2Deg);
            if (verticalAngle > verticalFieldOfView * 0.5f)
                return false;
        }

        // ── Wall / obstacle check (always runs, including for sound) ──
        if (Physics.Raycast(origin, toTarget.normalized, distance, obstacleMask,
                            QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

// This is all gizmo stuff and helps you see things with your eyeballs
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 fwd    = transform.forward;

        // ── Live line-of-sight ray to the current target (runtime only) ──
        if (Application.isPlaying)
        {
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null && ai.target != null)
            {
                Vector3 aim = ai.target.position + Vector3.up * eyeHeight;
                Gizmos.color = ai.PlayerVisible ? Color.green : Color.red;
                Gizmos.DrawLine(origin, aim);
                Gizmos.DrawSphere(aim, 0.08f);
            }
        }

        // Horizontal boundary rays
        Quaternion lRot = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
        Quaternion rRot = Quaternion.AngleAxis( fieldOfView * 0.5f, Vector3.up);
        Vector3 lDir    = lRot * fwd * viewDistance;
        Vector3 rDir    = rRot * fwd * viewDistance;

        Gizmos.color = new Color(0f, 0.85f, 1f, 0.9f);
        Gizmos.DrawRay(origin, lDir);
        Gizmos.DrawRay(origin, rDir);

        // Horizontal arc
        const int segments = 32;
        Vector3 prev = origin + lDir;
        for (int i = 1; i <= segments; i++)
        {
            float   t   = (float)i / segments;
            float   ang = Mathf.Lerp(-fieldOfView * 0.5f, fieldOfView * 0.5f, t);
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * fwd;
            Vector3 cur = origin + dir * viewDistance;
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.35f);
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }

        // Sound boundary rays
        Quaternion lSRot = Quaternion.AngleAxis(-360 * 0.5f, Vector3.up);
        Quaternion rSRot = Quaternion.AngleAxis(360 * 0.5f, Vector3.up);
        Vector3 lSDir = lSRot * fwd * soundDistance;
        Vector3 rSDir = rSRot * fwd * soundDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, lSDir);
        Gizmos.DrawRay(origin, rSDir);

        // Sound arc
        Vector3 prevS = origin + lSDir;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = Mathf.Lerp(-360 * 0.5f, 360 * 0.5f, t);
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * fwd;
            Vector3 cur = origin + dir * soundDistance;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(prevS, cur);
            prevS = cur;
        }

        // Vertical boundary rays (up and down)
        Vector3 right    = transform.right;
        Quaternion upRot = Quaternion.AngleAxis(-verticalFieldOfView * 0.5f, right);
        Quaternion dnRot = Quaternion.AngleAxis( verticalFieldOfView * 0.5f, right);
        Vector3 upDir    = upRot * fwd * viewDistance;
        Vector3 dnDir    = dnRot * fwd * viewDistance;

        Gizmos.color = new Color(0f, 0.85f, 1f, 0.9f);
        Gizmos.DrawRay(origin, upDir);
        Gizmos.DrawRay(origin, dnDir);

        // Vertical arc
        prev = origin + upDir;
        for (int i = 1; i <= segments; i++)
        {
            float   t   = (float)i / segments;
            float   ang = Mathf.Lerp(-verticalFieldOfView * 0.5f, verticalFieldOfView * 0.5f, t);
            Vector3 dir = Quaternion.AngleAxis(ang, right) * fwd;
            Vector3 cur = origin + dir * viewDistance;
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.2f);
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }
    }
#endif
}
