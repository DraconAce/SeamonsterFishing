using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]

public class MonsterAttackManager : MonoBehaviour
{
    private Tween attackTween;
    
    private MonsterSoundPlayer soundPlayer;
    private PlayerKilledChecker killedChecker;
    
    private BaitingMonsterState monsterState;
    private BaitingMonsterSingleton monsterSingleton;

    private void Awake() => TryGetComponent(out monsterState);

    private void Start()
    {
        monsterSingleton = BaitingMonsterSingleton.instance;

        TryGetComponent(out soundPlayer);
        TryGetComponent(out killedChecker);

        monsterSingleton.MonsterWasRepelledEvent += OnMonsterWasRepelled;
    }

    public void StartAttack()
    {
        soundPlayer.PlayAttackSound();
        monsterState.CurrentState = MonsterState.Attacking;

        attackTween = DOVirtual.DelayedCall(3f, () => killedChecker.StartKillingPlayer(), false);
    }

    private void OnMonsterWasRepelled() => attackTween?.Kill();

    private void OnDestroy()
    {
        attackTween?.Kill();
        
        monsterSingleton.MonsterWasRepelledEvent -= OnMonsterWasRepelled;
    }
}