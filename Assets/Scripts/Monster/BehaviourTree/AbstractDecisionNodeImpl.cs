using Unity.Collections;
using UnityEngine;

public abstract class AbstractDecisionNodeImpl : AbstractMonsterNodeImpl
{
    [SerializeField] private bool useRandomPriorityMode = true;
    [SerializeField] private NodePriorityMode preferredPriorityMode = NodePriorityMode.Greater;
    
    private DecisionNode DecisionNode => (DecisionNode) NodeToRepresent;
    
    protected override NodeData CollectNodeData()
    {
        var nodeComparisonData = new NodeComparisonData
        {
            NodeType = NodeType.Decision,
            PriorityMode = GetPriorityMode(),
            DataPoints = GatherComparableData()
        };
        
        return new NodeData
        {
            NodeID = new NativeText(BehaviourID, Allocator.Persistent),
            RandomArray = randomArrayProvider.RandomArray,
            NodeComparisonData = nodeComparisonData
        };
    }
    
    private NodePriorityMode GetPriorityMode()
    {
        return !useRandomPriorityMode ? preferredPriorityMode : GetRandomPriorityMode();
    }

    protected virtual NodePriorityMode GetRandomPriorityMode()
    {
        var randomNumber = Random.Range(0, 2);

        return randomNumber == 0 ? NodePriorityMode.Greater : NodePriorityMode.Less;
    }

    protected virtual NativeArray<ComparableData> GatherComparableData()
    {
        var comparableData = new NativeArray<ComparableData>(NodeToRepresent.NumberOfNextNodes, Allocator.Persistent);

        if (NodeToRepresent.NumberOfNextNodes <= 1)
        {
            var nextBehaviourID = DecisionNode.PossibleNextBehaviourIDs[0].BehaviourID;

            comparableData[0] = GetComparableDataOfBehaviour(nextBehaviourID);

            return comparableData;
        }

        for (var index = 0; index < DecisionNode.PossibleNextBehaviourIDs.Length; index++)
        {
            var childNodeID = DecisionNode.PossibleNextBehaviourIDs[index].BehaviourID;
            
            comparableData[index] = GetComparableDataOfBehaviour(childNodeID);
        }

        return comparableData;
    }

    private ComparableData GetComparableDataOfBehaviour(string behaviourID)
    {
        var nextBehaviour = behaviourTreeManager.GetNodeImplementation(behaviourID);

        return nextBehaviour.GetComparableData();
    }
}