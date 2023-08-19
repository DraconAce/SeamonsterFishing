using System;
using System.Collections.Generic;
using UnityEngine;

public class FightMonsterState : AbstractMonsterState
{
    [SerializeField] private int weakPointsHitsToKill = 3;
    
    private int weakPointHits;
    private FightMonsterSingleton fightMonsterSingleton;

    private readonly Dictionary<MonsterState, int> monsterStateUsageDict = new(); //monsterstate to usage count

    private void Start()
    {
        fightMonsterSingleton = FightMonsterSingleton.instance;
        
        fightMonsterSingleton.MonsterWeakpointWasHitEvent += WeakPointHit;
    }

    protected override void OnStateChangeRequest(MonsterState requestedChange)
    {
        if (monsterStateUsageDict.TryAdd(requestedChange, 1)) return;
        
        var currentStateUsage = monsterStateUsageDict[requestedChange];
        monsterStateUsageDict[requestedChange] = currentStateUsage + 1;
    }

    public int GetUsageOfMonsterState(MonsterState state)
    {
        return monsterStateUsageDict.TryGetValue(state, out var usage) ? usage : 0;
    }

    public void WeakPointHit()
    {
        weakPointHits++;

        if (!MonsterIsDefeated()) return;
        
        CurrentState = MonsterState.Dead;
    }

    public bool MonsterIsDefeated() => weakPointHits >= weakPointsHitsToKill;

    private void OnDestroy()
    {
        if(fightMonsterSingleton == null) return;
        
        fightMonsterSingleton.MonsterWeakpointWasHitEvent -= WeakPointHit;
    }
}