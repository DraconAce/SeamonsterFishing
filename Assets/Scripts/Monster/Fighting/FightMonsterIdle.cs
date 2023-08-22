using System.Collections;
using UnityEngine;

public class FightMonsterIdle : AbstractMonsterBehaviour
{
    private MonsterAnimationController monsterAnimationController;
    
    private readonly WaitForSeconds idleEndBuffer = new(2f);
    private const string IdleTrigger = "Idle";
    
    protected override MonsterState BehaviourState => MonsterState.Idle;

    protected override void Start()
    {
        base.Start();

        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        monsterAnimationController.SetTrigger(IdleTrigger);
        
        yield return idleEndBuffer;
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield break;
    }

    public override float GetExecutability() => executability.GetRandomBetweenLimits();

    protected override void ForceStopBehaviourImpl(){}
}