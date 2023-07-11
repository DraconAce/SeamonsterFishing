using UnityEngine;

public interface INodeImpl
{
    public AbstractBehaviourTreeNode NodeToRepresent { get; }
    public string BehaviourID { get; }

    void BehaviourHasFinished();
    
    public void Execute();
    public void Stop();
    public void UpdateNodeData();
    
    public ComparableData GetComparableData();

    //Todo: notify the behaviour tree manager that this behaviour has finished
}