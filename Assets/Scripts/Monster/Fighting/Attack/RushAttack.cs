using System.Collections;
using UnityEngine;

public class RushAttack : AbstractAttackNode
{
    public override MonsterAttackType AttackType => MonsterAttackType.ShortRange;

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Rush Started");
        yield return new WaitForSeconds(3f);
        Debug.Log("Rush Ended");
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Rush Stopped");
    }
    
    public override float GetExecutability() => 100f;
}