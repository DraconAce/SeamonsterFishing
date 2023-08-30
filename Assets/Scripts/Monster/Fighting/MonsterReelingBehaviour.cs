using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MonsterReelingBehaviour : AbstractMonsterBehaviour
{
    [Header("Reeling Implementation")] 
    [SerializeField] private int minNumberAttacksNeededUntilNextReeling = 3;

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
    private Quaternion rotationBeforeReeling;
    private Vector3[] diveUnderPathPositions;

    private Transform monsterTransform;
    private MonsterAnimationController monsterAnimationController;
    private GameStateManager gameStateManager;
    
    private WaitUntil waitForReelingStarted;
    private WaitUntil waitForReelingStopped;
    private WaitUntil waitForReachedInitialPosition;
    private WaitForSeconds triggerIdleWait;

    private Tween delayedAnimationChangeTween;
    private Tween delayedGameStateChangeTween;
    private Tween reelingStartTween;
    private Sequence reelingEndedSequence;

    private FightMonsterSingleton monsterSingleton;

    private const string IdleTrigger = "Idle";

    protected override MonsterState BehaviourState => MonsterState.Reeling;

    protected override void Start()
    {
        base.Start();
        
        monsterSingleton = FightMonsterSingleton.instance;
        
        gameStateManager = GameStateManager.instance;
        monsterTransform = FightMonsterSingleton.instance.MonsterTransform;
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;
        
        waitForReelingStarted = new(() 
            => gameStateManager.CurrentGameState == GameState.FightReelingStation);
        
        waitForReelingStopped = new(() 
            => gameStateManager.CurrentGameState != GameState.FightReelingStation && gameStateManager.CurrentGameState != GameState.Pause);
        
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

        var reelingExecutability = diffCurrentAndLastReelingUsages < minNumberAttacksNeededUntilNextReeling ? 0f : executability.GetRandomBetweenLimits();
        
        return reelingExecutability;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        delayedGameStateChangeTween?.Kill();
        delayedAnimationChangeTween?.Kill();
        
        behaviourTreeManager.ToggleBlockBehaviour(true);
        monsterSingleton.SetBlockFlash(true);
        
        arrivedAtInitialPosition = false;
        positionBeforeReeling = monsterTransform.position;
        rotationBeforeReeling = monsterTransform.rotation;
        
        diveUnderPathPositions[0] = positionBeforeReeling;
        
        numberOfAttackUsagesSinceLastReeling = FightMonsterState.GetUsageOfMonsterState(MonsterState.Attacking);
        
        StartReelingEntrySequence();
        
        delayedAnimationChangeTween = DOVirtual.DelayedCall(triggerReelingStartAnimationDelay,
            () => TriggerReelingAnimation(reelingStartedTrigger), false);
        
        delayedGameStateChangeTween = DOVirtual.DelayedCall(triggerReelingGameStateChangeDelay,
            () =>
            {
                gameStateManager.BlockGameStateChangeWithExceptions = false;
                gameStateManager.ChangeGameState(GameState.FightReelingStation);
            }, false);

        yield return delayedGameStateChangeTween.WaitForCompletion();
        
        if(delayedAnimationChangeTween.IsActive())
            yield return delayedAnimationChangeTween.WaitForCompletion();
        
        yield return waitForReelingStarted;
        
        yield return null;
        yield return waitForReelingStopped;
        
        StartEndReelingAnimation();
        
        yield return triggerIdleWait;
        monsterAnimationController.SetTrigger(IdleTrigger);

        yield return waitForReachedInitialPosition;
        
        OnReelingFinished();
    }

    private void OnReelingFinished()
    {
        behaviourTreeManager.ToggleBlockBehaviour(false);
        monsterSingleton.SetBlockFlash(false);
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
            SetMonsterPositionToUnderwaterPosition, false));
        
        reelingEndedSequence.AppendInterval(stayUnderWaterDuration);
        
        reelingEndedSequence.Append(monsterTransform.DOMove(positionBeforeReeling, surfaceDuration)
            .SetEase(diveEase)
            .OnComplete(() => arrivedAtInitialPosition = true));

        reelingEndedSequence.Join(monsterTransform.DORotateQuaternion(rotationBeforeReeling, surfaceDuration)
            .SetEase(diveEase));

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
        
        OnReelingFinished();
    }

    protected override void ForceStopBehaviourImpl()
    {
        if(gameStateManager.CurrentGameState == GameState.FightReelingStation)
            gameStateManager.ChangeGameState(GameState.FightOverview);
        
        reelingEndedSequence?.Kill();
        reelingStartTween?.Kill();
        delayedGameStateChangeTween?.Kill();
        delayedAnimationChangeTween?.Kill();
        
        monsterSingleton.SetBlockFlash(false);
    }
}