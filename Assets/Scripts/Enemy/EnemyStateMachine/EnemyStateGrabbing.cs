using UnityEngine;

/// <summary>
/// The clerk has caught the player and is dragging them to the front door.
/// The player's FPSController is set "externally driven" so it doesn't fight us —
/// mouse look still works (so the player can watch the indignity), but movement
/// input is ignored and CC is disabled. Each frame we pin the player's transform
/// just in front of the enemy so they're hauled along.
///
/// On arrival at the door we teleport the player just past it, ragdoll them with
/// an outward + upward impulse (the "kick"), then hand off to Returning so the
/// enemy walks back behind their desk instead of camping the entrance.
///
/// Transitions to:  Returning — on arrival + kick.
/// </summary>
public class EnemyStateGrabbing : EnemyState
{
    public override string StateName => "Grabbing";

    FPSController _playerFPS;

    public EnemyStateGrabbing(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        Agent.isStopped        = false;
        Agent.speed            = Enemy.grabSpeed;
        Agent.acceleration     = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.2f;

        _playerFPS = Enemy.target != null ? Enemy.target.GetComponent<FPSController>() : null;
        if (_playerFPS != null) _playerFPS.SetExternallyDriven(true);

        if (Enemy.frontDoor != null)
            Agent.SetDestination(Enemy.frontDoor.position);
    }

    public override void Tick()
    {
        // Drag the player along.
        if (Enemy.target != null)
        {
            Vector3 offset = EnemyTransform.forward * Enemy.dragOffsetForward
                           + Vector3.up * Enemy.dragOffsetUp;
            Enemy.target.position = EnemyTransform.position + offset;
        }

        if (Enemy.frontDoor == null)
        {
            // Front door not configured — fail soft: just release + go back to wander.
            Enemy.GoToWander();
            return;
        }

        float distSqr = (Enemy.frontDoor.position - EnemyTransform.position).sqrMagnitude;
        if (distSqr <= Enemy.kickArrivalRadius * Enemy.kickArrivalRadius)
        {
            KickPlayerOut();
            Enemy.GoToReturning();
        }
    }

    public override void Exit()
    {
        if (_playerFPS != null) _playerFPS.SetExternallyDriven(false);
    }

    void KickPlayerOut()
    {
        if (_playerFPS == null || Enemy.frontDoor == null) return;

        // Plant the player just past the door (outside), keeping their current Y so
        // they don't pop up or sink into the ground.
        Vector3 outside = Enemy.frontDoor.position
                        + Enemy.frontDoor.forward * Enemy.kickPlantDistance;
        outside.y = Enemy.target.position.y;
        Enemy.target.position = outside;

        // Hand control to the ragdoll state with an outward + upward impulse.
        Vector3 kickImpulse = Enemy.frontDoor.forward * Enemy.kickForwardImpulse
                            + Vector3.up           * Enemy.kickUpImpulse;
        _playerFPS.GoToRagdoll(kickImpulse);
    }
}
