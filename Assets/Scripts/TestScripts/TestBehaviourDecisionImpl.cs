using UnityEngine;

public class TestBehaviourDecisionImpl : AbstractDecisionNodeImpl
{
    [SerializeField] private int priority = 1;
    [SerializeField] private float executability = 1f;
    
    public override int Priority => priority;

    public override float GetExecutability() => executability;
}