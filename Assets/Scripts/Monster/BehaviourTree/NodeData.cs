using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;

public struct NodeData
{
    public NativeText NodeID;
    public NativeText NextNodeID;
    
    public NativeArray<Random> RandomArray;
    public NodeComparisonData NodeComparisonData;

    public void Dispose()
    {
        RandomArray.Dispose();
        NodeID.Dispose();
        NextNodeID.Dispose();
        
        for(var i = 0; i < NodeComparisonData.DataPoints.Length; i++)
            NodeComparisonData.DataPoints[i].NodeRepID.Dispose();
        
        NodeComparisonData.DataPoints.Dispose();
    }
}

public struct NodeComparisonData
{
    public NodeType NodeType;
    public NodePriorityMode PriorityMode;
    public NativeArray<ComparableData> DataPoints; //Todo: Maybe use persistent?, Important !!! Dispose of all on destroy otherwise memory leak!!!
}

public enum NodeType
{
    Action,
    Decision
}

//Todo: randomize whether to prioritze nodes with less priority or more
public enum NodePriorityMode
{
    Greater,
    Less
}

public readonly struct ComparableData : IComparableNode
{
    private readonly bool isExecutable;
    private readonly float executability;

    public ComparableData(NativeText nodeRepID, bool isExecutable, int priority, float executability)
    {
        this.executability = executability;
        this.isExecutable = isExecutable;
        
        NodeRepID = nodeRepID;
        Priority = priority;
    }

    public NativeText NodeRepID { get; }
    public int Priority { get; }
    public float Usability => Priority * executability;
    
    public bool IsNodeExecutable() => isExecutable;
    public float GetExecutability() => executability;
}