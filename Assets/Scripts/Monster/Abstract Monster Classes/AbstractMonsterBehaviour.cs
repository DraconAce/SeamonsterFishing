using System;
using System.Collections;
using UnityEngine;

public abstract class AbstractMonsterBehaviour : MonoBehaviour
{
    public virtual bool ChangeMonsterStateOnStartBehaviour => false;
    public virtual MonsterState MonsterStateOnBehaviourStart => MonsterState.None;

    private AbstractMonsterState monsterState;
    private MonsterBehaviourProvider monsterBehaviourProvider;
    
    public Coroutine BehaviourRoutine { get; protected set; }

    protected virtual void Start()
    {
        monsterState = GetComponentInParent<AbstractMonsterState>();
        monsterBehaviourProvider = GetComponentInParent<MonsterBehaviourProvider>();

        monsterState.MonsterStateChangedEvent += OnMonsterStateChanged;
    }

    public virtual void StartBehaviour()
    {
        if (monsterBehaviourProvider.IsBehaviourActive(this)) return;
        
        if (!ChangeMonsterStateOnStartBehaviour) return;

        monsterState.CurrentState = MonsterStateOnBehaviourStart;
    }

    private void OnMonsterStateChanged(MonsterState newState)
    {
        if (newState == MonsterStateOnBehaviourStart) return;
        
        InterruptBehaviour();
    }

    protected virtual void InterruptBehaviour()
    {
        if (BehaviourRoutine == null) return;
        
        StopCoroutine(BehaviourRoutine);
    }

    protected virtual void OnDestroy()
    {
        if (BehaviourRoutine == null) return;
        StopCoroutine(BehaviourRoutine);
    }
}