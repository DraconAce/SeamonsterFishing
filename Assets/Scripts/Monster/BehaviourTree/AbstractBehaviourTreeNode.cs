using System;
using UnityEngine;

public abstract class AbstractBehaviourTreeNode : ScriptableObject
{
    [SerializeField] private string behaviourName;
    [SerializeField] private NodeType nodeType;

    public string BehaviourName => behaviourName;
    public NodeType NodeType => nodeType;
    
    public virtual int NumberOfNextNodes => 1;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(name == string.Empty) return;
        
        behaviourName = name;
    }
#endif
}