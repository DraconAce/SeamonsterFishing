using UnityEngine;

public class TestBehaviourDecisionImpl : AbstractDecisionNodeImpl
{
    [SerializeField] private float executability = 1f;

    public override float GetExecutability() => executability;
}