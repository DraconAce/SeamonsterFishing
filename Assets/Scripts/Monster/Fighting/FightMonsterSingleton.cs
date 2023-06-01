using UnityEngine;

public class FightMonsterSingleton : Singleton<FightMonsterSingleton>
{
    [SerializeField] private MonsterFightBehaviourProvider fightBehaviourProvider;
    [SerializeField] private FightMonsterState fightState;

    public override bool AddToDontDestroy => false;

    public void WeakPointWasHit()
    {
        fightState.WeakPointHit();
        
        fightBehaviourProvider.ReactToStunned();
    }

    public void FlashWasUsed() => fightBehaviourProvider.ReactToStunned();

    public void CannonBallMissed()
    {
        if (fightState.CurrentState == MonsterState.Stunned) return;
        
        fightBehaviourProvider.ReactToCannonBallMiss();
    }
}