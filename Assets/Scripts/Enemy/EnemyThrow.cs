using UnityEngine;

/// <summary>
/// Handles ballistic projectile throwing during the Chase state.
/// Call TryThrow() from EnemyStateChase each Tick.
/// Requires: at least one prefab in throwPrefabs with a Rigidbody.
/// </summary>
public class EnemyThrow : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwRange    = 15f;
    public float throwCooldown = 3f;

    [Tooltip("Peak height the projectile reaches above the throw origin.")]
    public float arcHeight = 2.5f;

    [Header("Projectiles")]
    [Tooltip("Prefabs to throw. One is chosen at random each throw.")]
    public GameObject[] throwPrefabs;

    [Tooltip("Auto-found via 'Player' tag if left empty.")]
    public Transform target;

    [Header("Projectile Lifetime")]
    [Tooltip("Seconds before a thrown object is destroyed.")]
    public float projectileLifetime = 6f;

    bool  _canThrow = true;
    float _throwOriginHeight = 1.2f;

    void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    /// <summary>
    /// Called by EnemyStateChase.Tick(). Throws if off cooldown and in range.
    /// </summary>
    public void TryThrow()
    {
        if (!_canThrow) return;
        if (target == null) return;
        if (throwPrefabs == null || throwPrefabs.Length == 0) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > throwRange) return;

        Vector3 origin = transform.position + Vector3.up * _throwOriginHeight;
        Vector3 velocity = CalculateBallisticVelocity(origin, target.position, arcHeight);

        GameObject prefab = throwPrefabs[Random.Range(0, throwPrefabs.Length)];
        GameObject thrown = Instantiate(prefab, origin, Quaternion.identity);

        Rigidbody rb = thrown.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = velocity;
        else
            Debug.LogWarning($"[EnemyThrow] Prefab '{prefab.name}' has no Rigidbody — projectile won't fly.");

        Destroy(thrown, projectileLifetime);

        _canThrow = false;
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    void ResetThrow() => _canThrow = true;

    /// <summary>
    /// Calculates the initial velocity needed for a ballistic arc that peaks at
    /// <paramref name="height"/> above the higher of the two endpoints.
    /// </summary>
    static Vector3 CalculateBallisticVelocity(Vector3 origin, Vector3 target, float height)
    {
        float g    = Mathf.Abs(Physics.gravity.y);
        float peak = Mathf.Max(origin.y, target.y) + height;

        // Time to fall from peak to target
        float tDown = Mathf.Sqrt(2f * (peak - target.y) / g);
        // Time to rise from origin to peak
        float tUp   = Mathf.Sqrt(2f * (peak - origin.y) / g);
        float tTotal = tUp + tDown;

        if (tTotal <= 0f) return Vector3.zero;

        Vector3 vel;
        vel.x = (target.x - origin.x) / tTotal;
        vel.y = Mathf.Sqrt(2f * g * (peak - origin.y));  // upward launch speed
        vel.z = (target.z - origin.z) / tTotal;
        return vel;
    }

    // This is all gizmo stuff and helps you see things with your eyeballs
    #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, throwRange);
        }
    #endif
}
