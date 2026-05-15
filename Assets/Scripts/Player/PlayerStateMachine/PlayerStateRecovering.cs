using UnityEngine;

/// <summary>
/// "Getting up" state — runs after Ragdoll. The rigidbody is parked kinematic,
/// the body lerps smoothly from whatever pose it tumbled into back to upright,
/// and the camera's local rotation lerps to identity. When the lerp finishes
/// the player is ground-snapped and CC control is restored.
///
/// Transitions to:  Normal — once the recovery lerp completes.
/// </summary>
public class PlayerStateRecovering : PlayerState
{
    public override string StateName => "Recovering";

    Quaternion _fromRot;
    Quaternion _toRot;
    Quaternion _fromCamRot;
    float      _startTime;

    public PlayerStateRecovering(FPSController player) : base(player) { }

    public override void Enter()
    {
        // Body stays in place visually; physics is parked so the lerp owns rotation.
        if (Player.RB != null)
        {
            if (!Player.RB.isKinematic)
            {
                Player.RB.linearVelocity  = Vector3.zero;
                Player.RB.angularVelocity = Vector3.zero;
            }
            Player.RB.isKinematic = true;
        }

        // Recovery target: upright, preserving whichever yaw the player was facing
        // before they ragdolled (more predictable than re-deriving from a tumbled
        // forward vector, which goes sideways when the body is upside-down).
        _fromRot    = PlayerTransform.rotation;
        _toRot      = Quaternion.Euler(0f, Player.PreRagdollYaw, 0f);
        _fromCamRot = Player.PlayerCamera != null
            ? Player.PlayerCamera.transform.localRotation
            : Quaternion.identity;
        _startTime  = Time.unscaledTime;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public override void Tick()
    {
        if (Time.timeScale == 0f) return;

        float duration = Mathf.Max(0.01f, Player.RecoveryDuration);
        float k        = Mathf.Clamp01((Time.unscaledTime - _startTime) / duration);
        float eased    = k * k * (3f - 2f * k); // smoothstep

        PlayerTransform.rotation = Quaternion.Slerp(_fromRot, _toRot, eased);

        if (Player.PlayerCamera != null)
            Player.PlayerCamera.transform.localRotation =
                Quaternion.Slerp(_fromCamRot, Quaternion.identity, eased);

        if (k >= 1f)
        {
            SnapToGround();
            if (Player.RagdollCollider != null) Player.RagdollCollider.enabled = false;
            if (Player.CC != null)              Player.CC.enabled              = true;
            Player.GoToNormal();
        }
    }

    public override void Exit() { }

    /// <summary>Probes down for floor and places the player feet on it. Skips if nothing's below.</summary>
    void SnapToGround()
    {
        Vector3 from = PlayerTransform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, 5f,
                            ~0, QueryTriggerInteraction.Ignore))
        {
            float skin = Player.CC != null ? Player.CC.skinWidth : 0.02f;
            PlayerTransform.position = hit.point + Vector3.up * skin;
        }
    }
}
