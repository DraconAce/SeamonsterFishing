using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]
[RequireComponent(typeof(MonsterSpawnBehaviour))]
public class MonsterRepelledChecker : MonoBehaviour
{
    [SerializeField] private float delayBeforeDespawn = 5f;

    private Tween despawnDelayTween;
    
    private MonsterSoundPlayer soundPlayer;
    private MonsterSpawnBehaviour spawnBehaviour;
    private BaitingMonsterState monsterState;
    private BaitingMonsterSingleton monsterSingleton;

    private void Start()
    {
        TryGetComponent(out spawnBehaviour);
        TryGetComponent(out monsterState);
        TryGetComponent(out soundPlayer);
        
        monsterSingleton = BaitingMonsterSingleton.instance;

        SubscribeToWasRepelledEvent();
        
        spawnBehaviour.MonsterSpawnedEvent += SubscribeToWasRepelledEvent;
        spawnBehaviour.MonsterDespawnedEvent += UnsubscribeFromWasRepelledEvent;
    }

    private void SubscribeToWasRepelledEvent() => monsterSingleton.MonsterWasRepelledEvent += OnMonsterWasRepelled;

    private void UnsubscribeFromWasRepelledEvent() => monsterSingleton.MonsterWasRepelledEvent -= OnMonsterWasRepelled;

    private void OnMonsterWasRepelled()
    {
        if (monsterState.CurrentState != MonsterState.Attacking) return;
        
        Debug.Log("Repelled");
        monsterState.CurrentState = MonsterState.Repelled;

        soundPlayer.PlayRepelledSound();
        
        despawnDelayTween?.Kill();
        despawnDelayTween = DOVirtual.DelayedCall(delayBeforeDespawn, () => spawnBehaviour.DespawnMonster());
    }

    private void OnDestroy() => UnsubscribeFromWasRepelledEvent();
}