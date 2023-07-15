using System;
using System.Collections;
using UnityEngine;

public class MonsterKI : Singleton<MonsterKI>
{
    public override bool AddToDontDestroy => false;
    
    public FightMonsterBehaviourTreeManager BehaviourTreeManager { get; set; }

    public event Action<int> StartBehaviourEvent;
    public event Action StopCurrentBehaviourEvent;

    public void ForwardActionRequest(int indexOfRequestedBehaviour) => StartBehaviourEvent?.Invoke(indexOfRequestedBehaviour);
    
    public void TriggerStopCurrentBehaviour() => StopCurrentBehaviourEvent?.Invoke();

    public void RequestDirectStartOfBehaviour(int behaviourIndex) => BehaviourTreeManager.RequestSpecificBehaviour(behaviourIndex);

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (triggerNextBehaviourRoutine == null) return;
        
        StopCoroutine(triggerNextBehaviourRoutine);
    }


    #region Deprecated

    [SerializeField] private BehaviourTree behaviourTree;

    private BehaviourDecisionMaker decisionMaker;

    private AbstractBehaviourTreeNode lastExecutedNode;

    private AbstractBehaviourTreeNode currentlyExecutedNode;

    private WaitUntil waitUntilUnblocked;

    private Coroutine triggerNextBehaviourRoutine;

    public bool BlockNextBehaviour { get; set; }

    public event Action<string> interruptNodeActionNotificationEvent;

    //private void Awake()

    //{

    //    TryGetComponent(out decisionMaker);

        //

        //    //waitUntilUnblocked = new WaitUntil(() => !BlockNextBehaviour);

    //}


        //private void Start()

        //{

        //    //decisionMaker = BehaviourDecisionMaker.instance as BehaviourDecisionMaker;

        //    

        //    //StartBehaviour();

        //}


        public void InterruptCurrentBehaviour(bool scheduleNewBehaviour = false, string followUpAction = "")
    {
        interruptNodeActionNotificationEvent?.Invoke(currentlyExecutedNode.BehaviourName);

        currentlyExecutedNode = null;

        if (!scheduleNewBehaviour) return;
        
        if(followUpAction == "")
            StartBehaviour();
        //else
        //    ForwardActionRequest(followUpAction);
    }

        public void BehaviourEnded() => StartBehaviour();


    private void StartBehaviour()
    {
        if(!BlockNextBehaviour)
        {
            CacheOldAndGetNewBehaviour();
            return;
        }

        triggerNextBehaviourRoutine = StartCoroutine(DeferredTriggerNextBehaviour());
    }

    private IEnumerator DeferredTriggerNextBehaviour()
    {
        yield return waitUntilUnblocked;
        
        CacheOldAndGetNewBehaviour();
    }

    private void CacheOldAndGetNewBehaviour()
    {
        lastExecutedNode = currentlyExecutedNode;

        //currentlyExecutedNode = behaviourTree.RequestNextBehaviour();
    }

    public string ForwardDecisionRequest() => decisionMaker.DecideOnNextBehaviour();


    public bool IsBehaviourActive(string behaviourIDToCheck)
    {
        if (currentlyExecutedNode == null) return false;

        return currentlyExecutedNode.BehaviourName == behaviourIDToCheck;
    }

    #endregion
}