using UnityEngine;

/// <summary>
/// Abstract base for all player states. Mirrors the EnemyState pattern —
/// states get shared access to the FPSController owner / blackboard.
/// </summary>
public abstract class PlayerState
{
    public abstract string StateName { get; }

    protected FPSController Player          { get; }
    protected Transform     PlayerTransform { get; }

    protected PlayerState(FPSController player)
    {
        Player          = player;
        PlayerTransform = player.transform;
    }

    /// <summary>Called once when the state machine enters this state.</summary>
    public virtual void Enter() { }

    /// <summary>Called every frame while this state is active.</summary>
    public virtual void Tick() { }

    /// <summary>Called once when the state machine leaves this state.</summary>
    public virtual void Exit() { }
}
