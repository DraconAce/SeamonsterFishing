using System;
using UnityEngine;

public class MonsterSpawnBehaviour : MonoBehaviour, IPoolObject
{
    public event Action MonsterSpawnedEvent;
    public MonsterSpawner MonsterSpawner { get; set; }

    public PoolObjectContainer ContainerOfObject { get; set; }
    
    public void ResetInstance() {}

    public void OnInstantiation(PoolObjectContainer container) => MonsterSpawnedEvent?.Invoke();

    public void DespawnMonster() => ContainerOfObject.SourcePool.ReturnInstance(ContainerOfObject);

    public void OnReturnInstance() => MonsterSpawner.MonsterDespawned();
}