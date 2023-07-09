using UnityEngine;

public interface INodeImpl
{
    public AbstractBehaviourTreeNode NodeToRepresent { get; }
    public string GetBehaviourID => NodeToRepresent.BehaviourID;

    void RegisterInBehaviourManager();
    
    public void Execute();
    public void StopBehaviour();
    public void UpdateNodeData();

    //Todo: notify the behaviour tree manager that this behaviour has finished
}