using System;
using UnityEngine;

public class BaitingMonsterSingleton : Singleton<BaitingMonsterSingleton>
{
    [SerializeField] private MonsterSpawner spawner;
    public MonsterSpawner Spawner => spawner;

    public override bool AddToDontDestroy => false;
    public bool PlayerIsBeingKilled { get; set; }

    public event Action MonsterWasRepelledEvent;
    public event Action<Transform> MonsterStartedKillEvent;
    
    public void MonsterWasRepelled()
    {
        if (PlayerIsBeingKilled) return;
        
        MonsterWasRepelledEvent?.Invoke();
    }
    
    public void InvokeMonsterStartedKill(Transform playerAttackRepTrans) 
        => MonsterStartedKillEvent?.Invoke(playerAttackRepTrans);
}