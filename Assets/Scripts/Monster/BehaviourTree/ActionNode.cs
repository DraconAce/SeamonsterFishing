using UnityEngine;

[CreateAssetMenu(fileName = "Action Behaviour Node", menuName = "BehaviourTree/Create Action Node", order = 0)]
public class ActionNode : AbstractBehaviourTreeNode
{
    public override NodeType NodeType => NodeType.Action;
}