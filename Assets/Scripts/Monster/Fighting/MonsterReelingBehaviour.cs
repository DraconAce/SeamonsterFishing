using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class MonsterReelingBehaviour : AbstractMonsterBehaviour
{
    [Header("Reeling Implementation")] 
    [SerializeField] private int minNumberAttacksNeededUntilNextReeling = 3;
    [SerializeField] private float baseReelingExecutability = 55f;

    [Header("Animations")] 
    
    [Header("Start Reeling")]
    [SerializeField] private string reelingStartedTrigger;
    [SerializeField] private float triggerReelingStartAnimationDelay = 1.5f;
    [SerializeField] private float triggerReelingGameStateChangeDelay = 1.5f;
    [SerializeField] private float diveDuration = 2f;
    [SerializeField] private Ease diveEase = Ease.InOutSine;
    [SerializeField] private Transform[] diveUnderPath;

    [Header("End Reeling")]
    [SerializeField] private string reelingEndedTrigger;
    [SerializeField] private float startEndAnimationDelay = 1.5f;
    [SerializeField] private float triggerIdleDelay;
    [SerializeField] private float surfaceDuration = 0.75f;
    [SerializeField] private float diveEndDuration = 2f;
    [SerializeField] private float endReelingDiveUnderY;
    [SerializeField] private float stayUnderWaterDuration = 3f;
    [SerializeField] private Transform reelingEndedUnderwaterPos;

    private bool arrivedAtInitialPosition;
    private int numberOfAttackUsagesSinceLastReeling;
    
    private Vector3 positionBeforeReeling;
    private Vector3[] diveUnderPathPositions;

    private Transform monsterTransform;
    private MonsterAnimationController monsterAnimationController;
    private GameStateManager gameStateManager;
    
    private WaitUntil waitForReelingStopped;
    private WaitUntil waitForReachedInitialPosition;
    private WaitForSeconds triggerIdleWait;

    private Tween delayedAnimationChangeTween;
    private Tween delayedGameStateChangeTween;
    private Tween reelingStartTween;
    private Sequence reelingEndedSequence;

    private const string IdleTrigger = "Idle";

    protected override MonsterState BehaviourState => MonsterState.Reeling;

    protected override void Start()
    {
        base.Start();
        
        gameStateManager = GameStateManager.instance;
        monsterTransform = FightMonsterSingleton.instance.MonsterTransform;
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;
        
        waitForReelingStopped = new(() 
            => gameStateManager.CurrentGameState != GameState.FightReelingStation);
        
        waitForReachedInitialPosition = new(() => arrivedAtInitialPosition);
        
        triggerIdleWait = new WaitForSeconds(triggerIdleDelay);

        CreateDiveUnderPathPositionsArray();
    }
    
    private void CreateDiveUnderPathPositionsArray()
    {
        diveUnderPathPositions = new Vector3[diveUnderPath.Length + 1];
        
        for (var i = 1; i < diveUnderPathPositions.Length; i++) 
            diveUnderPathPositions[i] = diveUnderPath[i - 1].position;
    }
    
    public override float GetExecutability()
    {
        var numberOfAttackUsages = FightMonsterState.GetUsageOfMonsterState(MonsterState.Attacking);
        
        var diffCurrentAndLastReelingUsages = numberOfAttackUsages - numberOfAttackUsagesSinceLastReeling;

        return diffCurrentAndLastReelingUsages < minNumberAttacksNeededUntilNextReeling ? 0f : baseReelingExecutability;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        delayedGameStateChangeTween?.Kill();
        delayedAnimationChangeTween?.Kill();
        
        behaviourTreeManager.ToggleBlockBehaviour(true);
        
        arrivedAtInitialPosition = false;
        positionBeforeReeling = monsterTransform.position;
        
        diveUnderPathPositions[0] = positionBeforeReeling;
        
        numberOfAttackUsagesSinceLastReeling = FightMonsterState.GetUsageOfMonsterState(MonsterState.Attacking);
        
        StartReelingEntrySequence();
        
        delayedAnimationChangeTween = DOVirtual.DelayedCall(triggerReelingStartAnimationDelay,
            () => TriggerReelingAnimation(reelingStartedTrigger));
        
        delayedGameStateChangeTween = DOVirtual.DelayedCall(triggerReelingGameStateChangeDelay,
            () => gameStateManager.ChangeGameState(GameState.FightReelingStation));

        yield return delayedGameStateChangeTween.WaitForCompletion();
        yield return waitForReelingStopped;
        
        StartEndReelingAnimation();
        
        yield return triggerIdleWait;
        monsterAnimationController.SetTrigger(IdleTrigger);

        yield return waitForReachedInitialPosition;
        
        behaviourTreeManager.ToggleBlockBehaviour(false);
    }

    private void StartReelingEntrySequence()
    {
        reelingStartTween?.Kill();
        
        reelingStartTween = DOTween.Sequence();
        
        reelingStartTween = monsterTransform.DOPath(diveUnderPathPositions, diveDuration, PathType.CatmullRom)
            .SetEase(diveEase)
            .SetOptions(false, AxisConstraint.None,AxisConstraint.X | AxisConstraint.Z)
            .SetLookAt(0.01f);

        reelingStartTween.Play();
    }
    
    private void TriggerReelingAnimation(string triggerName) => monsterAnimationController.SetTrigger(triggerName);

    private void StartEndReelingAnimation()
    {
        reelingEndedSequence?.Kill();
        reelingStartTween?.Kill();
        delayedGameStateChangeTween?.Kill();
        delayedAnimationChangeTween?.Kill();

        TriggerReelingAnimation(reelingEndedTrigger);
        
        reelingEndedSequence = DOTween.Sequence();

        reelingEndedSequence.AppendInterval(startEndAnimationDelay);

        reelingEndedSequence.Append(
            monsterTransform.DOMoveY(endReelingDiveUnderY, diveEndDuration)
                .SetEase(diveEase)
                .SetRelative(true));
        
        reelingEndedSequence.Append(DOVirtual.DelayedCall(0.01f, 
            SetMonsterPositionToUnderwaterPosition));
        
        reelingEndedSequence.AppendInterval(stayUnderWaterDuration);
        
        reelingEndedSequence.Append(monsterTransform.DOMove(positionBeforeReeling, surfaceDuration)
            .SetEase(diveEase)
            .OnComplete(() => arrivedAtInitialPosition = true));

        reelingEndedSequence.Play();
    }

    private void SetMonsterPositionToUnderwaterPosition() => monsterTransform.position = reelingEndedUnderwaterPos.position;

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        gameStateManager.ChangeGameState(GameState.FightOverview);
        
        arrivedAtInitialPosition = false;
        
        reelingEndedSequence?.Kill();
        reelingStartTween?.Kill();
        delayedGameStateChangeTween?.Kill();
        delayedAnimationChangeTween?.Kill();

        StartEndReelingAnimation();
        
        yield return waitForReachedInitialPosition;
        behaviourTreeManager.ToggleBlockBehaviour(false);
    }

    protected override void ForceStopBehaviourImpl(){}
}