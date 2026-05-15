using UnityEngine;

/// <summary>
/// Player loses control: CharacterController is disabled, a capsule collider
/// takes over, the rigidbody goes non-kinematic so the body falls and tumbles.
/// Angular + linear damping and a clamped maxAngularVelocity keep the tumble
/// from going spazzy; a small torque (well below the impulse magnitude) gives
/// it character without launching.
///
/// Auto-recovers after FPSController.RagdollDuration by handing off to
/// PlayerStateRecovering, which does the smooth get-up lerp.
/// </summary>
public class PlayerStateRagdoll : PlayerState
{
    public override string StateName => "Ragdoll";

    Vector3 _pendingImpulse;
    float   _recoverAt;

    public PlayerStateRagdoll(FPSController player) : base(player) { }

    /// <summary>Set before transitioning in to apply an impulse on Enter. Cleared after use.</summary>
    public void SetPendingImpulse(Vector3 impulse) => _pendingImpulse = impulse;

    /// <summary>Called by FPSController when ragdoll is re-triggered while already in this state.</summary>
    public void ApplyAdditionalImpulse(Vector3 impulse)
    {
        if (Player.RB != null && !Player.RB.isKinematic)
        {
            Player.RB.AddForce(impulse, ForceMode.Impulse);
            ApplyTumbleTorque(impulse.magnitude);
        }
        _recoverAt = Time.unscaledTime + Player.RagdollDuration;
    }

    public override void Enter()
    {
        if (Player.FootSteps != null) Player.FootSteps.isWalking = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Remember the heading we were facing — Recovering uses this so the
        // player always gets up facing where they started, not wherever
        // physics happened to spin them.
        Player.PreRagdollYaw = PlayerTransform.eulerAngles.y;

        // Hand over to physics.
        if (Player.CC != null)              Player.CC.enabled              = false;
        if (Player.RagdollCollider != null) Player.RagdollCollider.enabled = true;

        if (Player.RB != null)
        {
            Player.RB.isKinematic        = false;
            Player.RB.useGravity         = true;
            Player.RB.linearVelocity     = Vector3.zero;
            Player.RB.angularVelocity    = Vector3.zero;
            Player.RB.constraints        = RigidbodyConstraints.None;

            // Damping so the tumble settles into a final pose instead of spinning
            // forever. maxAngularVelocity caps any extreme spin from low-inertia axes.
            Player.RB.linearDamping      = Player.RagdollLinearDamping;
            Player.RB.angularDamping     = Player.RagdollAngularDamping;
            Player.RB.maxAngularVelocity = Player.RagdollMaxAngularVelocity;

            if (_pendingImpulse.sqrMagnitude > 0f)
            {
                Player.RB.AddForce(_pendingImpulse, ForceMode.Impulse);
                ApplyTumbleTorque(_pendingImpulse.magnitude);
            }
        }
        _pendingImpulse = Vector3.zero;

        _recoverAt = Time.unscaledTime + Player.RagdollDuration;
    }

    public override void Tick()
    {
        if (Time.timeScale == 0f) return;
        if (Player.RagdollDuration > 0f && Time.unscaledTime >= _recoverAt)
            Player.GoToRecovering();
    }

    public override void Exit()
    {
        // PlayerStateRecovering does the heavy lifting (rotation lerp, ground
        // snap, restoring CC). Nothing to do here.
    }

    void ApplyTumbleTorque(float impulseMagnitude)
    {
        if (Player.RB == null) return;
        // Bias the spin axis toward horizontal so the body falls and tumbles
        // forward instead of spinning around its own vertical axis (which looks
        // like an ice skater, not a knockdown).
        Vector3 axis = Random.insideUnitSphere;
        axis.y      *= 0.3f;
        axis        = axis.normalized;
        Player.RB.AddTorque(axis * impulseMagnitude * Player.RagdollTorqueScale,
                            ForceMode.Impulse);
    }
}
