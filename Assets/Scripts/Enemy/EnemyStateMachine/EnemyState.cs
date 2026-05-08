using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base for all enemy AI states.
/// Gives each state clean access to the owner's components and blackboard.
/// </summary>
public abstract class EnemyState
{
    public abstract string StateName { get; }

    protected EnemyAI      Enemy          { get; }
    protected NavMeshAgent Agent          { get; }
    protected EnemyVision  Vision         { get; }
    protected Transform    EnemyTransform { get; }

    protected EnemyState(EnemyAI enemy)
    {
        Enemy          = enemy;
        Agent          = enemy.Agent;
        Vision         = enemy.Vision;
        EnemyTransform = enemy.transform;
    }

    /// <summary>Called once when the state machine enters this state.</summary>
    public virtual void Enter() { }

    /// <summary>Called every frame while this state is active.</summary>
    public virtual void Tick() { }

    /// <summary>Called once when the state machine leaves this state.</summary>
    public virtual void Exit() { }

    /// <summary>
    /// True when the NavMeshAgent has fully arrived at its current destination.
    /// </summary>
    protected bool HasReachedDestination()
    {
        if (Agent.pathPending) return false;
        if (Agent.remainingDistance > Agent.stoppingDistance) return false;
        return !Agent.hasPath || Agent.velocity.sqrMagnitude < 0.01f;
    }
}
