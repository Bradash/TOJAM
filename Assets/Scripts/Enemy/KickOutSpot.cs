using UnityEngine;

/// <summary>
/// Drop on the GameObject you want the enemy's "frontDoor" transform to point at.
/// Draws an always-visible gizmo: door frame, forward kick arrow, and a simulated
/// ragdoll trajectory preview based on the EnemyAI that references this transform.
/// </summary>
[DisallowMultipleComponent]
public class KickOutSpot : MonoBehaviour
{
#if UNITY_EDITOR
    static readonly Color FrameColor      = new Color(1f, 0.55f, 0f, 1f);
    static readonly Color FrameFaintColor = new Color(1f, 0.55f, 0f, 0.35f);
    static readonly Color ArrowColor      = new Color(1f, 0.3f,  0f, 1f);
    static readonly Color TrajectoryColor = new Color(1f, 0.8f,  0.2f, 0.95f);
    static readonly Color LandingColor    = new Color(1f, 0.2f,  0f, 1f);

    EnemyAI _cachedAI;

    void OnDrawGizmos()
    {
        Vector3 right = transform.right;
        Vector3 up    = transform.up;
        Vector3 fwd   = transform.forward;

        // ── Door frame ──
        Vector3 frameCenter = transform.position + up * 1f;
        Vector3 hw          = right * 0.5f;
        Vector3 hh          = up    * 1f;
        Vector3 tl = frameCenter - hw + hh;
        Vector3 tr = frameCenter + hw + hh;
        Vector3 br = frameCenter + hw - hh;
        Vector3 bl = frameCenter - hw - hh;

        Gizmos.color = FrameColor;
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
        Gizmos.color = FrameFaintColor;
        Gizmos.DrawLine(tl, br);
        Gizmos.DrawLine(tr, bl);

        // ── Kick arrow ──
        Gizmos.color = ArrowColor;
        Vector3 arrowStart = transform.position + up * 0.05f;
        Vector3 arrowTip   = arrowStart + fwd * 2.5f;
        Gizmos.DrawLine(arrowStart, arrowTip);
        Vector3 ah1 = arrowTip - fwd * 0.4f + right * 0.25f;
        Vector3 ah2 = arrowTip - fwd * 0.4f - right * 0.25f;
        Gizmos.DrawLine(arrowTip, ah1);
        Gizmos.DrawLine(arrowTip, ah2);

        // ── Trajectory preview ──
        EnemyAI ai = ResolveOwningAI();
        float fwdImpulse = ai != null ? ai.kickForwardImpulse : 6f;
        float upImpulse  = ai != null ? ai.kickUpImpulse      : 4f;
        float plantDist  = ai != null ? ai.kickPlantDistance  : 1f;

        // Ragdoll mass defaults to 1, so impulse ≈ initial velocity.
        Vector3 trajPos = transform.position + fwd * plantDist + up * 0.6f;
        Vector3 trajVel = fwd * fwdImpulse + Vector3.up * upImpulse;
        float groundY  = transform.position.y - 0.05f;

        Gizmos.color = TrajectoryColor;
        const int   steps = 60;
        const float dt    = 0.05f;
        Vector3 prev = trajPos;
        for (int i = 0; i < steps; i++)
        {
            trajVel += Physics.gravity * dt;
            trajPos += trajVel * dt;
            Gizmos.DrawLine(prev, trajPos);
            prev = trajPos;
            if (trajPos.y <= groundY) break;
        }

        // Landing marker
        Gizmos.color = LandingColor;
        Gizmos.DrawWireSphere(new Vector3(prev.x, groundY, prev.z), 0.25f);

        UnityEditor.Handles.color = FrameColor;
        UnityEditor.Handles.Label(frameCenter + up * 1.25f, "KICK OUT →");
    }

    EnemyAI ResolveOwningAI()
    {
        if (_cachedAI != null && _cachedAI.frontDoor == transform) return _cachedAI;

        EnemyAI[] all = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].frontDoor == transform) { _cachedAI = all[i]; return _cachedAI; }
        }
        _cachedAI = null;
        return null;
    }
#endif
}
