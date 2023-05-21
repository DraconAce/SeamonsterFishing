using System;
using UnityEngine;

public class FightMonsterState : AbstractMonsterState
{
    [SerializeField] private int weakPointsHitsToKill = 3;
    
    private int weakPointHits;
    private GameStateManager gameStateManager;

    private void Start() => gameStateManager = GameStateManager.instance;

    public void WeakPointHit()
    {
        weakPointHits--;

        if (!MonsterIsDefeated()) return;
        
        gameStateManager.ChangeGameState(GameState.Won);
    }

    public bool MonsterIsDefeated() => weakPointHits >= weakPointsHitsToKill;
}