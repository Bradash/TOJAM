using System;

/// <summary>
/// Manages transitions between PlayerState instances. Plain C# class —
/// not a MonoBehaviour. Mirrors EnemyStateMachine.
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState     { get; private set; }
    public string      CurrentStateName => CurrentState?.StateName ?? "None";

    /// <summary>Fired after every successful transition (previousState, newState).</summary>
    public event Action<PlayerState, PlayerState> OnStateTransition;

    public void Initialize(PlayerState initialState)
    {
        if (initialState == null)
            throw new ArgumentNullException(nameof(initialState));

        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void TransitionTo(PlayerState nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, CurrentState))
            return;

        PlayerState previous = CurrentState;
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
        OnStateTransition?.Invoke(previous, CurrentState);
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}
