using UnityEngine;

public abstract class AbstractBehaviourTreeNode : ScriptableObject
{
    [SerializeField] private string behaviourID;
    [SerializeField] private AbstractBehaviourTreeNode nextNode;

    public string BehaviourID => behaviourID;
    
    public virtual AbstractBehaviourTreeNode ExecuteNode() => this;
}