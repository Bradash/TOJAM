using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Search state: the enemy investigates the last known player position,
/// then scouts nearby waypoints while rotating to scan the area.
/// Resets CurrentChaseSpeed on exit so the next Chase ramp feels fair.
///
/// Transitions to:  Chase   — if the player is spotted at any point.
///                  Wander  — after searchDuration expires with no player found.
/// </summary>
public class EnemyStateSearch : EnemyState
{
    public override string StateName => "Search";

    float _searchEndTime;
    float _nextWaypointTime;
    bool  _reachedLastKnown;

    public EnemyStateSearch(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        Agent.isStopped        = false;
        Agent.speed            = Enemy.searchSpeed;
        Agent.acceleration     = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.5f;

        _searchEndTime    = Time.time + Enemy.searchDuration;
        _nextWaypointTime = float.MaxValue; // wait until we reach the last known position
        _reachedLastKnown = false;

        Agent.SetDestination(Enemy.LastKnownPlayerPosition);
        Enemy.enemySound.suspiciousSound();
    }

    public override void Tick()
    {
        // Always watching for the player
        if (Enemy.PlayerVisible)
        {
            Enemy.GoToChase();
            return;
        }

        if (Time.time >= _searchEndTime)
        {
            Enemy.GoToWander();
            return;
        }

        // Phase 1: walk to the last known player position
        if (!_reachedLastKnown)
        {
            if (HasReachedDestination())
            {
                _reachedLastKnown = true;
                _nextWaypointTime = Time.time + Enemy.searchWaypointInterval;
            }
            return;
        }

        // Phase 2: arrived — rotate and scout nearby waypoints
        if (HasReachedDestination())
        {
            // Rotate in place to scan; safe while Agent velocity is 0
            EnemyTransform.Rotate(Vector3.up, Enemy.searchLookSpeed * Time.deltaTime);

            if (Time.time >= _nextWaypointTime)
                PickSearchWaypoint();
        }
    }

    public override void Exit()
    {
        // Reset speed so the next Chase entry feels like a fresh pursuit
        Enemy.CurrentChaseSpeed = Enemy.chaseBaseSpeed;
    }

    void PickSearchWaypoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * Enemy.searchRadius;
        randomDir.y = 0f;
        Vector3 candidate = Enemy.LastKnownPlayerPosition + randomDir;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Enemy.searchRadius, NavMesh.AllAreas))
        {
            Agent.SetDestination(hit.position);
            _nextWaypointTime = Time.time + Enemy.searchWaypointInterval;
        }
    }
}
