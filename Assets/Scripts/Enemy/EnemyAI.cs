using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Core enemy controller. Acts as the shared blackboard for all states
/// and owns the EnemyStateMachine. Requires NavMeshAgent and EnemyVision
/// on the same GameObject.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the player transform, or leave empty to auto-find the 'Player' tag.")]
    public Transform target;

    [Header("Wander")]
    public float wanderSpeed     = 2f;
    public float wanderRadius    = 12f;
    public float wanderIdleTime  = 1.5f;
    public float wanderLookSpeed = 30f;

    [Header("Chase")]
    public float chaseBaseSpeed        = 3.5f;
    public float chaseMaxSpeed         = 6.5f;
    public float chaseSpeedBuildUpRate = 2f;
    public float chaseAcceleration     = 10f;
    public float lostSightTimeout      = 1.5f;
    public float catchRadius           = 1.2f;

    [Header("Search")]
    public float searchSpeed            = 2.5f;
    public float searchRadius           = 5f;
    public float searchDuration         = 6f;
    public float searchWaypointInterval = 2f;
    public float searchLookSpeed        = 55f;

    // Shared Blackboard
    [HideInInspector] public Vector3 LastKnownPlayerPosition;
    [HideInInspector] public float   CurrentChaseSpeed;
    [HideInInspector] public bool    PlayerVisible;

    // Component References
    public NavMeshAgent Agent  { get; private set; }
    public EnemyVision  Vision { get; private set; }

    // State Machine
    EnemyStateMachine _stateMachine;
    EnemyStateWander  _wanderState;
    EnemyStateChase   _chaseState;
    EnemyStateSearch  _searchState;

    //Sound
    public EnemySound enemySound { get; private set; }

    void Awake()
    {
        Agent  = GetComponent<NavMeshAgent>();
        Vision = GetComponent<EnemyVision>();

        enemySound = GetComponent<EnemySound>();

        _wanderState = new EnemyStateWander(this);
        _chaseState  = new EnemyStateChase(this);
        _searchState = new EnemyStateSearch(this);

        _stateMachine = new EnemyStateMachine();
        _stateMachine.OnStateTransition += (from, to) =>
        {
            Debug.Log($"[EnemyAI:{name}] {from.StateName} -> {to.StateName}");
        };
    }

    void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        CurrentChaseSpeed = chaseBaseSpeed;
        _stateMachine.Initialize(_wanderState);
    }

    void Update()
    {
        if (target == null) return;

        PlayerVisible = Vision.CanSeeTarget(target);
        if (PlayerVisible)
            LastKnownPlayerPosition = target.position;

        _stateMachine.Tick();
    }

    // Transition Helpers (called by state classes)
    public void GoToWander() => _stateMachine.TransitionTo(_wanderState);
    public void GoToChase()  => _stateMachine.TransitionTo(_chaseState);
    public void GoToSearch() => _stateMachine.TransitionTo(_searchState);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = new Color(1f, 0.92f, 0.02f, 0.08f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, wanderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Catch radius — when the player is inside, they get caught
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, catchRadius);

        if (Application.isPlaying)
        {
            // State / blackboard label above the enemy's head
            string state   = _stateMachine?.CurrentStateName ?? "—";
            string visible = PlayerVisible ? "SEEN" : "lost";
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.2f,
                $"{name}\nState: {state}\nTarget: {visible}\nSpeed: {CurrentChaseSpeed:0.0}");

            // Current NavMesh destination
            if (Agent != null && Agent.hasPath)
            {
                Gizmos.color = new Color(0f, 1f, 0.6f, 0.9f);
                Gizmos.DrawSphere(Agent.destination, 0.18f);
                Gizmos.DrawLine(transform.position, Agent.destination);
            }

            if (_stateMachine?.CurrentStateName == "Search")
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LastKnownPlayerPosition, 0.25f);
                Gizmos.DrawLine(transform.position, LastKnownPlayerPosition);
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Gizmos.DrawWireSphere(LastKnownPlayerPosition, searchRadius);
            }
        }
    }
#endif
}
