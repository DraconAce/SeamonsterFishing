using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]

public class MonsterAttackManager : MonoBehaviour
{
    [SerializeField] private MinMaxLimit delayBeforeAttack = new(0.5f, 3f);
    
    private Tween attackTween;
    
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

        monsterSingleton.MonsterWasRepelledEvent += OnMonsterWasRepelled;
    }

    public void StartAttack()
    {
        soundPlayer.PlayAttackSound();
        monsterState.CurrentState = MonsterState.Attacking;

        attackTween = DOVirtual.DelayedCall(delayBeforeAttack.GetRandomBetweenLimits(1f, difficultyManager.DifficultyFraction),
            () => killedChecker.StartKillingPlayer(), false);
    }

    private void OnMonsterWasRepelled() => attackTween?.Kill();

    private void OnDestroy()
    {
        attackTween?.Kill();
        
        monsterSingleton.MonsterWasRepelledEvent -= OnMonsterWasRepelled;
    }
}