using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]

public class MonsterAttackManager : MonoBehaviour
{
    [SerializeField] private MinMaxLimit delayBeforeAttack = new(0.5f, 3f);

    private float lurkSoundLength;
    
    private Sequence lurkSequence;
    
    private MonsterSoundPlayer soundPlayer;
    private PlayerKilledChecker killedChecker;
    
    private BaitingMonsterState monsterState;
    private BaitingMonsterSingleton monsterSingleton;
    private DifficultyProgressionManager difficultyManager;

    private void Awake() => TryGetComponent(out monsterState);

    private void Start()
    {
        monsterSingleton = BaitingMonsterSingleton.instance;
        difficultyManager = DifficultyProgressionManager.instance;

        TryGetComponent(out soundPlayer);
        TryGetComponent(out killedChecker);
        
        lurkSoundLength = soundPlayer.GetLurkSoundLength();

        monsterSingleton.MonsterWasRepelledEvent += OnMonsterWasRepelled;
    }

    public void StartAttack()
    {
        soundPlayer.PlayLurkSound();
        monsterState.CurrentState = MonsterState.Attacking;
        
        StartAttackAnnouncementSequence();
    }

    [Header("Lurk Animation")] 
    [SerializeField] private float moveTowardsPlayerDistance = 6f;
    [SerializeField] private float moveAwayDistance = 7f;
    [SerializeField] private TweenSettings moveTowardsPlayerSettings;
    [SerializeField] private float monsterWaitTime = 0.5f;
    [SerializeField] private TweenSettings moveAwaySettings;

    private Tween repelledTween;

    private void StartAttackAnnouncementSequence()
    {
        var monsterTrans = transform;
        
        repelledTween?.Kill();
        lurkSequence?.Kill();
        
        lurkSequence = DOTween.Sequence();

        lurkSequence.Append(CreateMoveTowardsPlayerTween(monsterTrans));

        lurkSequence.AppendInterval(monsterWaitTime);
        
        lurkSequence.Append(CreateMoveAwayFromPlayerTween(monsterTrans));
        
        lurkSequence.Append(CreateKillDelayTween());

        lurkSequence.Play();
    }

    private Tween CreateMoveTowardsPlayerTween(Transform monsterTrans)
    {
        var towardsPlayerPos = CreateTowardsPlayerPosition(monsterTrans);
        
        return monsterTrans.DOLocalMove(towardsPlayerPos, moveTowardsPlayerSettings.Duration)
            .SetRelative(true)
            .SetEase(moveTowardsPlayerSettings.TweenEase);
    }

    private Vector3 CreateTowardsPlayerPosition(Transform monsterTrans)
    {
        var position = monsterTrans.position;
        var forward = monsterTrans.forward;
        
        var towardsPlayerPos = position + forward * moveTowardsPlayerDistance;
        return towardsPlayerPos;
    }

    private Tween CreateMoveAwayFromPlayerTween(Transform monsterTrans)
    {
        var originalPosition = monsterTrans.position;
        
        var moveAwayPosition = CreateMoveAwayPosition(monsterTrans);
        
        return monsterTrans.DOLocalMove(moveAwayPosition, moveAwaySettings.Duration)
            .SetRelative(true)
            .SetEase(moveAwaySettings.TweenEase)
            .OnComplete(() => monsterTrans.position = originalPosition);
    }

    private Vector3 CreateMoveAwayPosition(Transform monsterTrans)
    {
        var monsterPosition = monsterTrans.position;

        var forward = CreateMoveAwayDirection(monsterTrans.forward, -1);
        var right = CreateMoveAwayDirection(monsterTrans.right, -1);
        var up = CreateMoveAwayDirection(monsterTrans.up, -1);
        
        return monsterPosition + forward + right + up;
    }

    private Vector3 CreateMoveAwayDirection(Vector3 monsterNormal, float direction)
    {
        return monsterNormal.normalized * (direction * moveAwayDistance);
    }

    private Tween CreateKillDelayTween()
    {
        var killDelay = delayBeforeAttack.GetRandomBetweenLimits(1f, difficultyManager.DifficultyFraction) +
                        lurkSoundLength;
        
        return DOVirtual.DelayedCall(killDelay,
            () => killedChecker.StartKillingPlayer(), false);
    }

    private void OnMonsterWasRepelled()
    {
        lurkSequence?.Kill();
        repelledTween?.Kill();

        repelledTween = CreateMoveAwayFromPlayerTween(transform);
    }

    private void OnDestroy()
    {
        lurkSequence?.Kill();
        
        monsterSingleton.MonsterWasRepelledEvent -= OnMonsterWasRepelled;
    }
}