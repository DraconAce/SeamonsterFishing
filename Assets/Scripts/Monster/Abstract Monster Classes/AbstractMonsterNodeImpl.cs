using System;
using Unity.Collections;
using UnityEngine;

public abstract class AbstractMonsterNodeImpl : MonoBehaviour, INodeImpl, IComparableNode
{
    private static int nodeImplIndexCounter;
    
    [RuntimeInitializeOnLoadMethod]
    static void RunOnApplicationStart()
    {
        nodeImplIndexCounter = 0;
    }
 
    [SerializeField] private int priority = 1;
    [SerializeField] private AbstractBehaviourTreeNode nodeToRepresent;
    public AbstractBehaviourTreeNode NodeToRepresent => nodeToRepresent;
    
    private MonsterKI monsterKI;

    protected MonsterKI MonsterKi => monsterKI;

    protected FightMonsterBehaviourTreeManager behaviourTreeManager;

    public int Priority => priority;
    public abstract bool IsNodeExecutable { get; set; }

    public NodeData CurrentNodeData { get; set; }
    
    private int nodeIndex = -1;
    public int NodeIndex => nodeIndex;

    private void Awake() => GetNodeIndex();

    public void GetNodeIndex() => nodeIndex = nodeImplIndexCounter++;

    protected virtual void Start()
    {
        monsterKI = FightMonsterSingleton.instance.MonsterKI;
        MakeSureTreeManagerIsAssigned();

        if (nodeToRepresent.NodeType == NodeType.Decision) return;
        monsterKI.StartBehaviourEvent += OnStartBehaviour;
    }

    protected void MakeSureTreeManagerIsAssigned() 
        => behaviourTreeManager ??= monsterKI.BehaviourTreeManager;

    private void OnStartBehaviour(int requestedIndex)
    {
        if(requestedIndex != NodeIndex) return;
        
        Execute();
    }

    public virtual void BehaviourHasFinished()
    {
        MakeSureTreeManagerIsAssigned();
        
        behaviourTreeManager.TryResetCurrentBehaviour();
    }

    public void Execute() => StartBehaviour();
    protected abstract void StartBehaviour();

    public void Stop() => StopBehaviour();
    protected abstract void StopBehaviour();


    public virtual NodeData RefreshNodeData()
    {
        MakeSureTreeManagerIsAssigned();

        var newNodeData = CollectNodeData();
        newNodeData.NodeComparableData = GetComparableData();
        
        CurrentNodeData = newNodeData;

        return CurrentNodeData;
    }

    public ComparableData GetComparableData()
    {
        return new ComparableData(NodeIndex, IsNodeExecutable, Priority, GetExecutability());
    }

    protected abstract NodeData CollectNodeData();
    
    public abstract float GetExecutability();

    public void TriggerBehaviourDirectly(bool useForceStop = false) => MonsterKi.RequestDirectStartOfBehaviour(NodeIndex, useForceStop);


    protected virtual void OnDestroy()
    {
        if (monsterKI == null || nodeToRepresent.NodeType == NodeType.Action) return;
        monsterKI.StartBehaviourEvent -= OnStartBehaviour;
    }
}