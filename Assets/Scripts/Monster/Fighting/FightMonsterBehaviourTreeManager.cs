using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class FightMonsterBehaviourTreeManager : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private float delayBeforeFirstBehaviour = 1f;
    [SerializeField] private BehaviourTree rootOfTree;
    
    private readonly Dictionary<string, INodeImpl> nodeImplDict = new();
    private NativeHashMap<NativeText, NodeData> nodeDataMap;
    
    private JobHandle chooseBehaviourJobHandle;
    private NativeArray<NativeText> nextBehaviourID;

    private UpdateManager updateManager;
    private MonsterKI monsterKI;
    
    public INodeImpl CurrentBehaviour { get; private set; }
    public bool IsAnyBehaviourActive => CurrentBehaviour != null;
    
    private WaitUntil waitUntilBehaviourIsInactive;
    private Coroutine stopCurrentBehaviourRoutine;

    private void Start()
    {
        updateManager = UpdateManager.instance;
        monsterKI = MonsterKI.instance;
        
        monsterKI.BehaviourTreeManager = this;
        monsterKI.StopCurrentBehaviourEvent += StopCurrentBehaviour;
        
        waitUntilBehaviourIsInactive = new WaitUntil(() => !IsAnyBehaviourActive);

        CreateNodeImplDict();
        
        CreateNodeDataMap();
        
        StartCoroutine(StartFirstBehaviourRoutine());
    }

    private void StopCurrentBehaviour() => StopCurrentBehaviour("");

    private void StopCurrentBehaviour(string nextBehaviour)
    {
        stopCurrentBehaviourRoutine = StartCoroutine(StopCurrentBehaviourRoutine(nextBehaviour));
    }

    private void CreateNodeImplDict()
    {
        var allNodeImplementation = FindObjectsByType<AbstractMonsterBehaviour>
            (FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<INodeImpl>();

        foreach (var nodeImpl in allNodeImplementation) 
            nodeImplDict.Add(nodeImpl.BehaviourID, nodeImpl);
    }

    private void CreateNodeDataMap()
    {
        nodeDataMap = new NativeHashMap<NativeText, NodeData>(nodeImplDict.Count, Allocator.Persistent);
    }

    private IEnumerator StartFirstBehaviourRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFirstBehaviour);
        
        RequestNextBehaviour();
    }
    
    public void UpdateNodeData(string behaviourID, NodeData nodeData)
    {
        var tempNativeID = new NativeText(behaviourID, Allocator.Temp);
        
        var oldData = nodeDataMap[tempNativeID];
        
        nodeDataMap[tempNativeID] = nodeData;
        
        oldData.Dispose();
        tempNativeID.Dispose();
    }
    
    public INodeImpl GetNodeImplementation(string behaviourID) => nodeImplDict[behaviourID];

    public void RequestNextBehaviour()
    {
        CurrentBehaviourEnded();
        
        foreach (var nodeImpl in nodeImplDict.Values) 
            nodeImpl.UpdateNodeData();
        
        ScheduleBehaviourTreeJobs();

        updateManager.SubscribeToManualLateUpdate(this);
    }

    public void CurrentBehaviourEnded() => CurrentBehaviour = null;

    private IEnumerator StopCurrentBehaviourRoutine(string nextBehaviour)
    {
        yield return waitUntilBehaviourIsInactive;
        
        CurrentBehaviourEnded();
        
        if(nextBehaviour != string.Empty)
        {
            monsterKI.ForwardActionRequest(nextBehaviour);
            yield break;
        }
        
        RequestNextBehaviour();
    }

    private void ScheduleBehaviourTreeJobs()
    {
        var evaluateTreeJob = new EvaluateTreeJob(nodeDataMap);
        var evaluateJobHandle = evaluateTreeJob.Schedule();

        nextBehaviourID = new NativeArray<NativeText>(1, Allocator.TempJob);
        var rootBehaviourID = new NativeText(rootOfTree.RootNode.BehaviourID, Allocator.TempJob);

        var chooseBehaviourJob = new ChooseBehaviourJob(rootBehaviourID, nodeDataMap, nextBehaviourID);
        chooseBehaviourJobHandle = chooseBehaviourJob.Schedule(evaluateJobHandle);
    }

    public void ManualLateUpdate()
    {
        if(chooseBehaviourJobHandle == default) return;
        
        chooseBehaviourJobHandle.Complete();

        ScheduleStartOfNextBehaviour();

        updateManager.UnsubscribeFromManualLateUpdate(this);
        nextBehaviourID.Dispose();
    }

    private void ScheduleStartOfNextBehaviour()
    {
        var behaviourID = nextBehaviourID[0].ToString();
        monsterKI.ForwardActionRequest(behaviourID);
        CurrentBehaviour = nodeImplDict[behaviourID];
    }
    
    public void RequestSpecificBehaviour(string behaviourID) => StopCurrentBehaviour(behaviourID);

    private void OnDestroy()
    {
        monsterKI.StopCurrentBehaviourEvent -= StopCurrentBehaviour;

        DisposeOfJobData();
    }

    private void DisposeOfJobData()
    {
        nodeDataMap.Dispose();

        if (nextBehaviourID.IsCreated)
            nextBehaviourID.Dispose();

        if (stopCurrentBehaviourRoutine == null) return;

        StopCoroutine(stopCurrentBehaviourRoutine);
    }
}

//Todo: Convert to parallel job
public struct EvaluateTreeJob : IJob
{
    private NativeHashMap<NativeText, NodeData> nodeDataMap;

    public EvaluateTreeJob(NativeHashMap<NativeText, NodeData> nodeDataMap)
    {
        this.nodeDataMap = nodeDataMap;
    }

    public void Execute() => EvaluateNodes();

    private void EvaluateNodes()
    {
        var nodeKeyArray = nodeDataMap.GetKeyArray(Allocator.Temp);
        
        for (var index = 0; index < nodeKeyArray.Length; index++)
        {
            var key = nodeKeyArray[index];
            var node = nodeDataMap[key];
            
            NodeFunctionality.EvaluateNodeData(ref node, index);
            
            nodeDataMap[key] = node;
        }
    }
}

public struct ChooseBehaviourJob : IJob
{
    private readonly NativeText rootID;
    private NativeHashMap<NativeText, NodeData> nodeDataMap;
    private NativeArray<NativeText> chosenBehaviourID;
    
    public ChooseBehaviourJob(NativeText rootID, NativeHashMap<NativeText, NodeData> nodeDataMap, NativeArray<NativeText> chosenBehaviourID)
    {
        this.rootID = rootID;
        this.nodeDataMap = nodeDataMap;
        this.chosenBehaviourID = chosenBehaviourID;
    }

    public void Execute() => ChooseBehaviour();

    private void ChooseBehaviour()
    {
        var numberOfNodesChecked = 0;
        var currentNode = nodeDataMap[rootID];
        
        while(numberOfNodesChecked < nodeDataMap.Count())
        {
            if (currentNode.NodeComparisonData.NodeType == NodeType.Action)
            {
                chosenBehaviourID[0] = currentNode.NodeID;
                break;
            }
            
            currentNode = nodeDataMap[currentNode.NextNodeID];
            numberOfNodesChecked++;
        }
    }
}