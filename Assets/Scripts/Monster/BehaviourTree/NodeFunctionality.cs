using Unity.Collections;
using Random = Unity.Mathematics.Random;

public static class NodeFunctionality
{
    public static void EvaluateNodeData(NodeData nodeData, int threadIndex)
    {
        var comparisonData = nodeData.NodeComparisonData;
        
        var nextNodeID = comparisonData.NodeType == NodeType.Action ? 
            nodeData.NodeID : CompareData(comparisonData, nodeData.RandomArray[threadIndex]);

        nodeData.NextNodeID.Dispose();
        nodeData.NextNodeID = nextNodeID;
    }

    private static NativeText CompareData(NodeComparisonData nodeComparisonData, Random random)
    {
        var executableData = new NativeArray<CompareableData>(nodeComparisonData.DataPoints.Length, Allocator.TempJob);
        
        var totalUsability = CalculateTotalUsabilityAndRemoveUnexecutable(nodeComparisonData, ref executableData);

        var idOfNextNode = GetNextNode(random, executableData, totalUsability);

        executableData.Dispose();

        return idOfNextNode;
    }

    private static float CalculateTotalUsabilityAndRemoveUnexecutable(NodeComparisonData nodeComparisonData, 
        ref NativeArray<CompareableData> executableData)
    {
        var totalUsability = 0f;
        
        for (var i = 0; i < nodeComparisonData.DataPoints.Length; i++)
        {
            var dataPoint = nodeComparisonData.DataPoints[i];

            if (!dataPoint.IsExecutable)
            {
                executableData[i] = default;
                continue;
            }
            
            totalUsability += dataPoint.Usability;
            executableData[i] = dataPoint;
        }

        return totalUsability;
    }

    private static NativeText GetNextNode(Random random, NativeArray<CompareableData> executableData, float totalUsability)
    {
        NativeText idOfNextNode = default;
        
        foreach (var dataPoint in executableData)
        {
            if (!dataPoint.IsExecutable) continue;

            var randomValue = random.NextFloat(0, totalUsability);

            if (randomValue > dataPoint.Usability) continue;

            idOfNextNode.Dispose();
            idOfNextNode = dataPoint.NodeRepID;
            break;
        }

        return idOfNextNode;
    }
}