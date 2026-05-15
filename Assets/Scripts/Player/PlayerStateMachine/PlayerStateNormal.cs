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

    float _verticalRotation;

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

        float horizontalInput = Input.GetAxis("Horizontal") * Player.WalkSpeed;
        float verticalInput   = Input.GetAxis("Vertical")   * Player.WalkSpeed;

        if (Player.FootSteps != null)
        {
            if (horizontalInput != 0f || verticalInput != 0f)
            {
                Player.FootSteps.isWalking = true;
                Player.FootSteps.footPitch = Player.WeightCarried * 0.1f;
            }
            else
            {
                Player.FootSteps.isWalking = false;
            }
        }

        Vector3 speed = new Vector3(horizontalInput, 0f, verticalInput) * Player.WalkSpeed;
        speed = PlayerTransform.rotation * speed;
        float weight = Mathf.Max(0.01f, Player.WeightCarried);
        Player.CC.SimpleMove(speed / weight);
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
