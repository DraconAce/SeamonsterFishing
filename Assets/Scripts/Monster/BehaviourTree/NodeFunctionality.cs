using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public static class NodeFunctionality
{
    public static void RecursiveEvaluateNodeData(Random random, int nodeIndex, ref NativeHashMap<int, NodeData> nodeDataMap, NativeMultiHashMap<int, ComparableData> comparableDataPointsOfNode)
    {
        var nodeData = nodeDataMap[nodeIndex];
        
        if(nodeData.NodeComparisonData.NodeType == NodeType.Action)
        {
            EvaluateNodeData(random, ref nodeData);
            nodeDataMap[nodeIndex] = nodeData;
            
            return;
        }
        
        var comparableDataPoints = CreateListOfComparableDataPointsForNode(nodeData.NodeIndex, comparableDataPointsOfNode);

        for (var index = 0; index < comparableDataPoints.Length; index++)
        {
            var dataPoint = comparableDataPoints[index];
            
            RecursiveEvaluateNodeData(random, dataPoint.NodeIndexRep, ref nodeDataMap, comparableDataPointsOfNode);
        }

        var executableBefore = nodeData.NodeComparableData.IsNodeExecutable;
        var nodeEvaluationResult = EvaluateNodeData(random, ref nodeData, comparableDataPoints);

        nodeData.NodeComparableData.IsNodeExecutable = executableBefore && nodeEvaluationResult;
        nodeDataMap[nodeIndex] = nodeData;
    }
    
    private static NativeList<ComparableData> CreateListOfComparableDataPointsForNode(int key, NativeMultiHashMap<int, ComparableData> comparableDataPointsOfNode)
    {
        var tempList = new NativeList<ComparableData>(Allocator.Temp);

        comparableDataPointsOfNode.TryGetFirstValue(key, out var dataPoint, out var iterator);
        tempList.Add(dataPoint);

        while (comparableDataPointsOfNode.TryGetNextValue(out dataPoint, ref iterator))
            tempList.Add(dataPoint);
        return tempList;
    }
    
    private static bool EvaluateNodeData(Random random, ref NodeData nodeData, NativeList<ComparableData> comparableData = default)
    {
        var comparisonData = nodeData.NodeComparisonData;
        
        var nextNodeID = comparisonData.NodeType == NodeType.Action ? 
            -1 : CompareData(comparisonData, random, comparableData);

        var isExecutableAfterCompare = nextNodeID != -1 || comparisonData.NodeType == NodeType.Action;

        nodeData.NextNodeIndex = isExecutableAfterCompare ? nextNodeID : -1;
        
        return isExecutableAfterCompare;
    }

    private static int CompareData(NodeComparisonData nodeComparisonData, Random random, NativeArray<ComparableData> comparableData)
    {
        var executableData = new NativeArray<ComparableData>(comparableData.Length, Allocator.Temp);
        
        var highestUsability = CalculateTotalUsabilityAndRemoveUnexecutable(comparableData, ref executableData);

        var indexOfNextNode = GetNextNode(random, executableData, highestUsability, nodeComparisonData.PriorityMode);

        executableData.Dispose();

        return indexOfNextNode;
    }

    private static float CalculateTotalUsabilityAndRemoveUnexecutable(NativeArray<ComparableData> comparableData, 
        ref NativeArray<ComparableData> executableData)
    {
        var highestUsability = 0f;
        
        for (var i = 0; i < comparableData.Length; i++)
        {
            var dataPoint = comparableData[i];

            if (!dataPoint.IsNodeExecutable)
            {
                executableData[i] = default;
                continue;
            }
            
            if(dataPoint.Usability > highestUsability)
                highestUsability = dataPoint.Usability;
            
            executableData[i] = dataPoint;
        }

        return highestUsability;
    }

    private static int GetNextNode(Random random, NativeArray<ComparableData> executableData, float highestUsability, NodePriorityMode priorityMode)
    {
        var indexOfNextNode = -1;
        
        var highestAvailableUsability = 0f;
        var indexOfHighestAvailable = -1;
        
        foreach (var dataPoint in executableData)
        {
            if (!dataPoint.IsNodeExecutable) continue;
            
            if(dataPoint.Usability >= highestAvailableUsability)
            {
                highestAvailableUsability = dataPoint.Usability;
                indexOfHighestAvailable = dataPoint.NodeIndexRep;
            }

            var randomValue = random.NextFloat(0.1f, highestUsability);
            if (!WasNodeRandomlyChosen(randomValue, dataPoint.Usability, priorityMode)) continue;

            indexOfNextNode = dataPoint.NodeIndexRep;
            break;
        }
        
        if(indexOfNextNode == -1) indexOfNextNode = indexOfHighestAvailable;

        return indexOfNextNode;
    }

    private static bool WasNodeRandomlyChosen(float randomValue, float usability, NodePriorityMode priorityMode)
    {
        if(priorityMode == NodePriorityMode.Greater)
            return randomValue <= usability;

        return randomValue >= usability;
    }
}