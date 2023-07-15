using Unity.Collections;
using Unity.Mathematics;

public struct NodeData
{
    public int NodeIndex;
    public int NextNodeIndex;
    
    public NodeComparisonData NodeComparisonData;
    public ComparableData NodeComparableData;
}

public struct NodeComparisonData
{
    public NodeType NodeType;
    public NodePriorityMode PriorityMode;
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

public struct ComparableData : IComparableNode
{
    private readonly float executability;

    public ComparableData(int nodeIndexRep, bool isExecutable, int priority, float executability)
    {
        this.executability = executability;
        this.IsNodeExecutable = isExecutable;
        
        NodeIndexRep = nodeIndexRep;
        Priority = priority;
    }

    public int NodeIndexRep { get; }
    public int Priority { get; }
    public float Usability => Priority * executability;
    
    public bool IsNodeExecutable { get; set; }

    public float GetExecutability() => executability;
}