using System;

/// <summary>
/// Manages transitions between EnemyState instances.
/// Plain C# class — not a MonoBehaviour.
/// </summary>
public class EnemyStateMachine
{
    public EnemyState CurrentState    { get; private set; }
    public string     CurrentStateName => CurrentState?.StateName ?? "None";

    /// <summary>Fired after every successful transition (previousState, newState).</summary>
    public event Action<EnemyState, EnemyState> OnStateTransition;

    /// <summary>
    /// Sets the initial state and calls Enter on it.
    /// Must be called before Tick.
    /// </summary>
    public void Initialize(EnemyState initialState)
    {
        if (initialState == null)
            throw new ArgumentNullException(nameof(initialState));

        CurrentState = initialState;
        CurrentState.Enter();
    }

    /// <summary>
    /// Exits the current state, enters the next, and fires OnStateTransition.
    /// Silently ignores null or same-state transitions.
    /// </summary>
    public void TransitionTo(EnemyState nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, CurrentState))
            return;

        EnemyState previous = CurrentState;
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
        OnStateTransition?.Invoke(previous, CurrentState);
    }

    /// <summary>Drives the active state each frame.</summary>
    public void Tick()
    {
        CurrentState?.Tick();
    }
}
