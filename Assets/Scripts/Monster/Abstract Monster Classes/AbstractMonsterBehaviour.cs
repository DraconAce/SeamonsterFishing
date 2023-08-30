using System.Collections;
using DG.Tweening;
using UnityEngine;

public abstract class AbstractMonsterBehaviour : AbstractMonsterNodeImpl
{
    [SerializeField] private float timeout = 1f;
    [SerializeField] protected MinMaxLimit executability;

    private float currentTimer;

    private Tween timeoutTween;
    private Coroutine behaviourRoutine;
    private Coroutine stopBehaviourRoutine;

    protected FightMonsterState FightMonsterState { get; private set; }

    private bool IsTimedOut { get; set; }
    protected abstract MonsterState BehaviourState { get; }

    private bool isNodeExecutable = true;
    public override bool IsNodeExecutable
    {
        get => isNodeExecutable && !IsTimedOut;
        set => isNodeExecutable = value;
    }

    protected override void Start()
    {
        base.Start();

        FightMonsterState = FightMonsterSingleton.instance.FightState;
    }

    protected override void StartBehaviour()
    {
        if(stopBehaviourRoutine != null)
            StopCoroutine(stopBehaviourRoutine);
        
        FightMonsterState.CurrentState = BehaviourState;
        
        MakeSureTreeManagerIsAssigned();

        behaviourRoutine = StartCoroutine(BehaviourRoutine());
    }
    
    private IEnumerator BehaviourRoutine()
    {
        yield return BehaviourRoutineImpl();

        StartTimeoutTween();
        
        behaviourTreeManager.TryResetCurrentBehaviour(this);
        behaviourTreeManager.RequestNextBehaviour();
    }

    protected abstract IEnumerator BehaviourRoutineImpl();

    private void StartTimeoutTween()
    {
        if (timeout <= 0) return;
        
        timeoutTween?.Kill();
        IsTimedOut = true;
        
        timeoutTween = DOVirtual.DelayedCall(timeout, () => IsTimedOut = false, false);
    }

    protected override void StopBehaviour()
    {
        if(behaviourRoutine != null)
            StopCoroutine(behaviourRoutine);
        
        stopBehaviourRoutine = StartCoroutine(StopBehaviourRoutine());
        
        StartTimeoutTween();
    }
    
    private IEnumerator StopBehaviourRoutine()
    {
        yield return StopBehaviourRoutineImpl();
        
        MonsterKi.BehaviourTreeManager.TryResetCurrentBehaviour(this);
    }

    protected abstract IEnumerator StopBehaviourRoutineImpl();

    public void ForceStopBehaviour()
    {
        if(behaviourRoutine != null) StopCoroutine(behaviourRoutine);
        if(stopBehaviourRoutine != null) StopCoroutine(stopBehaviourRoutine);
        
        ForceStopBehaviourImpl();
        
        StartTimeoutTween();
    }
    
    protected abstract void ForceStopBehaviourImpl();
    
    protected override NodeData CollectNodeData()
    {
        return new NodeData
        {
            NodeIndex = this.NodeIndex,
            NodeComparisonData = new NodeComparisonData { NodeType = NodeType.Action }
        };
    }

    public override float GetExecutability()
    {
        return executability.GetRandomBetweenLimits();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        timeoutTween?.Kill();
    }
}