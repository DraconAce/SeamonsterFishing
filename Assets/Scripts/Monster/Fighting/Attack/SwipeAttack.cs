using System.Collections;
using UnityEngine;

public class SwipeAttack : AbstractAttackNode
{
    public override float GetExecutability()
    {
        return 100f;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Swipe Started");
        yield return new WaitForSeconds(3f);
        Debug.Log("Swipe Ended");
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Swipe Stopped");
    }

    public override MonsterAttackType AttackType => MonsterAttackType.MidRange;
}