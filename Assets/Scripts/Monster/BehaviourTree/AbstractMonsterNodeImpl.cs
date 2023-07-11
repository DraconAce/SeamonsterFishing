using System;
using Unity.Collections;
using UnityEngine;

public abstract class AbstractMonsterNodeImpl : MonoBehaviour, INodeImpl, IComparableNode
{
    [SerializeField] private AbstractBehaviourTreeNode nodeToRepresent;
    public AbstractBehaviourTreeNode NodeToRepresent => nodeToRepresent;
    
    protected RandomArrayProvider randomArrayProvider;

    private MonsterKI monsterKI;
    protected FightMonsterBehaviourTreeManager behaviourTreeManager;

    public string BehaviourID => nodeToRepresent.BehaviourID;
    
    public int Priority { get; }

    protected virtual void Start()
    {
        monsterKI = MonsterKI.instance;
        MakeSureTreeManagerIsAssigned();

        randomArrayProvider = new RandomArrayProvider();
        
        monsterKI.StartBehaviourEvent += OnStartBehaviour;
    }

    private void MakeSureTreeManagerIsAssigned() 
        => behaviourTreeManager ??= monsterKI.BehaviourTreeManager;

    private void OnStartBehaviour(string requestedID)
    {
        if(requestedID != ((INodeImpl)this).BehaviourID) return;
        
        Execute();
    }

    public virtual void BehaviourHasFinished()
    {
        MakeSureTreeManagerIsAssigned();
        
        behaviourTreeManager.CurrentBehaviourEnded();
    }

    public void Execute() => StartBehaviour();
    protected virtual void StartBehaviour() { }

    public virtual void Stop() => StopBehaviour();
    protected virtual void StopBehaviour() { }


    public virtual void UpdateNodeData()
    {
        MakeSureTreeManagerIsAssigned();

        var updatedData = CollectNodeData();
        
        behaviourTreeManager.UpdateNodeData(((INodeImpl)this).BehaviourID, updatedData);
    }

    public virtual ComparableData GetComparableData()
    {
        return new ComparableData(new NativeText(BehaviourID, Allocator.Persistent), IsNodeExecutable(), Priority, GetExecutability());
    }

    protected virtual NodeData CollectNodeData() => default;

    public virtual bool IsNodeExecutable() => true;

    public virtual float GetExecutability() => 100f;


    protected virtual void OnDestroy()
    {
        randomArrayProvider.Dispose();
    }
}