using UnityEngine;

public class GenericBehaviourDecisionImpl : AbstractDecisionNodeImpl
{
    [SerializeField] private float executability = 1f;

    public override float GetExecutability() => executability;
}