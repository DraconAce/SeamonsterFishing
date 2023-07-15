using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]
[RequireComponent(typeof(MonsterSpawnBehaviour))]
public class MonsterRepelledChecker : MonoBehaviour
{
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

        //Todo: Repelled Animation
        //Todo: despawn after repelled animation ended

        soundPlayer.PlayRepelledSound();

        var position = transform.position;
        var forward = transform.forward;
        
        var towardsPlayerPos = position + forward * 6f;
        var repelledPosition = position + -forward * 10;

        var repelledSequence = DOTween.Sequence();

        repelledSequence.Append(transform.DOLocalMove(towardsPlayerPos, 1.5f)
            .SetRelative(true)
            .SetEase(Ease.InQuad));

        repelledSequence.AppendInterval(0.5f);

        repelledSequence.Append(transform.DOLocalMove(repelledPosition, 1f)
            .SetRelative(true)
            .SetEase(Ease.InExpo));

        repelledSequence.OnComplete(() => spawnBehaviour.DespawnMonster());

        repelledSequence.Play();
    }

    private void OnDestroy() => UnsubscribeFromWasRepelledEvent();
}