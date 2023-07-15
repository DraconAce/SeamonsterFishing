using System;
using UnityEngine;

[RequireComponent(typeof(MonsterApproachManager))]

public class MonsterSpawnBehaviour : MonoBehaviour, IPoolObject
{
    [SerializeField] private MaterialSwitcher matSwitcher;

    private MonsterApproachManager monsterApproachManager;
    
    public event Action MonsterSpawnedEvent;
    public event Action MonsterDespawnedEvent;

    private MonsterSpawner monsterSpawner;
    public PoolObjectContainer ContainerOfObject { get; set; }

    private void Awake() => TryGetComponent(out monsterApproachManager);

    private void Start() => monsterSpawner = BaitingMonsterSingleton.instance.Spawner;

    public void ResetInstance()
    {
        MonsterSpawnedEvent?.Invoke();
        matSwitcher.SwitchMaterial(0);
        
        monsterApproachManager.StartApproach();
    }

    public void OnInstantiation(PoolObjectContainer container) { }

    public void DespawnMonster()
    {
        MonsterDespawnedEvent?.Invoke();
        ContainerOfObject.SourcePool.ReturnInstance(ContainerOfObject);
    }

    public void OnReturnInstance() => monsterSpawner.MonsterDespawned();
}