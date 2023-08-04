using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public abstract class AbstractDecisionNodeImpl : AbstractMonsterNodeImpl
{
    [SerializeField] private bool useRandomPriorityMode = true;
    [SerializeField] private NodePriorityMode preferredPriorityMode = NodePriorityMode.Greater;

    private readonly Dictionary<string, int> possibleNextNodeIndices = new();

    private DecisionNode DecisionNode => (DecisionNode) NodeToRepresent;

    public override bool IsNodeExecutable
    {
        get => true;
        set { }
    }
    
    protected override void Start()
    {
        base.Start();
        
        StartCoroutine(DeferredStart());
    }

    private IEnumerator DeferredStart()
    {
        yield return null;
        yield return null;
        yield return null;
        
        GetNodeImplementationsOfNextNodes();
    }

    private void GetNodeImplementationsOfNextNodes()
    {
        MakeSureTreeManagerIsAssigned();
        
        foreach (var node in DecisionNode.PossibleNextBehaviourNodes)
        {
            var nodeImpl = behaviourTreeManager.GetNodeImplementation(node.BehaviourName);
            
            possibleNextNodeIndices.Add(node.BehaviourName, nodeImpl.NodeIndex);
        }
    }

    protected override NodeData CollectNodeData()
    {
        var nodeComparisonData = new NodeComparisonData
        {
            NodeType = NodeType.Decision,
            PriorityMode = GetPriorityMode()
        };
        
        return new NodeData
        {
            NodeIndex = NodeIndex,
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

    public virtual List<ComparableData> GatherComparableData()
    {
        var comparableData = new List<ComparableData>();

        if (NodeToRepresent.NumberOfNextNodes <= 1)
        {
            var nextBehaviourIndex = possibleNextNodeIndices.Values.ToArray()[0];

            comparableData.Add(GetComparableDataOfBehaviour(nextBehaviourIndex));

            return comparableData;
        }

        foreach (var childNodeIndex in possibleNextNodeIndices.Values)
            comparableData.Add(GetComparableDataOfBehaviour(childNodeIndex));
        
        comparableData.Sort((dataPointA, dataPointB) => dataPointA.Usability.CompareTo(dataPointB.Usability));

        return comparableData;
    }

    private ComparableData GetComparableDataOfBehaviour(int behaviourIndex)
    {
        var nextBehaviour = behaviourTreeManager.GetNodeImplementation(behaviourIndex);

        return nextBehaviour.CurrentNodeData.NodeComparableData;
    }

    protected override void StartBehaviour()
    {
#if UNITY_EDITOR
        Debug.LogErrorFormat("StartBehaviour() called on decision node {0}", NodeToRepresent.BehaviourName);
#endif
    }

    protected override void StopBehaviour()
    {
#if UNITY_EDITOR
        Debug.LogErrorFormat("StartBehaviour() called on decision node {0}", NodeToRepresent.BehaviourName);
#endif
    }
}