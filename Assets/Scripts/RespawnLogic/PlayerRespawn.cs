using System;
using UnityEngine;

/// <summary>
/// Attach this to the Player GameObject.
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn")]
    [Tooltip("Starting Respawn Point")]
    public RespawnPoint startingRespawnPoint;

    [Tooltip("How far above the respawn point to start the ground-finding raycast.")]
    public float groundScanStartHeight = 3f;

    [Tooltip("Layers to consider as ground when snapping the player after teleport.")]
    public LayerMask groundMask = ~0;

    [Tooltip("Offset above the detected ground surface to place the player's feet.")]
    public float groundSnapOffset = 0.05f;

    /// <summary>Fired after the player has been teleported. Use this to trigger UI, audio, etc.</summary>
    public event Action OnRespawn;

    RespawnPoint _currentRespawnPoint;

    // Cache for the character controller / rigidbody reset
    CharacterController _cc;
    Rigidbody           _rb;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (startingRespawnPoint != null)
        {
            _currentRespawnPoint = startingRespawnPoint;
            return;
        }

        // Auto-find the closest RespawnPoint if none is assigned
        _currentRespawnPoint = FindClosestRespawnPoint();

        if (_currentRespawnPoint == null)
            Debug.LogWarning("[PlayerRespawn] No RespawnPoint found in the scene. " +
                             "Create a GameObject and add the RespawnPoint component to it.");
    }

    /// <summary>
    /// Teleports the player to the current respawn point.
    /// Called by EnemyStateChase (or any other system) when the player is caught.
    /// </summary>
    public void Respawn()
    {
        if (_currentRespawnPoint == null)
        {
            Debug.LogWarning("[PlayerRespawn] Respawn called but no RespawnPoint is set.");
            return;
        }

        Vector3 spawnBase = _currentRespawnPoint.transform.position;

        // Raycast downward from above the spawn point to find the actual floor surface.
        // This prevents the player from teleporting into or below geometry.
        Vector3 scanOrigin = spawnBase + Vector3.up * groundScanStartHeight;
        Vector3 destination;
        if (Physics.Raycast(scanOrigin, Vector3.down, out RaycastHit groundHit,
                            groundScanStartHeight + 4f, groundMask, QueryTriggerInteraction.Ignore))
        {
            destination = groundHit.point + Vector3.up * groundSnapOffset;
        }
        else
        {
            // Fallback: use the respawn point position directly
            destination = spawnBase + Vector3.up * groundSnapOffset;
            Debug.LogWarning("[PlayerRespawn] Ground not found below respawn point — using raw position.");
        }

        // CharacterController must be disabled around the teleport
        if (_cc != null) _cc.enabled = false;

        transform.SetPositionAndRotation(destination, _currentRespawnPoint.transform.rotation);

        if (_cc != null) _cc.enabled = true;

        // Zero out rigidbody velocity so the player doesn't carry momentum
        if (_rb != null)
        {
            _rb.linearVelocity        = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        OnRespawn?.Invoke();
        Debug.Log($"[PlayerRespawn] Respawned at '{_currentRespawnPoint.pointName}'.");
    }

    /// <summary>
    /// Updates the active respawn point. Called automatically by RespawnPoint
    /// trigger volumes, or manually from other scripts.
    /// </summary>
    public void SetRespawnPoint(RespawnPoint point)
    {
        if (point == null) return;
        _currentRespawnPoint = point;
        Debug.Log($"[PlayerRespawn] Active respawn point set to '{point.pointName}'.");
    }

    public RespawnPoint CurrentRespawnPoint => _currentRespawnPoint;

    RespawnPoint FindClosestRespawnPoint()
    {
        RespawnPoint[] all   = FindObjectsByType<RespawnPoint>(FindObjectsSortMode.None);
        RespawnPoint   best  = null;
        float          bestD = float.MaxValue;

        foreach (RespawnPoint rp in all)
        {
            float d = Vector3.SqrMagnitude(rp.transform.position - transform.position);
            if (d < bestD) { bestD = d; best = rp; }
        }

        return best;
    }
}
