using System.Collections;
using UnityEngine;

public class BeamAttack : AbstractAttackNode
{
    public override MonsterAttackType AttackType => MonsterAttackType.LongRange;

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Beam Started");
        yield return new WaitForSeconds(3f);
        Debug.Log("Beam Ended");
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Beam Stopped");
    }
    
    public override float GetExecutability() => 100f;
}