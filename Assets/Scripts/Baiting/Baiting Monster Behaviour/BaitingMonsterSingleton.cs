using System;
using System.Collections.Generic;
using UnityEngine;

public class BaitingMonsterSingleton : Singleton<BaitingMonsterSingleton>
{
    [SerializeField] private MonsterSpawner spawner;
    public MonsterSpawner Spawner => spawner;
    
    [SerializeField] private MonsterRangeDefinition[] monsterRangeDefinitions;

    public override bool AddToDontDestroy => false;
    public bool PlayerIsBeingKilled { get; set; }
    public Dictionary<MonsterRange, float> MonsterRangesDict { get; private set; } = new();

    public event Action MonsterWasRepelledEvent;
    public event Action<Transform> MonsterStartedKillEvent;

    public override void OnCreated()
    {
        base.OnCreated();

        CreateMonsterRangesDict();
    }

    private void CreateMonsterRangesDict()
    {
        foreach (var monsterRangeDefinition in monsterRangeDefinitions) 
            MonsterRangesDict.Add(monsterRangeDefinition.Range, monsterRangeDefinition.Distance);
    }

    public void MonsterWasRepelled()
    {
        if (PlayerIsBeingKilled) return;
        
        MonsterWasRepelledEvent?.Invoke();
    }
    
    public void InvokeMonsterStartedKill(Transform playerAttackRepTrans) 
        => MonsterStartedKillEvent?.Invoke(playerAttackRepTrans);
}