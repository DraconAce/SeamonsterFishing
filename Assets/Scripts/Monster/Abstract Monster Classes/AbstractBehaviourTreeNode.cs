using System;
using UnityEngine;

public abstract class AbstractBehaviourTreeNode : ScriptableObject
{
    [SerializeField] private string behaviourName;
    public string BehaviourName => behaviourName;

    public abstract NodeType NodeType { get; }
    
    public virtual int NumberOfNextNodes => 1;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(name == string.Empty) return;
        
        behaviourName = name;
    }
#endif
}