using UnityEngine;

[CreateAssetMenu(fileName = "Action Behaviour Node", menuName = "Create Action Node", order = 0)]
public class ActionNode : AbstractBehaviourTreeNode
{
    [SerializeField] private AbstractBehaviourTreeNode nextNode;
    public AbstractBehaviourTreeNode NextNode => nextNode;

}