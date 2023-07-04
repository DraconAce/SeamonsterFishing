using System.Collections;
using UnityEngine;

public class MonsterReelingBehaviour : AbstractMonsterBehaviour
{
    private WaitUntil waitForReelingStopped;
    private GameStateManager gameStateManager;
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Reeling;

    protected override void Start()
    {
        base.Start();
        
        gameStateManager = GameStateManager.instance;
        
        waitForReelingStopped = new(() 
            => gameStateManager.CurrentGameState != GameState.FightReelingStation);
    }

    protected override IEnumerator StartBehaviourImpl()
    {
        gameStateManager.ChangeGameState(GameState.FightReelingStation);
        
        yield return waitForReelingStopped;

        yield return base.StartBehaviourImpl();
    }

    protected override IEnumerator StartInterruptedRoutineImpl()
    {
        gameStateManager.ChangeGameState(GameState.FightOverview);
        
        yield break;
    }
}