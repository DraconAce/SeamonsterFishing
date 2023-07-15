using UnityEngine;

public interface INodeImpl
{
    public AbstractBehaviourTreeNode NodeToRepresent { get; }
    public NodeData CurrentNodeData { get; set; }
    public int NodeIndex { get; }

    void GetNodeIndex();
    void BehaviourHasFinished();
    NodeData RefreshNodeData();

    public void Execute();
    public void Stop();

    public ComparableData GetComparableData();

    //Todo: notify the behaviour tree manager that this behaviour has finished
}