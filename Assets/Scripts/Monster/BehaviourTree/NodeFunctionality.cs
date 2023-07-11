using Unity.Collections;
using Random = Unity.Mathematics.Random;

public static class NodeFunctionality
{
    public static void EvaluateNodeData(ref NodeData nodeData, int threadIndex)
    {
        var comparisonData = nodeData.NodeComparisonData;
        
        var nextNodeID = comparisonData.NodeType == NodeType.Action ? 
            nodeData.NodeID : CompareData(comparisonData, nodeData.RandomArray[threadIndex]);

        nodeData.NextNodeID = nextNodeID;
    }

    private static NativeText CompareData(NodeComparisonData nodeComparisonData, Random random)
    {
        var executableData = new NativeArray<ComparableData>(nodeComparisonData.DataPoints.Length, Allocator.Temp);
        
        var totalUsability = CalculateTotalUsabilityAndRemoveUnexecutable(nodeComparisonData, ref executableData);

        var idOfNextNode = GetNextNode(random, executableData, totalUsability, nodeComparisonData.PriorityMode);

        executableData.Dispose();

        return idOfNextNode;
    }

    private static float CalculateTotalUsabilityAndRemoveUnexecutable(NodeComparisonData nodeComparisonData, 
        ref NativeArray<ComparableData> executableData)
    {
        var totalUsability = 0f;
        
        for (var i = 0; i < nodeComparisonData.DataPoints.Length; i++)
        {
            var dataPoint = nodeComparisonData.DataPoints[i];

            if (!dataPoint.IsNodeExecutable())
            {
                executableData[i] = default;
                continue;
            }
            
            totalUsability += dataPoint.Usability;
            executableData[i] = dataPoint;
        }

        return totalUsability;
    }

    private static NativeText GetNextNode(Random random, NativeArray<ComparableData> executableData, float totalUsability, NodePriorityMode priorityMode)
    {
        NativeText idOfNextNode = default;
        
        foreach (var dataPoint in executableData)
        {
            if (!dataPoint.IsNodeExecutable()) continue;

            var randomValue = random.NextFloat(0, totalUsability);

            if (!NodeWasRandomlyChosen(randomValue, dataPoint.Usability, priorityMode)) continue;

            idOfNextNode = dataPoint.NodeRepID;
            break;
        }

        return idOfNextNode;
    }

    private static bool NodeWasRandomlyChosen(float randomValue, float usability, NodePriorityMode priorityMode)
    {
        if(priorityMode == NodePriorityMode.Greater)
            return randomValue < usability;

        return randomValue > usability;
    }
}