public class StateMachine
{
    public State CurrentState { get; private set; }

    public void Initialize(State initialState)
    {
        CurrentState = initialState;
        CurrentState?.Enter();
    }

    public void ChangeState(State newState)
    {
        if (newState == null || ReferenceEquals(CurrentState, newState))
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }
}
