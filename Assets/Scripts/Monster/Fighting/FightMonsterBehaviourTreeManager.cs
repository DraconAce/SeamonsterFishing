using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class FightMonsterBehaviourTreeManager : MonoBehaviour, IManualUpdateSubscriber
{
    #if UNITY_EDITOR
    [Header("Debug Stuff")] 
    [SerializeField] private bool startWithSpecificBehaviour;
    [SerializeField] private AbstractMonsterBehaviour behaviourToStartWith;
#endif
    
    [Header("Behaviour Tree Setup")]
    [SerializeField] private float delayBeforeFirstBehaviour = 1f;
    [SerializeField] private float backupBehaviourCheckInterval = 5f;
    [SerializeField] private AbstractMonsterNodeImpl rootNode;
    [SerializeField] private AbstractMonsterBehaviour backupBehaviour;
    
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
    private WaitForSeconds backupBehaviourCheckIntervalWait;
    private Coroutine stopCurrentBehaviourRoutine;
    private Coroutine backupBehaviourRoutine;
    
    private bool useDirectBehaviourRequestAfterJobFlag;

    private void Start()
    {
        SetupVariables();

        monsterKI.BehaviourTreeManager = this;
        monsterKI.RequestSpecificBehaviourEvent += RequestSpecificBehaviour;

        CreateNodeImplDict();
        
        CreateNodeDataMap();
        
        StartCoroutine(StartFirstBehaviourRoutine());
    }

    private void SetupVariables()
    {
        updateManager = UpdateManager.instance;
        monsterKI = FightMonsterSingleton.instance.MonsterKI;

        randomArrayProvider = new RandomArrayProvider();
        nextBehaviourID = new NativeArray<int>(1, Allocator.Persistent);
        comparableDataMapRef = new NativeMultiHashMap<int, ComparableData>(0, Allocator.Persistent);

        waitUntilBehaviourIsInactive = new WaitUntil(() => !IsAnyBehaviourActive);
        backupBehaviourCheckIntervalWait = new WaitForSeconds(backupBehaviourCheckInterval);
    }

    private void RequestSpecificBehaviour(int nextBehaviourIndex)
    {
        void AfterBehaviourStopAction()
        {
            TryResetCurrentBehaviour();

            if (nextBehaviourIndex >= 0)
            {
                SetCurrentBehaviour(nextBehaviourIndex);
                monsterKI.ForwardActionRequest(nextBehaviourIndex);
                return;
            }

            RequestNextBehaviour();
        }

        StopCurrentBehaviourAndStartNextBehaviour(AfterBehaviourStopAction);
    }
    
    private void SetCurrentBehaviour(int behaviourIndex) => CurrentBehaviour = nodeImplDict[behaviourIndex];

    private void StopCurrentBehaviourAndStartNextBehaviour(Action afterBehaviourStopAction = null)
    {
        stopCurrentBehaviourRoutine = StartCoroutine(StopCurrentBehaviourRoutine(afterBehaviourStopAction));
    }

    private IEnumerator StopCurrentBehaviourRoutine(Action afterBehaviourStoppedAction = null)
    {
        yield return waitUntilBehaviourIsInactive;
        
        afterBehaviourStoppedAction?.Invoke();
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
        yield return new WaitForSeconds(delayBeforeFirstBehaviour);

        #if UNITY_EDITOR
        if (startWithSpecificBehaviour)
        {
            RequestSpecificBehaviour(behaviourToStartWith.NodeIndex);
            yield break;
        }
        #endif
        
        RequestNextBehaviour();
    }

    public INodeImpl GetNodeImplementation(int behaviourIndex) => nodeImplDict[behaviourIndex];

    public INodeImpl GetNodeImplementation(string behaviourName)
    {
        return nodeImplDict.Values.First(x => x.NodeToRepresent.BehaviourName == behaviourName);
    }

    public void RequestNextBehaviour()
    {
        TryResetCurrentBehaviour();

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

    public void TryResetCurrentBehaviour(INodeImpl behaviourToEndIfActive = null)
    {
        if(behaviourToEndIfActive == null || CurrentBehaviour != behaviourToEndIfActive)
            return;
        
        CurrentBehaviour = null;
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

        if (behaviourIndex == -1)
        {
            backupBehaviourRoutine ??= StartCoroutine(StartBackupBehaviourRoutine());
            return;
        }
        
        StopBackupRoutineIfActive();
        
#if UNITY_EDITOR
        Debug.Log(CurrentBehaviour.NodeToRepresent.BehaviourName);
#endif

        if (useDirectBehaviourRequestAfterJobFlag)
        {
            RequestSpecificBehaviour(behaviourIndex);
            useDirectBehaviourRequestAfterJobFlag = false;
            
            return;
        }

        SetCurrentBehaviour(behaviourIndex);
        monsterKI.ForwardActionRequest(behaviourIndex);
    }

    private IEnumerator StartBackupBehaviourRoutine()
    {
        var backupBehaviourIndex = backupBehaviour.NodeIndex;
        
        while (true)
        {
            var isBackupBehaviourAlreadyActive = CurrentBehaviour != null && CurrentBehaviour.NodeIndex == backupBehaviourIndex;
            
            if(!isBackupBehaviourAlreadyActive) 
                monsterKI.ForwardActionRequest(backupBehaviourIndex);
            
            yield return backupBehaviourCheckIntervalWait;
            
            useDirectBehaviourRequestAfterJobFlag = true;
            RequestNextBehaviour();
        }
    }

    private void StopBackupRoutineIfActive()
    {
        if (backupBehaviourRoutine == null) return;
        
        StopCoroutine(backupBehaviourRoutine);
        backupBehaviourRoutine = null;
    }

    public void StopCurrentBehaviour() => StopCurrentBehaviourAndStartNextBehaviour();

    private void OnDestroy()
    {
        monsterKI.RequestSpecificBehaviourEvent -= RequestSpecificBehaviour;

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

#region Job Implementations
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
#endregion