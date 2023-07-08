public struct NodeData
{
    public string NodeID;
    public string NextNodeID;
    
    public NodeType NodeType;
    public CompareableData[] DataPoints;
    public NodeDataCompareMode CompareMode;
}

public struct CompareableData
{
    public int Priority;
    
    public bool IsExecutable;
    public float ValueToCompare;
}

public enum NodeType
{
    Action,
    Decision
}

public enum NodeDataCompareMode
{
    Greater,
    Less
}