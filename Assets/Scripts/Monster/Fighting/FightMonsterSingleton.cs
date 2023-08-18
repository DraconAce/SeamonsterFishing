using System;
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
    [SerializeField] private AbstractAttackNode swipeAttack;

    public override bool AddToDontDestroy => false;
    
    public Transform MonsterTransform => monsterTransform;
    public Transform ReelingTarget => reelingTarget;

    private FightMonsterBehaviourTreeManager behaviourTreeManager;

    public FightMonsterBehaviourTreeManager BehaviourTreeManager
    {
        get
        {
            if(behaviourTreeManager != null)
                return behaviourTreeManager;
            
            behaviourTreeManager = monsterKI.BehaviourTreeManager;
            return behaviourTreeManager;
        }
    }

    public event Action MonsterWeakpointWasHitEvent;
    

    //[SerializeField] private bool triggerMonsterWasHit;
    //
    //private void Update()
    //{
    //    if (!triggerMonsterWasHit) return;
    //    
    //    triggerMonsterWasHit = false;
    //    WeakPointWasHit();
    //}

    public void WeakPointWasHit()
    {
        MonsterWeakpointWasHitEvent?.Invoke();

        TriggerMonsterStunnedBehaviour();
    }

    private void TriggerMonsterStunnedBehaviour()
    {
        behaviourTreeManager.TryForceStopCurrentBehaviour();
        fightMonsterStunned.TriggerBehaviourDirectly();
    }

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

    private void TriggerMonsterReaction() => swipeAttack.TriggerBehaviourDirectly();
}