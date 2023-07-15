using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractMonsterBehaviour : MonoBehaviour, IDecisionRuntimeRep, IMonsterBehaviourDecisionState
{
    [SerializeField] private int priority;
    [SerializeField] private AbstractBehaviourTreeNode nodeToRep;
    
    public virtual bool ChangeMonsterStateOnStartBehaviour => false;
    public virtual MonsterState MonsterStateOnBehaviourStart => MonsterState.None;

    private AbstractMonsterState monsterState;
    private BehaviourNotifier behaviourNotifier;
    private MonsterKI monsterKi;

    private Coroutine behaviourImplRoutine;
    private Coroutine BehaviourRoutine;
    private Coroutine InterruptRoutine;

    private bool behaviourIsPlaying;
    protected bool interruptBehaviourRoutine;
    
    public bool BehaviourIsDone { get; private set; }
    public bool InterruptIsDone { get; private set; } = true;
    public int Priority => priority;

    public WaitUntil WaitForInterruptDone { get; private set; }
    public WaitUntil WaitForCompletionOrInterruption { get; private set; }
    public AbstractBehaviourTreeNode NodeToRepresent => nodeToRep;


    private void Awake()
    {
        WaitForCompletionOrInterruption = new(() => interruptBehaviourRoutine || BehaviourIsDone);
        WaitForInterruptDone = new(() => InterruptIsDone);
    }

    protected virtual void Start()
    {
        behaviourNotifier = BehaviourNotifier.instance as BehaviourNotifier;
        behaviourNotifier.RegisterBehaviour(this);
        
        monsterState = GetComponentInParent<AbstractMonsterState>();
        monsterKi = MonsterKI.instance;

        monsterState.MonsterStateChangedEvent += OnMonsterStateChanged;
    }

    public void TriggerBehaviour()
    {
        if (monsterKi.IsBehaviourActive(nodeToRep.BehaviourName)) return;
        
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

    public virtual bool CanBeExecuted() => true;
    
    public void Execute() => TriggerBehaviour();
}