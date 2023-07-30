using System;
using System.Collections.Generic;
using UnityEngine;

public class FightMonsterState : AbstractMonsterState
{
    [SerializeField] private int weakPointsHitsToKill = 3;
    
    private int weakPointHits;
    private GameStateManager gameStateManager;

    private readonly Dictionary<MonsterState, int> monsterStateUsageDict = new(); //monsterstate to usage count

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        
        MonsterStateChangedEvent += OnMonsterStateChanged;
    }

    private void OnMonsterStateChanged(MonsterState newState)
    {
        if (monsterStateUsageDict.TryAdd(newState, 1)) return;
        
        var currentStateUsage = monsterStateUsageDict[newState];
        monsterStateUsageDict[newState] = currentStateUsage + 1;
    }

    public int GetUsageOfMonsterState(MonsterState state)
    {
        return monsterStateUsageDict.TryGetValue(state, out var usage) ? usage : 0;
    }

    public void WeakPointHit()
    {
        weakPointHits--;

        if (!MonsterIsDefeated()) return;
        
        CurrentState = MonsterState.Dead;
        gameStateManager.ChangeGameState(GameState.Won);
    }

    public bool MonsterIsDefeated() => weakPointHits >= weakPointsHitsToKill;
}