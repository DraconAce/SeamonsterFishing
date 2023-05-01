using System;

public class MonsterStateManager : Singleton<MonsterStateManager>
{
    private MonsterState currentState;
    public MonsterState CurrentState
    {
        get => currentState;
        set => CheckStateChangeRequest(value);
    }

    public event Action<MonsterState> MonsterStateChangedEvent;

    private void CheckStateChangeRequest(MonsterState requestedChange)
    {
        if (requestedChange == currentState) return;
        
        currentState = requestedChange;
        MonsterStateChangedEvent?.Invoke(currentState);
    }
}