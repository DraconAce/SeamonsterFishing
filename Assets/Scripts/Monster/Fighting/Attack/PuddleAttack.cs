using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleAttack : AbstractAttackNode
{
    [SerializeField] private GameObject puddlePrefab;
    
    public override MonsterAttackType AttackType => MonsterAttackType.LongRange;

    private readonly WaitForSeconds stopBehaviourBufferWait = new(0.5f);

    private bool puddleAttackEnded;
    private WaitUntil puddleAttackEndedWait;
    
    private PrefabPool puddlePrefabPool;
    
    private Queue<GameObject> activePuddleQueue = new();

    protected override void Start()
    {
        base.Start();
        
        puddleAttackEndedWait = new WaitUntil(() => puddleAttackEnded);

        puddlePrefabPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, puddlePrefab, transform);
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        puddleAttackEnded = false;
        
        Debug.Log("Beam Started");
        yield return new WaitForSeconds(3f);
        Debug.Log("Beam Ended");
        
        
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        Debug.Log("Beam Stopped");
        
        //trigger idle animation
        monsterAnimationController.SetTrigger(idleAnimationTriggerID);

        yield return stopBehaviourBufferWait;
    }
    
    public override float GetExecutability() => 100f;
}