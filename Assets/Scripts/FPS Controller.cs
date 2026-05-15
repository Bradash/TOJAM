using UnityEngine;

/// <summary>
/// Player controller and blackboard for the PlayerStateMachine.
/// States live in Assets/Scripts/Player/PlayerStateMachine/ and read tunables
/// + cached components through the public properties here.
///
/// States:
///   Normal     — CharacterController-driven movement + mouse look (default).
///   Ragdoll    — physics takes over; CC off, capsule collider + rigidbody handle the fall.
///   Recovering — smooth get-up lerp back to upright, then ground-snap → Normal.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange      = 80f;

    [Header("Weight")]
    public float weightCarried;

    [Header("References")]
    [SerializeField] private FootSteps footSteps;

    [Header("Ragdoll")]
    [Tooltip("Seconds in ragdoll before recovery starts. 0 = stay ragdolled until GoToNormal/GoToRecovering is called.")]
    [SerializeField] private float ragdollDuration = 2.5f;
    [Tooltip("Seconds the get-up lerp takes. The body smoothly rotates upright and the camera lerps to neutral.")]
    [SerializeField] private float recoveryDuration = 0.45f;

    [Header("Ragdoll Physics Tuning")]
    [Tooltip("Linear damping while ragdolling — higher = body settles faster after the impulse.")]
    [SerializeField] private float ragdollLinearDamping = 0.4f;
    [Tooltip("Angular damping while ragdolling — higher = spin bleeds off faster.")]
    [SerializeField] private float ragdollAngularDamping = 4f;
    [Tooltip("Cap on angular velocity (rad/s). Prevents extreme spins around the long axis.")]
    [SerializeField] private float ragdollMaxAngularVelocity = 9f;
    [Tooltip("Random torque applied on ragdoll entry, as a fraction of the impulse magnitude. Lower = less chaotic spin.")]
    [SerializeField] private float ragdollTorqueScale = 0.15f;

    [Header("Debug")]
    [Tooltip("Press to test-ragdoll. Set to None to disable.")]
    [SerializeField] private KeyCode debugRagdollKey = KeyCode.R;
    [Tooltip("Forward impulse strength when the debug key fires.")]
    [SerializeField] private float debugImpulseForward = 2f;
    [Tooltip("Upward impulse strength when the debug key fires.")]
    [SerializeField] private float debugImpulseUp      = 3f;

    // ── Blackboard access for states ──
    public float WalkSpeed                  => walkSpeed;
    public float MouseSensitivity           => mouseSensitivity;
    public float UpDownRange                => upDownRange;
    public float WeightCarried              => weightCarried;
    public float RagdollDuration            => ragdollDuration;
    public float RecoveryDuration           => recoveryDuration;
    public float RagdollLinearDamping       => ragdollLinearDamping;
    public float RagdollAngularDamping      => ragdollAngularDamping;
    public float RagdollMaxAngularVelocity  => ragdollMaxAngularVelocity;
    public float RagdollTorqueScale         => ragdollTorqueScale;
    public FootSteps FootSteps              => footSteps;

    /// <summary>Yaw the player was facing when they ragdolled. Recovering reads this to restore heading.</summary>
    public float PreRagdollYaw { get; set; }

    public CharacterController CC               { get; private set; }
    public Rigidbody           RB               { get; private set; }
    public CapsuleCollider     RagdollCollider  { get; private set; }
    public Camera              PlayerCamera     { get; private set; }

    // ── State machine ──
    PlayerStateMachine    _stateMachine;
    PlayerStateNormal     _normalState;
    PlayerStateRagdoll    _ragdollState;
    PlayerStateRecovering _recoveringState;
    public string CurrentStateName => _stateMachine?.CurrentStateName ?? "None";

    void Awake()
    {
        CC           = GetComponent<CharacterController>();
        PlayerCamera = Camera.main;

        RB = GetComponent<Rigidbody>();
        if (RB == null) RB = gameObject.AddComponent<Rigidbody>();
        RB.isKinematic            = true;
        RB.interpolation          = RigidbodyInterpolation.Interpolate;
        RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        RagdollCollider = GetComponent<CapsuleCollider>();
        if (RagdollCollider == null)
        {
            RagdollCollider = gameObject.AddComponent<CapsuleCollider>();
            if (CC != null)
            {
                RagdollCollider.center = CC.center;
                RagdollCollider.height = CC.height;
                RagdollCollider.radius = CC.radius;
            }
        }
        RagdollCollider.enabled = false;

        _normalState     = new PlayerStateNormal(this);
        _ragdollState    = new PlayerStateRagdoll(this);
        _recoveringState = new PlayerStateRecovering(this);
        _stateMachine    = new PlayerStateMachine();
    }

    void Start()
    {
        if (weightCarried <= 0f) weightCarried = 1f;
        Cursor.lockState = CursorLockMode.Locked;

        ApplySettings();
        GameSettings.Changed += ApplySettings;

        _stateMachine.Initialize(_normalState);
    }

    void OnDestroy()
    {
        GameSettings.Changed -= ApplySettings;
    }

    void Update()
    {
        if (debugRagdollKey != KeyCode.None && Input.GetKeyDown(debugRagdollKey))
            GoToRagdoll(transform.forward * debugImpulseForward + Vector3.up * debugImpulseUp);

        _stateMachine.Tick();
    }

    void ApplySettings()
    {
        mouseSensitivity = GameSettings.MouseSensitivity;
    }

    // ── Public transitions ──

    public void GoToNormal()     => _stateMachine.TransitionTo(_normalState);
    public void GoToRecovering() => _stateMachine.TransitionTo(_recoveringState);

    public void GoToRagdoll() => GoToRagdoll(Vector3.zero);

    /// <summary>Ragdoll the player with an initial impulse (e.g. knockback from a projectile).
    /// Safe to call while already ragdolling — applies the impulse and resets the recovery timer.</summary>
    public void GoToRagdoll(Vector3 impulse)
    {
        if (ReferenceEquals(_stateMachine.CurrentState, _ragdollState))
        {
            _ragdollState.ApplyAdditionalImpulse(impulse);
            return;
        }
        _ragdollState.SetPendingImpulse(impulse);
        _stateMachine.TransitionTo(_ragdollState);
    }
}
