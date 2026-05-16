using UnityEngine;

/// <summary>
/// One-way collision logic for an enemy throwable. The player's colliders are
/// configured (in FPSController.ConfigurePlayerCollisionFilter) to ignore the
/// Projectile layer, so the player walks straight through balls — no curb,
/// no stumble, no stuck-on-top.
///
/// The ball itself runs an OverlapSphere check each FixedUpdate and, when the
/// player is inside <see cref="pushRadius"/>, sets its horizontal velocity
/// away from the player. Vertical velocity is preserved so the ball can still
/// bounce normally on the floor.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Self-Push")]
    [Tooltip("Distance from the player at which the ball starts rolling itself away.")]
    public float pushRadius = 0.8f;
    [Tooltip("Speed (m/s) the ball rolls away when the player is in range.")]
    public float pushSpeed  = 4f;

    Rigidbody _rb;

    static int  s_playerMask;
    static bool s_masksReady;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        int projectileLayer = LayerMask.NameToLayer("Projectile");
        if (projectileLayer >= 0) gameObject.layer = projectileLayer;

        if (!s_masksReady)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            s_playerMask   = playerLayer >= 0 ? (1 << playerLayer) : 0;
            s_masksReady   = true;
        }
    }

    void FixedUpdate()
    {
        if (_rb.isKinematic || s_playerMask == 0) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pushRadius, s_playerMask, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return;

        Vector3 away = transform.position - hits[0].transform.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) return;

        // Keep the ball's vertical velocity (so bouncing / gravity isn't killed)
        // and override horizontal to roll away.
        Vector3 newVel = away.normalized * pushSpeed;
        newVel.y       = _rb.linearVelocity.y;
        _rb.linearVelocity = newVel;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
#endif
}
