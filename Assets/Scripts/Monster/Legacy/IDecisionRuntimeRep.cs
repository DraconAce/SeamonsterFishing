public interface IDecisionRuntimeRep
{
    public AbstractBehaviourTreeNode NodeToRepresent { get; }
    public string GetBehaviourID => NodeToRepresent.BehaviourName;
}