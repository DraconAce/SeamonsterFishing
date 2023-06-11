using System;
using UnityEngine;

[RequireComponent(typeof(MonsterApproachManager))]

public class MonsterSpawnBehaviour : MonoBehaviour, IPoolObject
{
    private MonsterApproachManager monsterApproachManager;
    
    public event Action MonsterSpawnedEvent;
    public MonsterSpawner MonsterSpawner { get; set; }
    public PoolObjectContainer ContainerOfObject { get; set; }

    private void Awake() => TryGetComponent(out monsterApproachManager);

    public void ResetInstance()
    {
        MonsterSpawnedEvent?.Invoke();
        monsterApproachManager.StartApproach();
    }

    public void OnInstantiation(PoolObjectContainer container) { }

    public void DespawnMonster() => ContainerOfObject.SourcePool.ReturnInstance(ContainerOfObject);

    public void OnReturnInstance() => MonsterSpawner.MonsterDespawned();
}