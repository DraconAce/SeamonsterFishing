using UnityEngine;

public class FightMonsterSingleton : Singleton<FightMonsterSingleton>
{
    [SerializeField] private Transform monsterTransform;
    [SerializeField] private Transform reelingTarget;
    
    [Header("Components to provide")]
    [SerializeField] private FightMonsterState fightState;
    public FightMonsterState FightState => fightState;

    [SerializeField] private MonsterKI monsterKI;
    public MonsterKI MonsterKI => monsterKI;
    
    [SerializeField] private MonsterAnimationController monsterAnimationController;
    public MonsterAnimationController MonsterAnimationController => monsterAnimationController;

    [Header("Behaviours With Direct Start")]
    [SerializeField] private FightMonsterStunned fightMonsterStunned;
    [SerializeField] private SwipeAttack swipeAttack;

    public override bool AddToDontDestroy => false;
    
    public Transform MonsterTransform => monsterTransform;
    public Transform ReelingTarget => reelingTarget;

    public FightMonsterBehaviourTreeManager BehaviourTreeManager { get; private set; }

    private void Start() => BehaviourTreeManager = monsterKI.BehaviourTreeManager;

    public void WeakPointWasHit()
    {
        fightState.WeakPointHit();

        TriggerMonsterStunnedBehaviour();
    }

    private void TriggerMonsterStunnedBehaviour() => fightMonsterStunned.TriggerBehaviourDirectly();

    public void FlashWasUsed() => TriggerMonsterStunnedBehaviour();

    public void CannonBallMissed()
    {
        if (!CanMonsterReactionBeTriggered()) return;

        TriggerMonsterReaction();
    }

    private bool CanMonsterReactionBeTriggered()
    {
        return fightState.CurrentState != MonsterState.Stunned 
               && !BehaviourTreeManager.IsAnyBehaviourActive;
    }

    private void TriggerMonsterReaction()
    {
        swipeAttack.TriggerBehaviourDirectly();
    }
}