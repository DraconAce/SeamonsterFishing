using UnityEngine;

[CreateAssetMenu(fileName = "Decision Behaviour Node", menuName = "Create Decision Node", order = 0)]
public class DecisionNode : AbstractBehaviourTreeNode
{
    [SerializeField] private AbstractBehaviourTreeNode[] possibleNextBehaviours;
    public AbstractBehaviourTreeNode[] PossibleNextBehaviourIDs => possibleNextBehaviours;

    public override int NumberOfNextNodes => possibleNextBehaviours.Length;
}