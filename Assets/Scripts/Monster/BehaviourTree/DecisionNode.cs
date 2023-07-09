using UnityEngine;

[CreateAssetMenu(fileName = "Decision Behaviour Node", menuName = "Create Decision Node", order = 0)]
public class DecisionNode : ScriptableObject
{
    [SerializeField] private string[] possibleNextBehaviourIDs;
}