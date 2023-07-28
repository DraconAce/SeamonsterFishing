using System.Collections;
using DG.Tweening;
using UnityEngine;

public abstract class AbstractMonsterBehaviour : AbstractMonsterNodeImpl
{
    [SerializeField] private float timeout = 1f;

    private float currentTimer;

    private Tween timeoutTween;
    private Coroutine behaviourRoutine;
    private Coroutine stopBehaviourRoutine;

    protected FightMonsterState FightMonsterState { get; private set; }

    protected bool IsTimedOut { get; private set; }
    protected abstract MonsterState BehaviourState { get; }

    public override bool IsNodeExecutable
    {
        get => !IsTimedOut; 
        set => IsTimedOut = !value;
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
        
        behaviourRoutine = StartCoroutine(BehaviourRoutine());
    }
    
    private IEnumerator BehaviourRoutine()
    {
        yield return BehaviourRoutineImpl();
        
        behaviourTreeManager.TryResetCurrentBehaviour(this);
        behaviourTreeManager.RequestNextBehaviour();
        
        StartTimeoutTween();
    }

    protected abstract IEnumerator BehaviourRoutineImpl();

    private void StartTimeoutTween()
    {
        if (timeout <= 0) return;
        
        IsTimedOut = true;
        
        timeoutTween = DOVirtual.DelayedCall(timeout, () => IsTimedOut = false);
    }

    protected override void StopBehaviour()
    {
        if(behaviourRoutine != null)
            StopCoroutine(behaviourRoutine);
        
        stopBehaviourRoutine = StartCoroutine(StopBehaviourRoutine());
    }
    
    private IEnumerator StopBehaviourRoutine()
    {
        yield return StopBehaviourRoutineImpl();
        
        MonsterKi.BehaviourTreeManager.TryResetCurrentBehaviour(this);
    }

    protected abstract IEnumerator StopBehaviourRoutineImpl();
    
    protected override NodeData CollectNodeData()
    {
        return new NodeData
        {
            NodeIndex = this.NodeIndex,
            NodeComparisonData = new NodeComparisonData { NodeType = NodeType.Action }
        };
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        timeoutTween?.Kill();
    }
}