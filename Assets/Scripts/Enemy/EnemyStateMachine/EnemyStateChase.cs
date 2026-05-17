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
        Enemy.enemySound.foundSound();
    }

    public override void Tick()
    {
        // ── Disengage if another NPC is already handling the player ──
        // Prevents this clerk from converging on a target that's already being
        // dragged / ragdolled / recovering. The catch check below still gates
        // the penalty itself, but bailing here also avoids the weird visual of
        // two clerks running to the same dragged player.
        if (IsPlayerAlreadyHandled())
        {
            Enemy.CurrentChaseSpeed = Enemy.chaseBaseSpeed;
            Enemy.GoToWander();
            return;
        }

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
        Enemy.CurrentChaseSpeed = Enemy.chaseBaseSpeed;

        // Belt-and-suspenders: even if Tick missed it, abort the catch (no
        // penalty, no second drag) if the player is already being handled.
        if (IsPlayerAlreadyHandled())
        {
            Enemy.GoToWander();
            return;
        }

        // If the clerk has a door + desk wired, drag the player to the door,
        // kick them out (ragdoll), then walk back. Otherwise fall back to the
        // original instant-respawn behavior.
        bool hasDragSetup = Enemy.frontDoor != null && Enemy.deskReturn != null;

        if (hasDragSetup)
        {
            // The catch still counts toward the lose total — fire the event
            // without performing the teleport, since the grab flow handles relocation.
            if (_playerRespawn != null) _playerRespawn.NotifyCaught();
            Enemy.GoToGrabbing();
        }
        else
        {
            if (_playerRespawn != null) _playerRespawn.Respawn();
            else Debug.LogWarning("[EnemyStateChase] Player caught but no PlayerRespawn component found on target.");
            Enemy.GoToWander();
        }
    }

    /// <summary>
    /// True when the player is already in a "caught" sequence — being dragged by
    /// another NPC (ExternallyDriven), tumbling from a kick (Ragdoll), or getting
    /// up (Recovering). In any of those, this clerk should not catch them again.
    /// </summary>
    bool IsPlayerAlreadyHandled()
    {
        if (Enemy.target == null) return false;
        FPSController fps = Enemy.target.GetComponent<FPSController>();
        if (fps == null) return false;
        if (fps.ExternallyDriven) return true;
        string state = fps.CurrentStateName;
        return state != "Normal" && state != "None";
    }
}
