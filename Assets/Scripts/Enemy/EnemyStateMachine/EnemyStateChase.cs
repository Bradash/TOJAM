using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Chase state: the enemy pursues the player directly.
/// Speed ramps up to chaseMaxSpeed while the player is visible
///
/// Transitions to:  Search  — when line of sight is lost for lostSightTimeout.
///                  Wander  — immediately after catching the player (post-respawn).
/// </summary>
public class EnemyStateChase : EnemyState
{
    public override string StateName => "Chase";

    float          _lastSightTime;
    PlayerRespawn  _playerRespawn;
    EnemyThrow     _thrower;

    public EnemyStateChase(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        Agent.isStopped        = false;
        Agent.acceleration     = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.2f;
        _lastSightTime         = Time.time;

        if (_playerRespawn == null && Enemy.target != null)
            _playerRespawn = Enemy.target.GetComponent<PlayerRespawn>();

        if (_thrower == null)
            _thrower = Enemy.GetComponent<EnemyThrow>();

        Agent.SetDestination(Enemy.target.position);
    }

    public override void Tick()
    {
        // ── Catch check ──
        float distSqr = (Enemy.target.position - EnemyTransform.position).sqrMagnitude;
        if (distSqr <= Enemy.catchRadius * Enemy.catchRadius)
        {
            CatchPlayer();
            return;
        }

        // ── Pursuit logic ──
        if (Enemy.PlayerVisible)
        {
            _lastSightTime = Time.time;

            // Ramp speed up toward chaseMaxSpeed while the player is in sight
            Enemy.CurrentChaseSpeed = Mathf.MoveTowards(
                Enemy.CurrentChaseSpeed,
                Enemy.chaseMaxSpeed,
                Enemy.chaseSpeedBuildUpRate * Time.deltaTime
            );

            Agent.SetDestination(Enemy.target.position);
        }
        else
        {
            // Bleed off speed slowly while heading to the last known position
            Enemy.CurrentChaseSpeed = Mathf.MoveTowards(
                Enemy.CurrentChaseSpeed,
                Enemy.chaseBaseSpeed,
                Enemy.chaseSpeedBuildUpRate * 0.4f * Time.deltaTime
            );

            if (Time.time - _lastSightTime > Enemy.lostSightTimeout)
            {
                Enemy.GoToSearch();
                return;
            }

            Agent.SetDestination(Enemy.LastKnownPlayerPosition);
        }

        Agent.speed = Enemy.CurrentChaseSpeed;

        // Try to throw a projectile at the player
        _thrower?.TryThrow();
    }

    public override void Exit() { }

    void CatchPlayer()
    {
        // Trigger the respawn on the player
        if (_playerRespawn != null)
            _playerRespawn.Respawn();
        else
            Debug.LogWarning("[EnemyStateChase] Player caught but no PlayerRespawn component found on target.");

        // Reset the enemy's speed and return to wandering
        Enemy.CurrentChaseSpeed = Enemy.chaseBaseSpeed;
        Enemy.GoToWander();
    }
}
