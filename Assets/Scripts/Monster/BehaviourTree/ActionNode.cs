using UnityEngine;

[CreateAssetMenu(fileName = "Action Behaviour Node", menuName = "Create Action Node", order = 0)]
public class ActionNode : AbstractBehaviourTreeNode
{
    public override AbstractBehaviourTreeNode ExecuteNode()
    {
        TriggerAction();
        
        return base.ExecuteNode();
    }

    private void TriggerAction() => MonsterKI.instance.ForwardActionRequest(BehaviourID);
}