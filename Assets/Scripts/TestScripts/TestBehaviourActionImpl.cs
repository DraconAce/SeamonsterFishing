using Unity.Collections;
using UnityEngine;

public class TestBehaviourActionImpl : AbstractMonsterNodeImpl
{
    [SerializeField] private bool isExecutable = true;
    [SerializeField] private float executability = 1f;

    public override bool IsNodeExecutable
    {
        get => isExecutable;
        set => isExecutable = value;
    }

    public override float GetExecutability() => executability;

    protected override void StartBehaviour()
    {
#if UNITY_EDITOR
        Debug.LogFormat("Behaviour {0} started", NodeToRepresent.BehaviourName);
#endif
    }

    protected override void StopBehaviour()
    {
#if UNITY_EDITOR
        Debug.LogFormat("Behaviour {0} stopped", NodeToRepresent.BehaviourName);
#endif
    }

    protected override NodeData CollectNodeData()
    {
        return new NodeData
        {
            NodeIndex = this.NodeIndex,
            NodeComparisonData = new NodeComparisonData { NodeType = NodeType.Action }
        };
    }
}