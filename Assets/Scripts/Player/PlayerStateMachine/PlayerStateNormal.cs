using UnityEngine;

/// <summary>
/// Default playable state: CharacterController-driven walking + mouse look.
/// Reads movement axes, drives footstep audio, applies weight-based slowdown.
///
/// Transitions to:  Ragdoll — when FPSController.GoToRagdoll is invoked.
/// </summary>
public class PlayerStateNormal : PlayerState
{
    public override string StateName => "Normal";

    // Sticks the CC to the floor when grounded — small negative value rather than
    // 0 so a single-frame airborne blip from walking over a tiny step doesn't drop
    // isGrounded. Set just deep enough to keep the CC pinned, not so deep that
    // walking off a real ledge feels delayed.
    const float GroundStickVelocity = -2f;

    float _verticalRotation;
    float _verticalVelocity;

    public PlayerStateNormal(FPSController player) : base(player) { }

    public override void Enter()
    {
        // Normal-mode physics setup: CC drives movement, rigidbody is parked kinematic.
        if (Player.CC != null) Player.CC.enabled = true;
        if (Player.RB != null)
        {
            if (!Player.RB.isKinematic)
            {
                Player.RB.linearVelocity  = Vector3.zero;
                Player.RB.angularVelocity = Vector3.zero;
            }
            Player.RB.isKinematic = true;
        }
        if (Player.RagdollCollider != null) Player.RagdollCollider.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Sync the local vertical-look from whatever angle the camera is at.
        if (Player.PlayerCamera != null)
        {
            float x = Player.PlayerCamera.transform.localEulerAngles.x;
            _verticalRotation = x > 180f ? x - 360f : x;
        }
    }

    public override void Tick()
    {
        if (Time.timeScale == 0f) return;
        HandleMovement();
        HandleRotation();
    }

    public override void Exit()
    {
        if (Player.FootSteps != null) Player.FootSteps.isWalking = false;
    }

    void HandleMovement()
    {
        if (Player.CC == null) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput   = Input.GetAxis("Vertical");
        bool  walking         = horizontalInput != 0f || verticalInput != 0f;

        if (Player.FootSteps != null)
        {
            Player.FootSteps.isWalking = walking;
            if (walking) Player.FootSteps.footPitch = Player.WeightCarried * 0.1f;
        }

        // Horizontal velocity from input, rotated by the player's facing.
        // walkSpeed is multiplied twice to match the legacy feel (the original
        // code accidentally squared it; preserved so existing inspector values
        // still feel right).
        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
        Vector3 horizontalVelocity = PlayerTransform.rotation * inputDir * (Player.WalkSpeed * Player.WalkSpeed);
        float weight = Mathf.Max(0.01f, Player.WeightCarried);
        horizontalVelocity /= weight;

        // Manual vertical handling — this is the slide fix.
        // When grounded we stay pinned (no slide-tangent for gravity to decompose into);
        // when airborne we accumulate real gravity until we land again.
        if (Player.CC.isGrounded)
            _verticalVelocity = GroundStickVelocity;
        else
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;

        Vector3 motion = horizontalVelocity;
        motion.y = _verticalVelocity;
        Player.CC.Move(motion * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (Player.PlayerCamera == null) return;

        float mouseXRotation = Input.GetAxis("Mouse X") * Player.MouseSensitivity;
        PlayerTransform.Rotate(0f, mouseXRotation, 0f);

        float mouseY = Input.GetAxis("Mouse Y") * Player.MouseSensitivity;
        _verticalRotation += GameSettings.InvertY ? mouseY : -mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -Player.UpDownRange, Player.UpDownRange);
        Player.PlayerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }
}
