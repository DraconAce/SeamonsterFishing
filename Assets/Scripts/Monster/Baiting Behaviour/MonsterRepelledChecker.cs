using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]
[RequireComponent(typeof(MonsterSpawnBehaviour))]
public class MonsterRepelledChecker : MonoBehaviour
{
    private MonsterSpawnBehaviour spawnBehaviour;
    private BaitingMonsterState monsterState;
    private BaitingMonsterSingleton monsterSingleton;

    private bool isInRepelRange;

    private void Start()
    {
        TryGetComponent(out spawnBehaviour);
        TryGetComponent(out monsterState);
        
        monsterSingleton = BaitingMonsterSingleton.instance;

        SubscribeToWasRepelledEvent();
        
        spawnBehaviour.MonsterSpawnedEvent += SubscribeToWasRepelledEvent;
        spawnBehaviour.MonsterDespawnedEvent += UnsubscribeFromWasRepelledEvent;
    }

    private void SubscribeToWasRepelledEvent() => monsterSingleton.MonsterWasRepelledEvent += OnMonsterWasRepelled;

    private void UnsubscribeFromWasRepelledEvent()
    {
        monsterSingleton.MonsterWasRepelledEvent -= OnMonsterWasRepelled;
        isInRepelRange = false;
    }

    private void OnMonsterWasRepelled()
    {
        Debug.Log("Repelled");
        monsterState.CurrentState = MonsterState.Repelled;

        //Todo: Repelled Animation
        //Todo: despawn after repelled animation ended

        var repelledPosition = transform.position + -transform.forward * 10;
        
        transform.DOLocalMove(repelledPosition, 0.5f)
            .SetRelative(true)
            .SetEase(Ease.InExpo)
            .OnComplete(() => spawnBehaviour.DespawnMonster());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("RepelRange")) return;
        isInRepelRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("RepelRange")) return;
        isInRepelRange = false;
    }

    private void OnDestroy() => UnsubscribeFromWasRepelledEvent();
}