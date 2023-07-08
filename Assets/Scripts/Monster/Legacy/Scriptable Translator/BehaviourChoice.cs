using System;
using UnityEngine;

public class BehaviourChoice : MonoBehaviour, IDecisionRuntimeRep
{
    [SerializeField] private AbstractBehaviourTreeNode nodeToRepresent;

    public IMonsterBehaviourDecisionState MonsterBehaviour { get; private set; }

    private void Awake()
    {
        MonsterBehaviour = GetComponent<IMonsterBehaviourDecisionState>();
        
        CheckIfDecisionMakerInParent();
    }
    
    private void CheckIfDecisionMakerInParent()
    {
        var decisionMaker = GetComponentInParent<BehaviourDecisionMaker>();
        
        if (decisionMaker == null) return;
        
        decisionMaker.RegisterBehaviour(this);
    }

    public AbstractBehaviourTreeNode NodeToRepresent => nodeToRepresent;

    public bool IsChoiceSuitable() => MonsterBehaviour.CanBeExecuted();
}