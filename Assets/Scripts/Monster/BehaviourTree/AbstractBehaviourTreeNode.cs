using UnityEngine;

public abstract class AbstractBehaviourTreeNode : ScriptableObject
{
    [SerializeField] private string behaviourID;
    [SerializeField] private NodeType nodeType;

    public string BehaviourID => behaviourID;
    public NodeType NodeType => nodeType;
    
    public virtual int NumberOfNextNodes => 1;
}