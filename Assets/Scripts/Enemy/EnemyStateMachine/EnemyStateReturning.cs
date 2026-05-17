using UnityEngine;

/// <summary>
/// The clerk has just ejected the player and is walking back to their desk.
/// Ignores PlayerVisible during this state so they don't immediately re-chase
/// the player they just kicked out (no entrance-camping).
///
/// Transitions to:  Wander — when the desk position is reached.
/// </summary>
public class EnemyStateReturning : EnemyState
{
    public override string StateName => "Returning";

    public EnemyStateReturning(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        Agent.isStopped        = false;
        Agent.speed            = Enemy.returnSpeed;
        Agent.acceleration     = Enemy.chaseAcceleration;
        Agent.stoppingDistance = 0.5f;

        if (Enemy.deskReturn != null)
            Agent.SetDestination(Enemy.deskReturn.position);
        else
        {
            // No desk configured — just go straight back to wandering.
            Enemy.GoToWander();
        }
    }

    public override void Tick()
    {
        if (HasReachedDestination())
        {
            // Optional: face the desk's forward direction before resuming wander.
            if (Enemy.deskReturn != null)
            {
                Vector3 fwd = Enemy.deskReturn.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.001f)
                    EnemyTransform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            }
            Enemy.GoToWander();
        }
    }

    public override void Exit() { }
}
