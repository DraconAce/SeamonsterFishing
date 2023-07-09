using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FightMonsterBehaviourTreeManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeFirstBehaviour = 1f;
    [SerializeField] private BehaviourTree behaviourTree;
    
    private Dictionary<string, INodeImpl> behaviourIDToNodeImplMap = new();
    private NativeArray<NodeData> behaviourIDNodeDataArray;

    public INodeImpl CurrentBehaviour { get; private set; }
    public bool IsAnyBehaviourActive => CurrentBehaviour != null;

    private void Start()
    {
        behaviourIDNodeDataArray = new NativeArray<NodeData>(behaviourIDToNodeImplMap.Count, Allocator.Persistent);
    }

    //private void TriggerAction() => MonsterKI.instance.ForwardActionRequest(BehaviourID);
    public void RequestNextBehaviour()
    {
        foreach (var nodeImpl in behaviourIDToNodeImplMap.Values) 
            nodeImpl.UpdateNodeData();
        
        //start next Behaviour decide job
        //sub to lateupdate and call complete there
        //unsub from late update after accessing data
    }

    private void OnDestroy()
    {
        for(var i = 0; i < behaviourIDNodeDataArray.Length; i++)
            behaviourIDNodeDataArray[i].Dispose();
        
        behaviourIDNodeDataArray.Dispose();
    }
}