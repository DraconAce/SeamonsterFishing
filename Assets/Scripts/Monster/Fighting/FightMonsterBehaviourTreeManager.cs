using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

public class FightMonsterBehaviourTreeManager : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private float delayBeforeFirstBehaviour = 1f;
    [SerializeField] private AbstractMonsterNodeImpl rootNode;
    [SerializeField] private BehaviourTree behaviourTree;
    
    private readonly Dictionary<int, INodeImpl> nodeImplDict = new();
    private NativeHashMap<int, NodeData> nodeDataMap;

    private JobHandle chooseBehaviourJobHandle;
    private NativeArray<int> nextBehaviourID;
    private NativeMultiHashMap<int, ComparableData> comparableDataMapRef;

    private RandomArrayProvider randomArrayProvider;
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
        
        randomArrayProvider = new RandomArrayProvider();
        nextBehaviourID = new NativeArray<int>(1, Allocator.Persistent);
        comparableDataMapRef = new NativeMultiHashMap<int, ComparableData>(0,Allocator.Persistent);

        monsterKI.BehaviourTreeManager = this;
        monsterKI.StopCurrentBehaviourEvent += StopCurrentBehaviour;
        
        waitUntilBehaviourIsInactive = new WaitUntil(() => !IsAnyBehaviourActive);

        CreateNodeImplDict();
        
        CreateNodeDataMap();
        
        StartCoroutine(StartFirstBehaviourRoutine());
    }

    private void StopCurrentBehaviour() => StopCurrentBehaviour(-1);

    private void StopCurrentBehaviour(int nextBehaviourIndex)
    {
        stopCurrentBehaviourRoutine = StartCoroutine(StopCurrentBehaviourRoutine(nextBehaviourIndex));
    }

    private void CreateNodeImplDict()
    {
        var allNodeImplementation = FindObjectsByType<AbstractMonsterNodeImpl>
            (FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<INodeImpl>();

        foreach (var nodeImpl in allNodeImplementation) 
            nodeImplDict.Add(nodeImpl.NodeIndex, nodeImpl);
    }

    private void CreateNodeDataMap()
    {
        nodeDataMap = new NativeHashMap<int, NodeData>(nodeImplDict.Count, Allocator.Persistent);
    }

    private IEnumerator StartFirstBehaviourRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(delayBeforeFirstBehaviour);

            RequestNextBehaviour();
        }
    }

    public INodeImpl GetNodeImplementation(int behaviourIndex) => nodeImplDict[behaviourIndex];

    public INodeImpl GetNodeImplementation(string behaviourName)
    {
        return nodeImplDict.Values.First(x => x.NodeToRepresent.BehaviourName == behaviourName);
    }

    public void RequestNextBehaviour()
    {
        CurrentBehaviourEnded();

        UpdateNodeData();
        
        ScheduleBehaviourTreeJobs();

        updateManager.SubscribeToManualLateUpdate(this);
    }

    private void UpdateNodeData()
    {
        var nodeImplArray = nodeImplDict.Values.ToArray();

        foreach (var nodeImpl in nodeImplArray) 
            nodeDataMap[nodeImpl.NodeIndex] = nodeImpl.RefreshNodeData();
    }

    public void CurrentBehaviourEnded() => CurrentBehaviour = null;

    private IEnumerator StopCurrentBehaviourRoutine(int nextBehaviourIndex)
    {
        yield return waitUntilBehaviourIsInactive;
        
        CurrentBehaviourEnded();
        
        if(nextBehaviourIndex >= 0)
        {
            monsterKI.ForwardActionRequest(nextBehaviourIndex);
            yield break;
        }
        
        RequestNextBehaviour();
    }

    private void ScheduleBehaviourTreeJobs()
    {
        var randomArray = randomArrayProvider.RandomArray;

        comparableDataMapRef.Clear();
        CreateNodeComparableDataLookup(comparableDataMapRef);
        
        var evaluateTreeJob = new EvaluateTreeJob { RootIndex = rootNode.NodeIndex, RandomArray = randomArray, NodeDataMap = nodeDataMap, ComparableDataPointsOfNodes = comparableDataMapRef};
        var evaluateJobHandle = evaluateTreeJob.Schedule();
        
        var chooseBehaviourJob = new ChooseBehaviourJob{RootIndex = rootNode.NodeIndex, NodeDataMap = nodeDataMap, ChosenBehaviourIndex = nextBehaviourID};
        chooseBehaviourJobHandle = chooseBehaviourJob.Schedule(evaluateJobHandle);
    }

    private void CreateNodeComparableDataLookup(NativeMultiHashMap<int, ComparableData> nodeComparableDataLookup)
    {
        foreach(var nodeImpl in nodeImplDict.Values)
        {
            if(nodeImpl.NodeToRepresent.NodeType == NodeType.Action)
                continue;

            var decisionImpl = (AbstractDecisionNodeImpl) nodeImpl;
            
            var comparableDataList = decisionImpl.GatherComparableData();
            
            foreach(var dataPoint in comparableDataList)
                nodeComparableDataLookup.Add(nodeImpl.NodeIndex, dataPoint);
        }
    }

    public void ManualLateUpdate()
    {
        if(chooseBehaviourJobHandle == default) return;
        
        chooseBehaviourJobHandle.Complete();

        ScheduleStartOfNextBehaviour();

        updateManager.UnsubscribeFromManualLateUpdate(this);
    }

    private void ScheduleStartOfNextBehaviour()
    {
        var behaviourIndex = nextBehaviourID[0];
        
        monsterKI.ForwardActionRequest(behaviourIndex);
        CurrentBehaviour = nodeImplDict[behaviourIndex];
    }
    
    public void RequestSpecificBehaviour(int behaviourIndex) => StopCurrentBehaviour(behaviourIndex);

    private void OnDestroy()
    {
        monsterKI.StopCurrentBehaviourEvent -= StopCurrentBehaviour;

        DisposeOfJobData();
    }

    private void DisposeOfJobData()
    {
        randomArrayProvider.Dispose();
        
        if(comparableDataMapRef.IsCreated) comparableDataMapRef.Dispose();
        
        if(nodeDataMap.IsCreated) nodeDataMap.Dispose();

        if (nextBehaviourID.IsCreated) nextBehaviourID.Dispose();

        if (stopCurrentBehaviourRoutine == null) return;

        StopCoroutine(stopCurrentBehaviourRoutine);
    }
}

//Todo: Convert to parallel job
public struct EvaluateTreeJob : IJob
{
    public int RootIndex;
    public NativeArray<Unity.Mathematics.Random> RandomArray;
    public NativeHashMap<int, NodeData> NodeDataMap;
    public NativeMultiHashMap<int, ComparableData> ComparableDataPointsOfNodes;

    public void Execute() => EvaluateNodes();

    private void EvaluateNodes()
    {
        NodeFunctionality.RecursiveEvaluateNodeData(RandomArray[0], RootIndex, ref NodeDataMap, ComparableDataPointsOfNodes);
    }
}

public struct ChooseBehaviourJob : IJob
{
    public int RootIndex;
    public NativeHashMap<int, NodeData> NodeDataMap;
    public NativeArray<int> ChosenBehaviourIndex;

    public void Execute() => ChooseBehaviour();

    private void ChooseBehaviour()
    {
        var numberOfNodesChecked = 0;
        var currentNode = NodeDataMap[RootIndex];
        
        ChosenBehaviourIndex[0] = -1;
        
        while(numberOfNodesChecked <= NodeDataMap.Count())
        {
            numberOfNodesChecked++;
            
            if (currentNode.NodeComparisonData.NodeType == NodeType.Action)
            {
                ChosenBehaviourIndex[0] = currentNode.NodeIndex;
                break;
            }
            
            currentNode = NodeDataMap[currentNode.NextNodeIndex];
        }
    }
}