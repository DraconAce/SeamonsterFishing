using UnityEngine;

[CreateAssetMenu(fileName = "Behaviour Tree", menuName = "Create Behaviour Tree", order = 0)]
public class BehaviourTree : ScriptableObject
{
    [SerializeField] private AbstractBehaviourTreeNode rootNode;
    
    public AbstractBehaviourTreeNode RootNode => rootNode;
}