using UnityEngine;

public class FightMonsterSingleton : Singleton<FightMonsterSingleton>
{
    [SerializeField] private Transform monsterTransform;
    
    [Header("Components to provide")]
    [SerializeField] private FightMonsterState fightState;
    public FightMonsterState FightState => fightState;

    [SerializeField] private MonsterKI monsterKI;
    public MonsterKI MonsterKI => monsterKI;

    [Header("Behaviours With Direct Start")]
    [SerializeField] private FightMonsterStunned fightMonsterStunned;
    [SerializeField] private SwipeAttack swipeAttack;

    public override bool AddToDontDestroy => false;
    public Transform MonsterTransform => monsterTransform;

    public void WeakPointWasHit()
    {
        fightState.WeakPointHit();

        TriggerMonsterStunnedBehaviour();
    }

    private void TriggerMonsterStunnedBehaviour() => fightMonsterStunned.TriggerBehaviourDirectly();

    public void FlashWasUsed() => TriggerMonsterStunnedBehaviour();

    public void CannonBallMissed()
    {
        if (fightState.CurrentState == MonsterState.Stunned) return;
        
        swipeAttack.TriggerBehaviourDirectly();
    }
}