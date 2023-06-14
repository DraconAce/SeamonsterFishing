using System;
using UnityEngine;

public class BaitingMonsterSingleton : Singleton<BaitingMonsterSingleton>
{
    [SerializeField] private MonsterSpawner spawner;
    public MonsterSpawner Spawner => spawner;

    public override bool AddToDontDestroy => false;
    public bool PlayerIsBeingKilled { get; set; }

    public event Action MonsterWasRepelledEvent;

    public void MonsterWasRepelled()
    {
        if (PlayerIsBeingKilled) return;
        
        MonsterWasRepelledEvent?.Invoke();
    }
}