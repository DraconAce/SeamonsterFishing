using UnityEngine;

public class FightMonsterSingleton : Singleton<FightMonsterSingleton>
{
    [SerializeField] private Transform monsterTransform;
    [SerializeField] private MonsterFightBehaviourProvider fightBehaviourProvider;
    [SerializeField] private FightMonsterState fightState;

    public override bool AddToDontDestroy => false;
    public Transform MonsterTransform => monsterTransform;

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