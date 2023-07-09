using Unity.Collections;
using Unity.Mathematics;

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
    public NativeArray<CompareableData> DataPoints; //Todo: Maybe use persistent?, Important !!! Dispose of all on destroy otherwise memory leak!!!
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

public struct CompareableData
{
    public NativeText NodeRepID;
    public bool IsExecutable;
    
    public int Priority; //doesn't change
    public float Executability; //1-100, changes with game situation
    
    public float Usability => Priority * Executability;
}