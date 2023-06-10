using System;
using System.Collections;
using UnityEngine;

public class MonsterKI : Singleton<MonsterKI>
{
    [SerializeField] private BehaviourTree behaviourTree;

    private BehaviourDecisionMaker decisionMaker;
    private AbstractBehaviourTreeNode lastExecutedNode;
    private AbstractBehaviourTreeNode currentlyExecutedNode;

    private WaitUntil waitUntilUnblocked;
    private Coroutine triggerNextBehaviourRoutine;

    public override bool AddToDontDestroy => false;
    public bool BlockNextBehaviour { get; set; }

    public event Action<string> startNodeActionNotificationEvent;
    public event Action<string> interruptNodeActionNotificationEvent;

    private void Awake()
    {
        TryGetComponent(out decisionMaker);

        waitUntilUnblocked = new WaitUntil(() => !BlockNextBehaviour);
    }

    private void Start()
    {
        decisionMaker = BehaviourDecisionMaker.instance as BehaviourDecisionMaker;
        
        StartBehaviour();
    }

    public void InterruptCurrentBehaviour(bool scheduleNewBehaviour = false, string followUpAction = "")
    {
        interruptNodeActionNotificationEvent?.Invoke(currentlyExecutedNode.BehaviourID);

        currentlyExecutedNode = null;

        if (!scheduleNewBehaviour) return;
        
        if(followUpAction == "")
            StartBehaviour();
        else
            ForwardActionRequest(followUpAction);
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

    private void CacheOldAndGetNewBehaviour()
    {
        lastExecutedNode = currentlyExecutedNode;

        currentlyExecutedNode = behaviourTree.RequestNextBehaviour();
    }

    private IEnumerator DeferredTriggerNextBehaviour()
    {
        yield return waitUntilUnblocked;
        
        CacheOldAndGetNewBehaviour();
    }

    public void ForwardActionRequest(string idOfRequestedBehaviour) => startNodeActionNotificationEvent?.Invoke(idOfRequestedBehaviour);

    public string ForwardDecisionRequest() => decisionMaker.DecideOnNextBehaviour();

    public bool IsBehaviourActive(string behaviourIDToCheck)
    {
        if (currentlyExecutedNode == null) return false;

        return currentlyExecutedNode.BehaviourID == behaviourIDToCheck;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (triggerNextBehaviourRoutine == null) return;
        
        StopCoroutine(triggerNextBehaviourRoutine);
    }
}