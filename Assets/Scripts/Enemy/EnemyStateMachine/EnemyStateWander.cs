using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Wander state: the enemy roams random NavMesh points within a radius,
/// pauses briefly at each destination while slowly rotating to look around,
/// then picks the next point.
///
/// Transitions to:  Chase  — when the player enters line of sight.
/// </summary>
public class EnemyStateWander : EnemyState
{
    public override string StateName => "Wander";

    bool  _isIdling;
    float _idleUntil;

    public EnemyStateWander(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        Agent.isStopped        = false;
        Agent.speed            = Enemy.wanderSpeed;
        Agent.acceleration     = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.5f;
        _isIdling              = false;
        PickDestination();
    }

    public override void Tick()
    {
        // Highest priority: spotted the player
        if (Enemy.PlayerVisible)
        {
            Enemy.GoToChase();
            return;
        }

        if (_isIdling)
        {
            // Slowly rotate while standing at destination
            // Safe to do while Agent.updateRotation = true because velocity is 0
            EnemyTransform.Rotate(Vector3.up, Enemy.wanderLookSpeed * Time.deltaTime);

            if (Time.time >= _idleUntil)
            {
                _isIdling = false;
                PickDestination();
            }
            return;
        }

        if (HasReachedDestination())
        {
            _isIdling  = true;
            _idleUntil = Time.time + Enemy.wanderIdleTime;
        }
    }

    public override void Exit()
    {
        _isIdling = false;
    }

    void PickDestination()
    {
        // Choose a random point on the NavMesh within wanderRadius
        Vector3 randomDir = Random.insideUnitSphere * Enemy.wanderRadius;
        randomDir.y = 0f;
        Vector3 candidate = EnemyTransform.position + randomDir;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Enemy.wanderRadius, NavMesh.AllAreas))
            Agent.SetDestination(hit.position);
        else
            Agent.SetDestination(EnemyTransform.position); // fallback: stay put
    }
}
