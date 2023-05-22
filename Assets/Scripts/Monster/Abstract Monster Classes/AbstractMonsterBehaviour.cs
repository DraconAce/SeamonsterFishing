using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractMonsterBehaviour : MonoBehaviour
{
    public virtual bool ChangeMonsterStateOnStartBehaviour => false;
    public virtual MonsterState MonsterStateOnBehaviourStart => MonsterState.None;

    private AbstractMonsterState monsterState;
    private MonsterBehaviourProvider monsterBehaviourProvider;

    private Coroutine behaviourImplRoutine;
    private Coroutine BehaviourRoutine;
    private Coroutine InterruptRoutine;

    private bool behaviourIsPlaying;
    protected bool interruptBehaviourRoutine;
    
    public bool BehaviourIsDone { get; private set; }
    public bool InterruptIsDone { get; private set; } = true;

    public WaitUntil WaitForInterruptDone { get; private set; }
    public WaitUntil WaitForCompletionOrInterruption { get; private set; }

    private void Awake()
    {
        WaitForCompletionOrInterruption = new(() => interruptBehaviourRoutine || BehaviourIsDone);
        WaitForInterruptDone = new(() => InterruptIsDone);
    }

    protected virtual void Start()
    {
        monsterState = GetComponentInParent<AbstractMonsterState>();
        monsterBehaviourProvider = GetComponentInParent<MonsterBehaviourProvider>();

        monsterState.MonsterStateChangedEvent += OnMonsterStateChanged;
    }

    public void TriggerBehaviour()
    {
        if (monsterBehaviourProvider.IsBehaviourActive(this)) return;
        
        BehaviourRoutine = StartCoroutine(StartBehaviourRoutine());

        if (!ChangeMonsterStateOnStartBehaviour) return;
        
        monsterState.CurrentState = MonsterStateOnBehaviourStart;
    }

    private IEnumerator StartBehaviourRoutine()
    {
        behaviourImplRoutine = StartCoroutine(StartBehaviourImpl());
        behaviourIsPlaying = true;

        yield return WaitForCompletionOrInterruption;

        yield return null;
        ResetAfterBehaviourCompleted();
    }

    protected virtual IEnumerator StartBehaviourImpl()
    {
        BehaviourIsDone = true;
        yield break;
    }

    private void OnMonsterStateChanged(MonsterState newState)
    {
        if (newState == MonsterStateOnBehaviourStart) return;
        
        InterruptBehaviour();
    }

    public void InterruptBehaviour()
    {
        if (!behaviourIsPlaying) return;

        InterruptIsDone = false;
        interruptBehaviourRoutine = true;

        if(behaviourImplRoutine != null)
            StopCoroutine(behaviourImplRoutine);

        InterruptRoutine = StartCoroutine(StartInterruptedRoutineImpl());
    }

    protected virtual IEnumerator StartInterruptedRoutineImpl()
    {
        InterruptIsDone = true;

        yield return null;
        yield return null;

        interruptBehaviourRoutine = false;
    }

    private void ResetAfterBehaviourCompleted() => (BehaviourIsDone, interruptBehaviourRoutine, behaviourIsPlaying) = (false, false, false);

    protected virtual void OnDestroy()
    {
        if (BehaviourRoutine != null)
            StopCoroutine(BehaviourRoutine);

        if (InterruptRoutine == null) return;
        
        StopCoroutine(InterruptRoutine);
    }
}