using System;
using UnityEngine;

public abstract class AbstractMonsterState : MonoBehaviour
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

    public bool MonsterCanAttack() => CurrentState is not (MonsterState.Stunned or MonsterState.Reeling or MonsterState.Dead);
}